using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Schemas.HR;
using SeviceSmartHopitail.Services.Health;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BloodSugarController : ControllerBase
    {
        private readonly BloodSugarService _service;

        public BloodSugarController(BloodSugarService service)
        {
            _service = service;
        }


        /// Lấy dữ liệu đường huyết hôm nay của người dùng

        [HttpGet("today/{userProfileId}")]
        public async Task<IActionResult> GetToday(int userProfileId)
        {
            var result = await _service.GetTodayAsync(userProfileId);
            if (result == null)
                return NotFound(new { message = "Chưa có dữ liệu đường huyết hôm nay." });

            return Ok(result);
        }


        /// Lấy dữ liệu đường huyết hôm qua

        [HttpGet("yesterday/{userProfileId}")]
        public async Task<IActionResult> GetYesterday(int userProfileId)
        {
            var result = await _service.GetYesterdayAsync(userProfileId);
            if (result == null)
                return NotFound(new { message = "Chưa có dữ liệu đường huyết hôm qua." });

            return Ok(result);
        }


        /// Tạo bản ghi đường huyết mới cho hôm nay

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBloodSugarRecord model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var record = await _service.CreateAsync(model);
                return Ok(new
                {
                    message = "Tạo bản ghi đường huyết thành công.",
                    record
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }


        /// Cập nhật bản ghi đường huyết hôm nay

        [HttpPut("update/{userProfileId}")]
        public async Task<IActionResult> Update(int userProfileId, [FromBody] UpdateBloodSugarR model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _service.UpdateTodayAsync(userProfileId, model);
            if (record == null)
                return NotFound(new { message = "Không tìm thấy bản ghi đường huyết hôm nay để cập nhật." });

            return Ok(new
            {
                message = "Cập nhật đường huyết thành công.",
                record
            });
        }


        /// So sánh đường huyết hôm nay và hôm qua

        [HttpGet("compare/{userProfileId}")]
        public async Task<IActionResult> CompareTodayWithYesterday(int userProfileId)
        {
            var todayData = await _service.GetTodayAsync(userProfileId);
            var yesterdayData = await _service.GetYesterdayAsync(userProfileId);

            if (todayData == null || yesterdayData == null)
                return NotFound(new { message = "Không đủ dữ liệu để so sánh đường huyết." });

            var todayRecord = (todayData as dynamic).Record as BloodSugarRecord;
            var yesterdayRecord = (yesterdayData as dynamic).Record as BloodSugarRecord;

            string result = _service.CompareWithPrevious(todayRecord, yesterdayRecord);
            return Ok(new { message = result });
        }


        /// Lấy dữ liệu biểu đồ đường huyết trong 1 tháng gần nhất

        [HttpGet("chart/{userProfileId}")]
        public async Task<IActionResult> GetChartData(int userProfileId)
        {
            var data = await _service.GetBloodSugarChartDataAsync(userProfileId);
            if (data == null)
                return NotFound(new { message = "Không đủ dữ liệu để vẽ biểu đồ." });

            return Ok(data);
        }
        [HttpGet("average/{userProfileId}")]
        public async Task<IActionResult> GetAverage(int userProfileId, [FromQuery] int days = 30)
        {
            var result = await _service.GetAverageBloodSugarAsync(userProfileId, days);
            if (result == null)
                return NotFound(new { message = $"Không có dữ liệu đường huyết trong {days} ngày gần nhất." });

            return Ok(result);
        }

    }
}
