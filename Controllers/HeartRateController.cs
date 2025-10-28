using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Models.Health;
using SeviceSmartHopitail.Schemas.HR;
using SeviceSmartHopitail.Services.Health;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeartRateController : ControllerBase
    {
        private readonly HeartRateService _service;

        public HeartRateController(HeartRateService service)
        {
            _service = service;
        }

        /// Lấy dữ liệu nhịp tim hôm nay của người dùng
        [HttpGet("today/{userProfileId}")]
        public async Task<IActionResult> GetToday(int userProfileId)
        {
            var result = await _service.GetTodayAsync(userProfileId);
            if (result == null)
                return NotFound(new { message = "Chưa có dữ liệu nhịp tim hôm nay." });

            return Ok(result);
        }

        /// Lấy dữ liệu nhịp tim hôm qua
        [HttpGet("yesterday/{userProfileId}")]
        public async Task<IActionResult> GetYesterday(int userProfileId)
        {
            var result = await _service.GetYesterdayAsync(userProfileId);
            if (result == null)
                return NotFound(new { message = "Chưa có dữ liệu nhịp tim hôm qua." });

            return Ok(result);
        }

        /// Tạo bản ghi nhịp tim mới cho hôm nay
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateHeartRateRecord model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var record = await _service.CreateAsync(model);
                return Ok(new
                {
                    message = "Tạo bản ghi nhịp tim thành công.",
                    record
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// Cập nhật bản ghi nhịp tim hôm nay
        [HttpPut("update/{userProfileId}")]
        public async Task<IActionResult> Update(int userProfileId, [FromBody] UpdateHeartRateR model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _service.UpdateTodayAsync(userProfileId, model);
            if (record == null)
                return NotFound(new { message = "Không tìm thấy bản ghi nhịp tim hôm nay để cập nhật." });

            return Ok(new
            {
                message = "Cập nhật nhịp tim thành công.",
                record
            });
        }

        /// So sánh nhịp tim hôm nay và hôm qua
        [HttpGet("compare/{userProfileId}")]
        public async Task<IActionResult> CompareTodayWithYesterday(int userProfileId)
        {
            var todayData = await _service.GetTodayAsync(userProfileId);
            var yesterdayData = await _service.GetYesterdayAsync(userProfileId);

            if (todayData == null || yesterdayData == null)
                return NotFound(new { message = "Không đủ dữ liệu để so sánh nhịp tim." });

            var todayRecord = (todayData as dynamic).Record as HeartRateRecord;
            var yesterdayRecord = (yesterdayData as dynamic).Record as HeartRateRecord;

            string result = _service.CompareWithPrevious(todayRecord, yesterdayRecord);
            return Ok(new { message = result });
        }

        /// Lấy dữ liệu biểu đồ nhịp tim trong 1 tháng gần nhất
        [HttpGet("chart/{userProfileId}")]
        public async Task<IActionResult> GetChartData(int userProfileId)
        {
            var data = await _service.GetHeartRateChartDataAsync(userProfileId);
            if (data == null)
                return NotFound(new { message = "Không đủ dữ liệu để vẽ biểu đồ." });

            return Ok(data);
        }

        // ===================== API: Lấy trung bình nhịp tim =====================
        [HttpGet("average/{userProfileId}")]
        public async Task<IActionResult> GetAverageHeartRate(int userProfileId)
        {
            var average = await _service.GetAverageHeartRateAsync(userProfileId);

            if (average == 0)
            {
                return NotFound(new { 
                    Message = "Không có dữ liệu nhịp tim trong 30 ngày gần nhất.",
                    UserProfileId = userProfileId 
                });
            }

            return Ok(new
            {
                UserProfileId = userProfileId,
                AverageHeartRate = Math.Round(average, 2),
                Period = "30 ngày gần nhất"
            });
        }
        [HttpGet("Recently/{userProfileId}")]
        public async Task<IActionResult> GetRecent(int userProfileId)
        {
            var result = await _service.GetRecentlyAsync(userProfileId);
            if (result == null)
                return NotFound(new { message = $"Không có dữ liệu nhịp tim nào" });

            return Ok(result);
        }
    }
}
