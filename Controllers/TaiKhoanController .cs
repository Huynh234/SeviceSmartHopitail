using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Services;
using SeviceSmartHopitail.Schemas;
namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaiKhoanController : ControllerBase
    {
        private readonly TaiKhoanServices _taiKhoanService;

        public TaiKhoanController(TaiKhoanServices taiKhoanService)
        {
            _taiKhoanService = taiKhoanService;
        }

        // ================== ĐĂNG KÝ ==================
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            _taiKhoanService.Register(request.UserName, request.Email, request.Password);
            return Ok(new { message = "Đăng ký thành công. Vui lòng kiểm tra email để lấy OTP." });
        }

        // ================== XÁC THỰC OTP ==================
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            _taiKhoanService.VerifyOtp(request.Email, request.Otp);
            return Ok(new { message = "Đã xác thực OTP." });
        }

        // ================== GỬI LẠI OTP ==================
        [HttpPost("resend-otp")]
        public IActionResult ResendOtp([FromBody] ResendOtpRequest request)
        {
            _taiKhoanService.ResendOtp(request.Email);
            return Ok(new { message = "OTP mới đã được gửi." });
        }

        // ================== QUÊN MẬT KHẨU ==================
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            _taiKhoanService.ForgotPassword(request.Email);
            return Ok(new { message = "OTP reset mật khẩu đã gửi." });
        }

        // ================== ĐẶT LẠI MẬT KHẨU ==================
        [HttpPut("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _taiKhoanService.ResetPassword(request.Email, request.Otp, request.NewPassword);
            return Ok(new { message = "Đặt lại mật khẩu thành công." });
        }

        // ================== ĐĂNG NHẬP ==================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var  user = await _taiKhoanService.LoginAsync(request.Email, request.Password);
            if (user.Token == null)
                return Unauthorized(new { message = "Đăng nhập thất bại. Sai email/mật khẩu hoặc tài khoản chưa kích hoạt." });

            return Ok(user);
        }
    }
}
