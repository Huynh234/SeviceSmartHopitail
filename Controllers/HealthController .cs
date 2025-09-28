using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Services.Profiles;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthService _healthService;

        public HealthController(HealthService healthService)
        {
            _healthService = healthService;
        }

        [HttpGet("compare/{userProfileId}")]
        public async Task<IActionResult> Compare(int userProfileId)
        {
            var today = await _healthService.GetTodayRecordAsync(userProfileId);
            var yesterday = await _healthService.GetYesterdayRecordAsync(userProfileId);

            if (today == null)
                return NotFound("Không có dữ liệu hôm nay");

            var comparison = _healthService.CompareWithPrevious(today, yesterday);
            return Ok(new
            {
                Today = today.RecordedAt,
                Comparison = comparison
            });
        }
    }
}
