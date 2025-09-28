using Newtonsoft.Json;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using SeviceSmartHopitail.Models;

namespace SeviceSmartHopitail.Services
{
    public class CloudinaryServices
    {
        private Cloudinary cloudinary = new Cloudinary(@"cloudinary://258271866169244:oLIbS35IyesuA-Vv3gTRPQxZhes@dm9tzekzx");
        public async Task<string> uploadImg(MemoryStream memoryStream, string fileName, string fileNames)
        {
            cloudinary.Api.Secure = true;
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, memoryStream),
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };
            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            var getResourceParams = new GetResourceParams(fileNames)
            {
                QualityAnalysis = true
            };
            var getResourceResult = await cloudinary.GetResourceAsync(getResourceParams);
            var resultJson = getResourceResult.JsonObj;

            var qualityAnalysisJson = resultJson["quality_analysis"];
            if (qualityAnalysisJson != null)
            {
                var asset = JsonConvert.DeserializeObject<CloudinaryImage>(resultJson.ToString());
                Console.WriteLine(asset?.SecureUrl);
                return asset?.SecureUrl ?? "";
            }
            else
            {
                Console.WriteLine("Error");
                return "";
            }
        }
    }
}
