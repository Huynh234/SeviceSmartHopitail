using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Schemas;
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

            if (yesterday == null)
                return NotFound("Không có dữ liệu qua nay");

            var comparison = _healthService.CompareWithPrevious(today, yesterday);
            return Ok(new
            {
                Today = today.RecordedAt,
                Comparison = comparison
            });
        }


        // ================== GET hôm nay ==================
        [HttpGet("today/{userProfileId}")]
        public async Task<IActionResult> GetToday(int userProfileId)
        {
            var record = await _healthService.GetTodayRecordAsync(userProfileId);
            if (record == null) return NotFound("Chưa có bản ghi hôm nay.");
            return Ok(record);
        }

        // ================== GET hôm qua ==================
        [HttpGet("yesterday/{userProfileId}")]
        public async Task<IActionResult> GetYesterday(int userProfileId)
        {
            var record = await _healthService.GetYesterdayRecordAsync(userProfileId);
            if (record == null) return NotFound("Không có bản ghi hôm qua.");
            return Ok(record);
        }

        // ================== POST (tạo mới hôm nay) ==================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateHealthRecord dto)
        {
            try
            {
                var record = await _healthService.CreateTodayAsync(dto);
                return Ok(record);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ================== PUT (update hôm nay) ==================
        [HttpPut("update/{userProfileId}")]
        public async Task<IActionResult> Update(int userProfileId, [FromBody] UpdateHealthRecord dto)
        {
            var record = await _healthService.UpdateTodayAsync(userProfileId, dto);
            if (record == null) return NotFound("Không tìm thấy bản ghi hôm nay.");
            return Ok(record);
        }

        // ================== DELETE (xóa hôm nay) ==================
        [HttpDelete("delete/{userProfileId}")]
        public async Task<IActionResult> Delete(int userProfileId)
        {
            var deleted = await _healthService.DeleteTodayAsync(userProfileId);
            if (!deleted) return NotFound("Không tìm thấy bản ghi hôm nay.");
            return Ok("Xóa thành công.");
        }

        // ================== GET chart dữ liệu huyết áp ==================
        [HttpGet("chart/blood-pressure/{userProfileId}")]
        public async Task<IActionResult> GetBloodPressureChart(int userProfileId)
        {
            var data = await _healthService.GetBloodPressureChartDataAsync(userProfileId);
            if (data == null) return NotFound("Không đủ dữ liệu để vẽ biểu đồ.");
            return Ok(data);
        }

    }
}
