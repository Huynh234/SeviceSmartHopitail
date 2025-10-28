using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Infomation;
using SeviceSmartHopitail.Schemas.TK;
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

        public (int, string?) Register(string username, string email, string password)
        {
            if (_db.TaiKhoans.Any(x => x.Email == email))
            {
                var tk = _db.TaiKhoans.FirstOrDefault(x => x.Email == email);
                if (!tk.Status)
                {
                    string otp2 = _mailService.GenerateOTP();
                    tk.OtpExpireAt = DateTime.Now.AddMinutes(5);
                    tk.UserName = username; // cập nhật tên nếu muốn
                    tk.PasswordHash = _mailService.Hash(password);
                    tk.OtpHash = _mailService.Hash(otp2);
                    tk.LockStatus = true;
                    _db.SaveChanges();
                    _mailService.SendEmail(email, "Xác thực tài khoản", $"Xin chào {username},\nOTP: {otp2}\nHết hạn sau 5 phút.");
                    return (0, "Tài khoản chưa xác thực. Vui lòng xác minh email.");
                }
                
                if(tk.Status)
                {
                    return (2, "Tài khoản đã tồn tại");
                }

                if (!tk.LockStatus){
                    return (2, "Tài khoản của bạn đã bị khóa. Liên hệ admin để mở khóa.");
                }

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
                Status = false,
                LockStatus = true
            };

            _db.TaiKhoans.Add(user);
            _db.SaveChanges();

            _mailService.SendEmail(email, "Xác thực tài khoản", $"Xin chào {username},\nOTP: {otp}\nHết hạn sau 5 phút.");
            Console.WriteLine("Đăng ký thành công. OTP đã gửi qua email.");
            return (1, "Đăng ký thành công. OTP đã gửi qua email.");
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

            if(user.LockStatus == false)
            {
                return (false, "Tài khoản bị khóa liên hệ admin.");
            }

            if (user.OtpExpireAt < DateTime.Now)
            {
                Console.WriteLine("OTP đã hết hạn.");
                return (false, "OTP đã hết hạn.");
            }

            if (user.OtpHash == _mailService.Hash(inputOtp))
            {
                user.Status = true;
                user.LockStatus = true;
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
            if (user.LockStatus == false)
            {
                return (false, "Tài khoản bị khóa liên hệ admin.");
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
            if (user.LockStatus == false)
            {
                return (false, "Tài khoản bị khóa liên hệ admin.");
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
            if (user.LockStatus == false)
            {
                return (false, "Tài khoản bị khóa liên hệ admin.");
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

        public async Task<(LoginReply, string?, int, string?)> LoginAsync(string email, string password)
        {
            var rep = new LoginReply();
            string? Token = null;
            if (!email.Equals("adminLaAnhHuynh@gmail.com")){
                var user = await _db.TaiKhoans.FirstOrDefaultAsync(u => u.Email == email);

                if (user != null && user.Status != true)
                {
                    var tk = new LoginReply
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        Role = "user",
                        check = false,
                        Pf = -1
                    };

                    string otp = _mailService.GenerateOTP();
                    user.OtpHash = _mailService.Hash(otp);
                    user.OtpExpireAt = DateTime.Now.AddMinutes(5);
                    await _db.SaveChangesAsync();
                    _mailService.SendEmail(email, "OTP mới", $"OTP mới: {otp}\nHết hạn sau 5 phút.");
                    Console.WriteLine("OTP mới đã gửi.");
                    return (tk, string.Empty, 0, "tài khoản chưa xác thực");
                }

                if (user == null || user.LockStatus != true)
                    return (new LoginReply(), string.Empty, 2, "Tài khoản không tồn tại hoặc sai mật khẩu/Tài khoản bị khóa"); // Không tồn tại hoặc chưa active

                // So sánh password hash
                if (user.PasswordHash != _mailService.Hash(password))
                    return (new LoginReply(), string.Empty, 2,"Tài khoản không tồn tại hoặc sai mật khẩu/Tài khoản bị khóa"); // Sai mật khẩu
                var iss = await _db.UserProfiles.Where(x => x.TaiKhoanId.Equals(user.Id)).Select(x => x.Check).FirstOrDefaultAsync();
                var pfe = await _db.UserProfiles.Where(x => x.TaiKhoanId.Equals(user.Id)).Select(x => x.HoSoId).FirstOrDefaultAsync();

                var ue = new LoginReply{
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Role = "user",
                    check = iss,
                    Pf = pfe
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
                       check = true,
                       Pf = 0

                   };
                    Token = _jwt.GenerateToken("mai_la_em_anh_huynh", "admin");
                }
                rep = ue;
            }
            return (rep, Token, 1, "Đăng nhập thành công");
        }

    }
}
