using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SeviceSmartHopitail.Services.MAIL
{
    public class MailServices
    {
        private readonly string fromEmail = "healthcare2025.work@gmail.com";
        private readonly string password;

        public MailServices(IConfiguration configuration)
        {
            password = configuration["PassMail"] ?? throw new Exception("Password for email is not configured.");
        }

        public void SendEmail(string toEmail, string subject, string code)
        {
            // Cấu hình SMTP
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            // Tạo email
            MailMessage mail = new MailMessage(fromEmail, toEmail, subject, code);

            try
            {
                smtp.Send(mail);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public (string,bool) SendEmail2(string toEmail, string subject, string body, byte[] fileBytes, string fileName)
        {
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            MailMessage mail = new MailMessage(fromEmail, toEmail, subject, body);

            // Nếu muốn nội dung email là HTML
            mail.IsBodyHtml = true;

            // ------------------------------
            //  ĐÍNH KÈM FILE TỪ BYTE[]
            // ------------------------------
            if (fileBytes != null && fileBytes.Length > 0)
            {
                var stream = new MemoryStream(fileBytes);
                var attachment = new Attachment(stream, fileName, "application/pdf"); // <- content type đúng
                mail.Attachments.Add(attachment);
            }


            try
            {
                smtp.Send(mail);
                return ("Email sent successfully!", true);
            }
            catch (Exception ex)
            {
                return (("Error: " + ex.Message), false);
            }
        }


        public string GenerateOTP(int length = 6)
        {
            Random rand = new Random();
            string otp = "";
            for (int i = 0; i < length; i++)
                otp += rand.Next(0, 10).ToString();
            return otp;
        }

        public string GenerateCaptcha(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            Random rand = new Random();
            StringBuilder captcha = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int index = rand.Next(chars.Length);
                captcha.Append(chars[index]);
            }

            return captcha.ToString();
        }

        public string Hash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
