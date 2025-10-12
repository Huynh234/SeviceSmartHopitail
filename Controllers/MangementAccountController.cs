using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Services;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Schemas;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MangementAccountController : ControllerBase
    {
        private readonly MangementAccountServices _service;

        public MangementAccountController(MangementAccountServices service)
        {
            _service = service;
        }

        // --- Lấy tất cả tài khoản ---
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _service.GetAllAsync();
            return Ok(accounts);
        }

        // --- Lấy tài khoản theo Id hoặc Email ---
        [HttpGet("find")]
        public async Task<IActionResult> Get([FromQuery] int? id, [FromQuery] string? email)
        {
            var account = await _service.GetByIdAsync(id, email);
            if (account == null) return NotFound("Không tìm thấy tài khoản");
            return Ok(account);
        }

        // --- Xóa tài khoản ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound("Không tìm thấy tài khoản để xóa");
            return Ok("Xóa tài khoản thành công");
        }

        // --- Khóa tài khoản ---
        [HttpPut("lock/{id}")]
        public async Task<IActionResult> Lock(int id)
        {
            var result = await _service.LockAccountAsync(id);
            if (!result) return NotFound("Không tìm thấy tài khoản");
            return Ok("Tài khoản đã bị khóa");
        }

        // --- Mở khóa tài khoản ---
        [HttpPut("unlock/{id}")]
        public async Task<IActionResult> Unlock(int id)
        {
            var result = await _service.UnlockAccountAsync(id);
            if (!result) return NotFound("Không tìm thấy tài khoản");
            return Ok("Tài khoản đã được mở");
        }

        // --- Reset mật khẩu ---
        [HttpPut("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(int id, MangementRPassword dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest("Mật khẩu mới không được để trống");

            var result = await _service.ResetPasswordAsync(id, dto.NewPassword);
            if (!result) return NotFound("Không tìm thấy tài khoản");
            return Ok("Đặt lại mật khẩu thành công");
        }
        // --- Đăng ký tài khoản mới ---
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            var (check, mess) = await _service.AddMoi(request.Email, request.Username, request.Password);
            if (!check)
            {
                return BadRequest(new { message = mess });
            }
            else
            {
                return Ok(new { message = mess });
            }
        }
    }
}
