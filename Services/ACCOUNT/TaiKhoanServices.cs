using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Schemas;
using SeviceSmartHopitail.Services.MAIL;

namespace SeviceSmartHopitail.Services
{
    public class TaiKhoanServices
    {
        private readonly AppDbContext _db;
        private readonly MailServices _mailService;
        private readonly JWTServices _jwt;

        public TaiKhoanServices(AppDbContext db, MailServices mailService, JWTServices jwt)
        {
            _db = db;
            _mailService = mailService;
            _jwt = jwt;
        }

        public (bool, string?) Register(string username, string email, string password)
        {
            if (_db.TaiKhoans.Any(x => x.Email == email))
            {
                Console.WriteLine("Email đã tồn tại!");
                return (false, "Email đã tồn tại!");
            }

            string otp = _mailService.GenerateOTP();
            string otpHash = _mailService.Hash(otp);

            var user = new TaiKhoan
            {
                UserName = username,
                Email = email,
                PasswordHash = _mailService.Hash(password),
                OtpHash = otpHash,
                OtpExpireAt = DateTime.Now.AddMinutes(5),
                Status = false
            };

            _db.TaiKhoans.Add(user);
            _db.SaveChanges();

            _mailService.SendEmail(email, "Xác thực tài khoản", $"Xin chào {username},\nOTP: {otp}\nHết hạn sau 5 phút.");
            Console.WriteLine("Đăng ký thành công. OTP đã gửi qua email.");
            return (true, "Đăng ký thành công. OTP đã gửi qua email.");
        }

        // ================== VERIFY OTP ==================
        public (bool, string) VerifyOtp(string email, string inputOtp)
        {
            var user = _db.TaiKhoans.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                Console.WriteLine("Email không tồn tại.");
                return (false, "Email không tồn tại.");
            }

            if (user.OtpExpireAt < DateTime.Now)
            {
                Console.WriteLine("OTP đã hết hạn.");
                return (false, "OTP đã hết hạn.");
            }

            if (user.OtpHash == _mailService.Hash(inputOtp))
            {
                user.Status = true;
                user.OtpHash = "";
                user.OtpExpireAt = null;
                _db.SaveChanges();
                Console.WriteLine("Xác thực thành công.");
                return (true, "Xác thực thành công.");
            }
            else
            {
                Console.WriteLine("OTP không đúng.");
                return (false, "OTP không đúng.");
            }
        }

        // ================== RESEND OTP ==================
        public (bool, string) ResendOtp(string email)
        {
            var user = _db.TaiKhoans.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                Console.WriteLine("Email không tồn tại.");
                return (false, "Email không tồn tại.");
            }

            string otp = _mailService.GenerateOTP();
            user.OtpHash = _mailService.Hash(otp);
            user.OtpExpireAt = DateTime.Now.AddMinutes(5);
            _db.SaveChanges();

            _mailService.SendEmail(email, "OTP mới", $"OTP mới: {otp}\nHết hạn sau 5 phút.");
            Console.WriteLine("OTP mới đã gửi.");
            return (true, "OTP mới đã gửi.");
        }

        // ================== RESEND CAPCHA ==================
        public (bool, string?) ResendCapcha(string email)
        {
            var user = _db.TaiKhoans.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                Console.WriteLine("Email không tồn tại.");
                return (false, "Email không tồn tại.");
            }

            string otp = _mailService.GenerateCaptcha();
            user.OtpHash = _mailService.Hash(otp);
            user.OtpExpireAt = DateTime.Now.AddMinutes(3);
            _db.SaveChanges();

            _mailService.SendEmail(email, "CAPTCHA mới", $"CAPTCHA mới: {otp}\nHết hạn sau 5 phút.");
            Console.WriteLine("OTP mới đã gửi.");
            return (true, "OTP mới đã gửi.");
        }

        // ================== FORGOT PASSWORD ==================
        public (bool, string) ForgotPassword(string email)
        {
            var user = _db.TaiKhoans.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                Console.WriteLine("Email không tồn tại.");
                return (false, "Email không tồn tại.");
            }

            string otp = _mailService.GenerateCaptcha();
            user.OtpHash = _mailService.Hash(otp);
            user.OtpExpireAt = DateTime.Now.AddMinutes(3);
            _db.SaveChanges();

            _mailService.SendEmail(email, "Quên mật khẩu", $"Mã CAPTCHA xác thực reset mật khẩu: {otp}\nHết hạn sau 5 phút.");
            Console.WriteLine("OTP reset password đã gửi.");
            return (true, "CAPTCHA reset password đã gửi.");
        }

        // ================== RESET PASSWORD ==================
        public (bool,string) ResetPassword(string email, string inputOtp, string newPassword)
        {
            var user = _db.TaiKhoans.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                Console.WriteLine("Email không tồn tại.");
                return (false, "Email không tồn tại.");
            }

            if (user.OtpExpireAt < DateTime.Now)
            {
                Console.WriteLine("OTP đã hết hạn.");
                return (false, "OTP đã hết hạn.");
            }

            if (user.OtpHash == _mailService.Hash(inputOtp))
            {
                user.PasswordHash = _mailService.Hash(newPassword);
                user.OtpHash = "";
                user.OtpExpireAt = null;
                user.UpdateAt = DateTime.UtcNow;
                _db.SaveChanges();
                Console.WriteLine("Đặt lại mật khẩu thành công.");
                return( true, "Đặt lại mật khẩu thành công.");
            }
            else
            {
                Console.WriteLine("OTP không đúng.");
                return(false, "OTP không đúng");
            }
        }

        public async Task<(LoginReply, string?)> LoginAsync(string email, string password)
        {
            var rep = new LoginReply();
            string? Token = null;
            if (!email.Equals("adminLaAnhHuynh@gmail.com")){
                var user = await _db.TaiKhoans.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null || user.Status != true)
                    return (new LoginReply(), string.Empty); // Không tồn tại hoặc chưa active

                // So sánh password hash
                if (user.PasswordHash != _mailService.Hash(password))
                    return (new LoginReply(), string.Empty); // Sai mật khẩu
                var iss = await _db.UserProfiles.Where(x => x.TaiKhoanId.Equals(user.Id)).Select(x => x.Check).FirstOrDefaultAsync();


                var ue = new LoginReply{
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Role = "user",
                    
                    check = iss
                };
                Token = _jwt.GenerateToken(user.UserName, "user");
                rep = ue;
            }
            else
            {
                var ue = new LoginReply();
                if (password.Equals("emAnhHuynh"))
                {
                   ue = new LoginReply
                    {
                        Id = 0,
                        Email = "adminLaAnhHuynh@gmail.com",
                        UserName = "EM la em anh Huynh",
                        Role = "admin",
                       
                    };
                    Token = _jwt.GenerateToken("mai_la_em_anh_huynh", "admin");
                }
                rep = ue;
            }
            return (rep, Token);
        }

    }
}
