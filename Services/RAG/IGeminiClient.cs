using System.Threading;
using System.Threading.Tasks;

namespace SeviceSmartHopitail.Services.RAG
{
    public interface IGeminiClient
    {
        Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default);
        Task<byte[]> GetEmbeddingBytesAsync(string text, CancellationToken ct = default);
        Task<string> GenerateTextAsync(string prompt, int maxTokens = 512, CancellationToken ct = default);
        Task<string> GenerateTextWithExternalDataAsync(string prompt, string context, CancellationToken ct = default);
    }
}
