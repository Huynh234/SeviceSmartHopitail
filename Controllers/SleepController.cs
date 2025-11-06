using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Health;
using SeviceSmartHopitail.Schemas.HR;
using SeviceSmartHopitail.Services.Health;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SleepController : ControllerBase
    {
        private readonly SleepService _sleepService;

        public SleepController(AppDbContext db)
        {
            _sleepService = new SleepService(db);
        }

        // ===================== Lấy bản ghi giấc ngủ hôm nay =====================
        [Authorize(Roles = "user")]
        [HttpGet("today/{userProfileId}")]
        public async Task<IActionResult> GetToday(int userProfileId)
        {
            var result = await _sleepService.GetTodayAsync(userProfileId);
            if (result == null)
                return NotFound(new { message = "Chưa có dữ liệu giấc ngủ hôm nay." });
            return Ok(result);
        }

        // ===================== Lấy bản ghi giấc ngủ hôm qua =====================
        [Authorize(Roles = "user")]
        [HttpGet("yesterday/{userProfileId}")]
        public async Task<IActionResult> GetYesterday(int userProfileId)
        {
            var result = await _sleepService.GetYesterdayAsync(userProfileId);
            if (result == null)
                return NotFound(new { message = "Không có dữ liệu giấc ngủ hôm qua." });
            return Ok(result);
        }

        // ===================== Thêm mới bản ghi giấc ngủ =====================
        [Authorize(Roles = "user")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateSleepRecord model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var record = await _sleepService.CreateAsync(model);
                return Ok(new
                {
                    message = "Đã thêm bản ghi giấc ngủ thành công.",
                    data = record
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // ===================== Cập nhật giấc ngủ hôm nay =====================
        [Authorize(Roles = "user")]
        [HttpPut("today/{userProfileId}")]
        public async Task<IActionResult> UpdateToday(int userProfileId, [FromBody] UpdateSleepR model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _sleepService.UpdateTodayAsync(userProfileId, model);
            if (updated == null)
                return NotFound(new { message = "Không tìm thấy bản ghi hôm nay để cập nhật." });

            return Ok(new
            {
                message = "Cập nhật bản ghi giấc ngủ thành công.",
                data = updated
            });
        }

        // ===================== So sánh giấc ngủ hôm nay và hôm qua =====================
        [Authorize(Roles = "user")]
        [HttpGet("compare/{userProfileId}")]
        public async Task<IActionResult> CompareSleep(int userProfileId)
        {
            var today = await _sleepService.GetTodayAsync(userProfileId);
            var yesterday = await _sleepService.GetYesterdayAsync(userProfileId);

            if (today == null || yesterday == null)
                return NotFound(new { message = "Không đủ dữ liệu để so sánh." });

            var todayRecord = ((dynamic)today).Record as SleepRecord;
            var yesterRecord = ((dynamic)yesterday).Record as SleepRecord;

            var result = _sleepService.CompareWithPrevious(todayRecord!, yesterRecord);
            return Ok(new { message = result });
        }

        // ===================== Biểu đồ giấc ngủ =====================
        [Authorize(Roles = "user")]
        [HttpGet("chart/{userProfileId}")]
        public async Task<IActionResult> GetChartData(int userProfileId)
        {
            var data = await _sleepService.GetSleepChartDataAsync(userProfileId);
            if (data == null)
                return NotFound(new { message = "Không đủ dữ liệu để vẽ biểu đồ." });
            return Ok(data);
        }

        // ===================== Trung bình giấc ngủ tháng =====================
        [Authorize(Roles = "user")]
        [HttpGet("average/{userProfileId}")]
        public async Task<IActionResult> GetAverage(int userProfileId)
        {
            var avg = await _sleepService.GetAverageSleepAsync(userProfileId);
            return Ok(new { AverageHours = avg });
        }

        [Authorize(Roles = "user")]
        [HttpGet("Recently/{userProfileId}")]
        public async Task<IActionResult> GetRecent(int userProfileId)
        {
            var result = await _sleepService.GetRecentlyAsync(userProfileId);
            if (result == null)
                return NotFound(new { message = $"Không có dữ liệu giấc ngủ nào" });

            return Ok(result);
        }
    }
}
