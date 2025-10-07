using Newtonsoft.Json;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using SeviceSmartHopitail.Models;

namespace SeviceSmartHopitail.Services
{
    public class CloudinaryServices
    {
        private readonly Cloudinary cloudinary = new Cloudinary(
            new Account("dm9tzekzx", "258271866169244", "oLIbS35IyesuA-Vv3gTRPQxZhes")
        );

        public async Task<string> UploadImg(MemoryStream memoryStream, string fileName)
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

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Uploaded: {uploadResult.SecureUrl}");
                return uploadResult.SecureUrl?.ToString() ?? "";
            }
            else
            {
                Console.WriteLine($"Upload failed: {uploadResult.Error?.Message}");
                return "";
            }
        }
    }
}
