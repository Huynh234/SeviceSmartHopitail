using System.Threading;
using System.Threading.Tasks;

namespace SeviceSmartHopitail.Services.RAG
{
    /// <summary>
    /// High-level embedding service sử dụng OpenAIClientWrapper.
    /// </summary>
    public class EmbeddingService
    {
        private readonly IGeminiClient _client;

        public EmbeddingService(IGeminiClient client)
        {
            _client = client;
        }

        public async Task<byte[]> CreateEmbeddingBytesAsync(string text, CancellationToken ct = default)
        {
            var vec = await _client.GetEmbeddingAsync(text, ct);
            return EmbeddingUtils.FloatArrayToBytes(vec);
        }

        public async Task<float[]> CreateEmbeddingFloatAsync(string text, CancellationToken ct = default)
        {
            return await _client.GetEmbeddingAsync(text, ct);
        }
    }
}
