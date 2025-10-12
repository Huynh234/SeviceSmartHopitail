using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Messages;
using SeviceSmartHopitail.Schemas;
using SeviceSmartHopitail.Services;
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
            var (check , mess) = _taiKhoanService.Register(request.Username, request.Email, request.Password);
            if (!check)
            {
                return BadRequest(new { message = mess });
            }
            else
            {
                return Ok(new { message = mess });
            }
        }

        // ================== XÁC THỰC OTP ==================
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var (check , mess) = _taiKhoanService.VerifyOtp(request.Email, request.Otp);
            if(check){
                return Ok(new { message = mess });
            }
            else
            {
                return BadRequest(new { message = mess });
            }
        }

        // ================== GỬI LẠI OTP ==================
        [HttpPost("resend-otp")]
        public IActionResult ResendOtp([FromBody] ResendOtpRequest request)
        {
           var (check, mess) = _taiKhoanService.ResendOtp(request.Email);
            if (!check)
            {
                return BadRequest(new { message = mess });
            }
            else
            {
                return Ok(new { message = mess });
            }
        }

        // ================== GỬI LẠI OTP ==================
        [HttpPost("resend-captcha")]
        public IActionResult ResendCaptcha([FromBody] ResendOtpRequest request)
        {
            var (check, mess) = _taiKhoanService.ResendCapcha(request.Email);
            if (!check)
            {
                return BadRequest(new { message = mess });
            }
            else
            {
                return Ok(new { message = mess });
            }
        }

        // ================== QUÊN MẬT KHẨU ==================
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var (check, mess) = _taiKhoanService.ForgotPassword(request.Email);
            if (!check)
            {
                return BadRequest(new { message = mess });
            }
            else
            {
                return Ok(new { message = mess });
            }
        }

        // ================== ĐẶT LẠI MẬT KHẨU ==================
        [HttpPut("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var (check, mess) = _taiKhoanService.ResetPassword(request.Email, request.Otp, request.NewPassword);
            if (check == true)
            {
                return Ok(new { message = mess });
            }
            else
            {
                return BadRequest(new { message = mess });
            }
        }

        // ================== ĐĂNG NHẬP ==================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (user, Token) = await _taiKhoanService.LoginAsync(request.Email, request.Password);
            if (user == new LoginReply() || Token == "")
                return Unauthorized(new { message = "Đăng nhập thất bại. Sai email/mật khẩu hoặc tài khoản chưa kích hoạt." });

            return Ok(new { auth = user, token = Token });
        }
    }
}
