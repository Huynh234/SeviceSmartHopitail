using DotnetGeminiSDK.Client;
using DotnetGeminiSDK.Config;
using Newtonsoft.Json;
using System.Text;

namespace SeviceSmartHopitail.Services.RAG
{
    public class GeminiClientWrapper : IGeminiClient
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly GeminiClient _geminiClient;

        public GeminiClientWrapper(IConfiguration configuration)
        {
            _apiKey = configuration["GEMINI_API_KEY"] ?? "AIzaSyDqBu83UghPGDlHd7UfgvQfKGKb_WYec_o";
            //_apiKey = "AIzaSyDUq4EMq3j9oH2TxjjP5EtfMFsplMP6LE8";
            _httpClient = new HttpClient();
            _geminiClient = new GeminiClient(new GoogleGeminiConfig
            {
                ApiKey = _apiKey,
                TextBaseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash"
            });
        }

        /// <summary>
        /// Lấy embedding từ Gemini.
        /// </summary>
        public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={_apiKey}";

            var requestBody = new
            {
                model = "gemini-embedding-001",
                content = new
                {
                    parts = new[]
                    {
                        new { text }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var resp = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), ct);
            resp.EnsureSuccessStatusCode();

            var resultJson = await resp.Content.ReadAsStringAsync(ct);
            dynamic result = JsonConvert.DeserializeObject(resultJson)!;

            var vector = result.embedding.values;
            return ((Newtonsoft.Json.Linq.JArray)vector).Select(v => (float)v).ToArray();
        }

        public async Task<byte[]> GetEmbeddingBytesAsync(string text, CancellationToken ct = default)
        {
            var floats = await GetEmbeddingAsync(text, ct);
            return EmbeddingUtils.FloatArrayToBytes(floats);
        }

        /// <summary>
        /// Sinh văn bản từ Gemini (chat Q&A tiếng Việt).
        /// </summary>
        public async Task<string> GenerateTextAsync(string prompt, int maxTokens = 1500, CancellationToken ct = default)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = maxTokens,
                    temperature = 0.7,
                    topP = 0.9
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var resp = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), ct);
            resp.EnsureSuccessStatusCode();

            var resultJson = await resp.Content.ReadAsStringAsync(ct);
            dynamic result = JsonConvert.DeserializeObject(resultJson)!;

            Console.WriteLine(result);
            // Lấy text từ response
            var text = result?.candidates[0]?.content?.parts[0]?.text;
            return text?.ToString() ?? string.Empty;
        }

        public async Task<string> GenerateTextWithExternalDataAsync(string prompt, string context, CancellationToken ct = default)
        {
            var externalService = new ExternalDataService(_httpClient);

            // 1. Lấy dữ liệu ngoài (Wikipedia chẳng hạn)
            var wikiInfo = await externalService.GetFromWikipediaAsync(prompt, ct);

            // 2. Ghép context (DB) + dữ liệu ngoài
            var finalPrompt = $@"
                                Người dùng hỏi: {prompt}

                                Dữ liệu tham khảo từ DB:
                                {context}

                                Dữ liệu tham khảo từ Wikipedia:
                                {wikiInfo}

                                Hãy tổng hợp câu trả lời chi tiết, dễ hiểu, và chính xác bằng tiếng Việt.";

            // 3. Gọi Gemini sinh câu trả lời
            return await GenerateTextAsync(finalPrompt, ct: ct);
        }

    }
}