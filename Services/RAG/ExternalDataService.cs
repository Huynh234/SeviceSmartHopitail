using Newtonsoft.Json;

namespace SeviceSmartHopitail.Services.RAG
{
    public class ExternalDataService
    {
        private readonly HttpClient _httpClient;

        public ExternalDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // lấy liệu tóm tắt từ Wikipedia
        public async Task<string> GetFromWikipediaAsync(string keyword, CancellationToken ct = default)
        {
            var url = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(keyword)}";
            var resp = await _httpClient.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return string.Empty;

            var json = await resp.Content.ReadAsStringAsync(ct);
            dynamic result = JsonConvert.DeserializeObject(json)!;
            return result.extract?.ToString() ?? string.Empty;
        }
    }
}
