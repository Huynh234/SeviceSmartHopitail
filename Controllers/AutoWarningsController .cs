using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Models.Health;
using SeviceSmartHopitail.Services.Health;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutoWarningsController : ControllerBase
    {
        private readonly AWarningSevice _warningService;

        public AutoWarningsController(AWarningSevice warningService)
        {
            _warningService = warningService;
        }

        // GET: api/warnings/{userProfileId}
        [Authorize(Roles = "user")]
        [HttpGet("{userProfileId:int}")]
        public async Task<IActionResult> GetWarnings(int userProfileId, [FromQuery] string? fill = "")
        {
            var warnings = await _warningService.getID(userProfileId, fill);
            if (warnings == null || !warnings.Any())
                return Ok(new { message = "Chưa có cảnh báo nào. Hãy duy trì trong độ nhé." });

            return Ok(warnings);
        }

        // DELETE: api/warnings/{id}
        [Authorize(Roles = "user")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteWarning(int id)
        {
            var result = await _warningService.delete(id);
            if (!result)
                return NotFound(new { message = "Cảnh báo không tồn tại." });
            return Ok(new { message = "Xóa cảnh báo thành công." });
        }

        [Authorize(Roles = "user")]
        [HttpDelete("all/{pid:int}")]
        public async Task<IActionResult> DeleteWarningAll(int pid)
        {
            var result = await _warningService.deleteAll(pid);
            if (!result)
                return NotFound(new { message = "Cảnh báo không tồn tại." });
            return Ok(new { message = "Xóa cảnh báo thành công." });
        }
    }
}
