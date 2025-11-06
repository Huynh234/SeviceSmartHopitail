using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Schemas.HR;
using SeviceSmartHopitail.Services.Profiles;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarningController : ControllerBase
    {

        private readonly WarningService _warningService;

        public WarningController(WarningService warningService)
        {
            _warningService = warningService;
        }
        // Tạo cảnh báo cá nhân mới
        [Authorize(Roles = "user")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CPWarning model)
        {
            if (model == null || model.UserProfileId == null)
                return BadRequest(new { message = "Thông tin cảnh báo không hợp lệ hoặc thiếu UserProfileId." });

            var result = await _warningService.CreateAsync(model);

            if (result == null)
                return Conflict(new { message = "Cảnh báo cho UserProfileId này đã tồn tại." });

            return Ok(result);
        }
        // Cập nhật cảnh báo theo ID
        [Authorize(Roles = "user")]
        [HttpPut("{ProFileid:int}")]
        public async Task<IActionResult> Update(int ProFileid, [FromBody] CPWarning model)
        {
            if (model == null)
                return BadRequest(new { message = "Dữ liệu cập nhật không hợp lệ." });

            var updated = await _warningService.UpdateAsync(ProFileid, model);
            if (updated == null)
                return NotFound(new { message = "Không tìm thấy cảnh báo cần cập nhật." });

            return Ok(updated);
        }
        // Xóa cảnh báo theo ID
        [Authorize(Roles = "user")]
        [HttpDelete("{ProFileid:int}")]
        public async Task<IActionResult> Delete(int ProFileid)
        {
            var success = await _warningService.DeleteAsync(ProFileid);
            if (!success)
                return NotFound(new { message = "Không tìm thấy cảnh báo cần xóa." });

            return Ok(new { message = "Xóa cảnh báo thành công." });
        }
        // Lấy cảnh báo theo UserProfileId
        [Authorize(Roles = "user")]
        [HttpGet("user/{userProfileId:int}")]
        public async Task<IActionResult> GetByUserProfileId(int userProfileId)
        {
            var warning = await _warningService.GetByUserProfileIdAsync(userProfileId);
            if (warning == null)
                return NotFound(new { message = "Không tìm thấy cảnh báo cho UserProfileId này." });

            return Ok(warning);
        }
    }
}
