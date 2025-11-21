using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Services;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Schemas.TK;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _service.GetAllAsync();
            return Ok(accounts);
        }

        // --- Lấy tài khoản theo Id hoặc Email ---
        [Authorize(Roles = "admin")]
        [HttpGet("find")]
        public async Task<IActionResult> Get([FromQuery] int? id, [FromQuery] string? email)
        {
            var account = await _service.GetByIdAsync(id, email);
            if (account == null) return NotFound("Không tìm thấy tài khoản");
            return Ok(account);
        }

        // --- Xóa tài khoản ---
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound("Không tìm thấy tài khoản để xóa");
            return Ok("Xóa tài khoản thành công");
        }

        // --- Khóa tài khoản ---
        [Authorize(Roles = "admin")]
        [HttpPut("lock/{id}")]
        public async Task<IActionResult> Lock(int id)
        {
            var result = await _service.LockAccountAsync(id);
            if (!result) return NotFound("Không tìm thấy tài khoản");
            return Ok("Tài khoản đã bị khóa");
        }

        // --- Mở khóa tài khoản ---
        [Authorize(Roles = "admin")]
        [HttpPut("unlock/{id}")]
        public async Task<IActionResult> Unlock(int id)
        {
            var result = await _service.UnlockAccountAsync(id);
            if (!result) return NotFound("Không tìm thấy tài khoản");
            return Ok("Tài khoản đã được mở");
        }

        // --- Reset mật khẩu ---
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
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

        //--- Lấy dữ liệu tổng quan ---

        [Authorize(Roles = "admin")]
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _service.GetOverviewAsync();
            return Ok(overview);
        }
    }
}
