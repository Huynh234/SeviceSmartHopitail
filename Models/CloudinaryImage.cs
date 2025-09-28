using Newtonsoft.Json;

namespace SeviceSmartHopitail.Models
{
    public class CloudinaryImage
    {
        [JsonProperty("asset_id")]
        public string AssetId { get; set; } = "";

        [JsonProperty("public_id")]
        public string PublicId { get; set; } = "";

        public long Version { get; set; }

        [JsonProperty("version_id")]
        public string VersionId { get; set; } = "";

        public string Signature { get; set; } = "";

        public int Width { get; set; }

        public int Height { get; set; }

        public string Format { get; set; } = "";

        [JsonProperty("resource_type")]
        public string ResourceType { get; set; } = "";

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        public string[] Tags { get; set; } = new string[] { };

        public int Pages { get; set; }

        public int Bytes { get; set; }

        public string Type { get; set; } = "";

        public string Etag { get; set; } = "";

        public bool Placeholder { get; set; }

        public string Url { get; set; } = "";

        [JsonProperty("secure_url")]
        public string SecureUrl { get; set; } = "";

        [JsonProperty("asset_folder")]
        public string AssetFolder { get; set; } = "";

        [JsonProperty("display_name")]
        public string DisplayName { get; set; } = "";

        [JsonProperty("original_filename")]
        public string OriginalFilename { get; set; } = "";

        [JsonProperty("api_key")]
        public string ApiKey { get; set; } = "";

        [JsonProperty("quality_analysis")]
        public QualityAnalysis QualityAnalysis { get; set; } = new QualityAnalysis();
    }

    public class QualityAnalysis
    {
        public double Focus { get; set; }
    }
}
