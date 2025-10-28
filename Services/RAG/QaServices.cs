using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.AI;
using SeviceSmartHopitail.Schemas.RAG;
using System.Text;

namespace SeviceSmartHopitail.Services.RAG
{
    public class QaServices
    {
        private readonly AppDbContext _db;
        private readonly IGeminiClient _gemini;

        public QaServices(AppDbContext db, IGeminiClient gemini)
        {
            _db = db;
            _gemini = gemini;
        }
        public async Task<String> Ask(AskDto dto, CancellationToken ct)
        {
            // 1) embedding for question
            var qVec = await _gemini.GetEmbeddingAsync(dto.Question, ct);

            // 2) get candidate chunks (only those with embedding)
            var candidates = await _db.TextChunks
                .Where(t => t.Embedding != null)
                .AsNoTracking()
                .ToListAsync(ct);

            // 3) compute similarity
            var scored = new List<(TextChunk chunk, double score)>();
            foreach (var c in candidates)
            {
                try
                {
                    var emb = EmbeddingUtils.BytesToFloatArray(c.Embedding!);
                    var sim = CosineSimilarity(qVec, emb);
                    scored.Add((c, sim));
                }
                catch
                {
                    throw new Exception($"Failed to compute similarity for chunk ID {c.Id}");
                }
            }

            // 4) pick top K contexts
            var top = scored.OrderByDescending(x => x.score).Take(6).Select(x => x.chunk.Text).ToList();

            //5) construct prompt in Vietnamese
            var prompt = new StringBuilder();
            prompt.AppendLine("Bạn là một trợ lý y tế chuyên tra mã ICD-10. Trả lời bằng tiếng Việt, chính xác, ngắn gọn, thêm 2-3 thông tin chính.");
            prompt.AppendLine("Nếu trong context có mã ICD, nêu rõ mã và tìm thêm 2-3 thông tin chính cho mã bệnh. Nếu không có thông tin, hãy nói bạn không tìm thấy thông tin và tìm 3-4 thông tin liên quan từ nguồn có kiểm định.");
            prompt.AppendLine();
            prompt.AppendLine("Context (dùng nội dung dưới để trả lời và tìm kiếm thêm một số thông tin từ nguồn kiểm định để bổ sung):");
            foreach (var t in top)
            {
                prompt.AppendLine("---");
                prompt.AppendLine(t);
            }
            prompt.AppendLine("---");
            prompt.AppendLine("Hỏi: " + dto.Question);
            prompt.AppendLine("Trả lời:");

            // 6) call LLM for answer
            var answer = await _gemini.GenerateTextAsync(prompt.ToString(), maxTokens: 2000, ct);
            // 7) log
            var qlog = new QuestionLog
            { 
                TkId = dto.TkId,
                Question = dto.Question,
                Answer = answer,
                CreatedAt = dto?.Time != null  ? dto.Time.Value.ToDateTime(TimeOnly.MinValue) : DateTime.UtcNow
            };
            _db.QuestionLogs.Add(qlog);
            await _db.SaveChangesAsync(ct);

            return answer;
        }

        private static double CosineSimilarity(float[] a, float[] b)
        {
            if (a == null || b == null || a.Length == 0 || b.Length == 0) return 0;
            var n = Math.Min(a.Length, b.Length);
            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < n; i++)
            {
                dot += a[i] * b[i];
                na += a[i] * a[i];
                nb += b[i] * b[i];
            }
            var denom = Math.Sqrt(na) * Math.Sqrt(nb);
            return denom == 0 ? 0 : dot / denom;
        }

        public async Task<Object?> GetAllLogsAsync(int id)
        {
            var logs = await _db.QuestionLogs
                                  .Where(u => u.TkId == id)
                                  .OrderBy(u => u.CreatedAt)
                                  .ToListAsync();
            var result = logs.GroupBy(log => DateOnly.FromDateTime(log.CreatedAt))
                      .Select(g => g.Select(log => new
                      {
                          id = log.Id,
                          question = log.Question,
                          answer = log.Answer,
                          date = DateOnly.FromDateTime(log.CreatedAt)
                      }).Cast<object>().ToList())
                      .ToList();


            return result;
        }
    }
}
