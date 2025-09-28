using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SeviceSmartHopitail.Datas;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeviceSmartHopitail.Services.RAG
{
    public class EmbeddingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmbeddingBackgroundService> _logger;
        private readonly int _batchSize = 50;
        private readonly TimeSpan _delayBetweenBatches = TimeSpan.FromSeconds(30);

        public EmbeddingBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<EmbeddingBackgroundService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmbeddingBackgroundService started at: {time}", DateTimeOffset.Now);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessBatchAsync(stoppingToken);
                    await Task.Delay(_delayBetweenBatches, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // service stopping
            }
        }

        private async Task ProcessBatchAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var gemini = scope.ServiceProvider.GetRequiredService<IGeminiClient>();

            var chunks = await db.TextChunks
                .Where(tc => tc.Embedding == null)
                .OrderBy(tc => tc.Id)
                .Take(_batchSize)
                .ToListAsync(ct);

            if (chunks.Count == 0)
            {
                _logger.LogInformation("Không có TextChunk nào cần tạo embedding trong lần chạy này.");
                return;
            }

            _logger.LogInformation("Xử lý {Count} TextChunks để tạo embedding...", chunks.Count);

            foreach (var chunk in chunks)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    var bytes = await gemini.GetEmbeddingBytesAsync(chunk.Text, ct);

                    if (bytes == null || bytes.Length == 0)
                    {
                        _logger.LogWarning("Embedding trả về rỗng cho chunkId {Id}. Bỏ qua.", chunk.Id);
                        continue;
                    }

                    chunk.Embedding = bytes;
                    chunk.EmbeddingCreatedAt = DateTime.UtcNow;
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    _logger.LogError(ex, "Lỗi khi tạo embedding cho chunkId {Id}. Bỏ qua chunk này.", chunk.Id);
                }
            }

            try
            {
                await db.SaveChangesAsync(ct);
                _logger.LogInformation("Đã lưu embedding cho {Count} TextChunks.", chunks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu batch embedding.");
            }
        }
    }
}

