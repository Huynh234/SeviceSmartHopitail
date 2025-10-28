using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Models.Health;
using SeviceSmartHopitail.Schemas.HR;
using SeviceSmartHopitail.Services.Health;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BloodPressureController : ControllerBase
    {
        private readonly BloodPressureService _service;

        public BloodPressureController(BloodPressureService service)
        {
            _service = service;
        }


        /// Lấy dữ liệu huyết áp hôm nay của người dùng
        [HttpGet("today/{userProfileId}")]
        public async Task<IActionResult> GetToday(int userProfileId)
        {
            var result = await _service.GetTodayAsync(userProfileId);
            if (result == null) return NotFound(new { message = "Chưa có dữ liệu huyết áp hôm nay." });
            return Ok(result);
        }


        /// Lấy dữ liệu huyết áp hôm qua
        [HttpGet("yesterday/{userProfileId}")]
        public async Task<IActionResult> GetYesterday(int userProfileId)
        {
            var result = await _service.GetYesterdayAsync(userProfileId);
            if (result == null) return NotFound(new { message = "Chưa có dữ liệu huyết áp hôm qua." });
            return Ok(result);
        }


        /// Thêm bản ghi huyết áp mới cho hôm nay
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBloodPressureRecord model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var record = await _service.CreateAsync(model);
                return Ok(new { message = "Tạo bản ghi huyết áp thành công.", record });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }


        /// Cập nhật bản ghi huyết áp hôm nay
        [HttpPut("update/{userProfileId}")]
        public async Task<IActionResult> Update(int userProfileId, [FromBody] UpdateBloodPressureR model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _service.UpdateTodayAsync(userProfileId, model);
            if (record == null)
                return NotFound(new { message = "Không tìm thấy bản ghi huyết áp hôm nay để cập nhật." });

            return Ok(new { message = "Cập nhật huyết áp thành công.", record });
        }


        /// So sánh huyết áp hôm nay và hôm qua
        [HttpGet("compare/{userProfileId}")]
        public async Task<IActionResult> CompareTodayWithYesterday(int userProfileId)
        {
            var todayData = await _service.GetTodayAsync(userProfileId);
            var yesterdayData = await _service.GetYesterdayAsync(userProfileId);

            if (todayData == null || yesterdayData == null)
                return NotFound(new { message = "Không đủ dữ liệu để so sánh huyết áp." });

            var todayRecord = (todayData as dynamic).Record as BloodPressureRecord;
            var yesterdayRecord = (yesterdayData as dynamic).Record as BloodPressureRecord;

            string result = _service.CompareWithPrevious(todayRecord, yesterdayRecord);
            return Ok(new { message = result });
        }


        /// Lấy dữ liệu biểu đồ huyết áp trong 1 tháng gần nhất
        [HttpGet("chart/{userProfileId}")]
        public async Task<IActionResult> GetChartData(int userProfileId)
        {
            var data = await _service.GetBloodPressureChartDataAsync(userProfileId);
            if (data == null) return NotFound(new { message = "Không đủ dữ liệu để vẽ biểu đồ." });
            return Ok(data);
        }
        [HttpGet("average/{userProfileId}")]
        public async Task<IActionResult> GetAverage(int userProfileId, [FromQuery] int days = 30)
        {
            var result = await _service.GetAverageBloodPressureAsync(userProfileId, days);
            if (result == null)
                return NotFound(new { message = $"Không có dữ liệu huyết áp trong {days} ngày gần nhất." });

            return Ok(result);
        }

        [HttpGet("Recently/{userProfileId}")]
        public async Task<IActionResult> GetRecent(int userProfileId)
        {
            var result = await _service.GetRecentlyAsync(userProfileId);
            if (result == null)
                return NotFound(new { message = $"Không có dữ liệu huyết áp nào" });

            return Ok(result);
        }

    }
}
