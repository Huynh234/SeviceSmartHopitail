using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Schemas;
using SeviceSmartHopitail.Services.Profiles;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly UserProfileSevices _profileServices;

        public UserProfileController(UserProfileSevices profileServices)
        {
            _profileServices = profileServices;
        }

        // GET api/UserProfile/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var profile = await _profileServices.GetByIdAsync(id);
            if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ" });
            return Ok(profile);
        }

        // GET api/UserProfile/taiKhoan/5
        [HttpGet("taiKhoan/{taiKhoanId}")]
        public async Task<IActionResult> GetByTaiKhoanId(int taiKhoanId)
        {
            var profile = await _profileServices.GetByTaiKhoanIdAsync(taiKhoanId);
            if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ theo tài khoản" });
            return Ok(profile);
        }

        // POST api/UserProfile
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateUserProfile profile, IFormFile? avatar)
        {
            MemoryStream? stream = null;
            if (avatar != null)
            {
                stream = new MemoryStream();
                await avatar.CopyToAsync(stream);
                stream.Position = 0;
            }

            var created = await _profileServices.CreateAsync(profile, stream ?? new MemoryStream());
            return CreatedAtAction(nameof(GetById), new { id = created.HoSoId }, created);
        }

        [HttpPut("/update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] CreateUserProfile profile, IFormFile? avatar)
        {
            MemoryStream? stream = null;
            if (avatar != null)
            {
                stream = new MemoryStream();
                await avatar.CopyToAsync(stream);
                stream.Position = 0;
            }

            var updated = await _profileServices.UpdateAsync(profile, stream ?? new MemoryStream(), id);
            if (updated == null) return NotFound(new { message = "Không tìm thấy hồ sơ để cập nhật" });

            return Ok(updated);
        }

        // PATCH api/UserProfile/avatar/5
        [HttpPatch("avatar-updload/{id}")]
        public async Task<IActionResult> UpdateAvatar(int id, IFormFile avatar)
        {
            if (avatar == null) return BadRequest(new { message = "Chưa chọn ảnh" });

            using var stream = new MemoryStream();
            await avatar.CopyToAsync(stream);
            stream.Position = 0;

            var result = await _profileServices.UpdatedAvatarAsync(id, stream, avatar.FileName);
            if (result == null) return NotFound(new { message = "Không tìm thấy hồ sơ" });
            if (result == false) return BadRequest(new { message = "Upload thất bại" });

            return Ok(new { message = "Cập nhật avatar thành công" });
        }

        // DELETE api/UserProfile/5
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _profileServices.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Không tìm thấy hồ sơ để xóa" });

            return Ok(new { message = "Xóa thành công" });
        }
    }
}
