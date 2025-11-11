using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Services.Health;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        public readonly ReportService _rs;
        public readonly BloodSugarService _bs;
        public readonly BloodPressureService _bp;
        public readonly HeartRateService _hr;
        public readonly SleepService _ss;
        public ReportController(ReportService rs, BloodSugarService bs,BloodPressureService bp,HeartRateService hr,SleepService ss)
        {
            _rs = rs;
            _bs = bs;
            _bp = bp;
            _hr = hr;
            _ss = ss;
        }

        [Authorize(Roles = "user")]
        [HttpGet("getdata-table/{userProfileId}")]
        public async Task<IActionResult> GenerateReport(int userProfileId, [FromQuery] string Start, [FromQuery] string? end)
        {
            var report = await _rs.DataDetail(userProfileId, Start, end ?? "");
            if (report == null)
            {
                return NotFound(new { message = "Không thể tạo báo cáo cho người dùng này." });
            }
            return Ok(report);
        }

        [Authorize(Roles = "user")]
        [HttpGet("export-pdf/{userProfileId}")]
        public async Task<IActionResult> ExportPdf(int userProfileId,[FromQuery] string start, [FromQuery] string end = "")
        {
            var data = await _rs.DataDetail(userProfileId, start, end);
            var pdfBytes = _rs.ExportToPdf(data);
            return File(pdfBytes, "application/pdf", "BaoCaoSucKhoe.pdf");
        }

        [Authorize(Roles = "user")]
        [HttpGet("chart/{userProfileId}/{type}")]
        public async Task<IActionResult> ExportPdf(int userProfileId, string type, [FromQuery] string start, [FromQuery] string end = "")
        {
            var enddate = _rs.ConvertSTD(end);
            var startdate = _rs.ConvertSTD(start);
            if (type == "BloodPressure")
            {
                var chart = await _bp.GetBloodPressureChartDataAsync(userProfileId, startdate, enddate);
                return Ok(chart);
            }
            else if (type == "BloodSugar")
            {
                var chart = await _bs.GetBloodSugarChartDataAsync(userProfileId, startdate, enddate);
                return Ok(chart);
            }
            else if (type == "HeartRate")
            {
                var chart = await _hr.GetHeartRateChartDataAsync(userProfileId, startdate, enddate);
                return Ok(chart);
            }
            else if (type == "Sleep")
            {
                var chart = await _ss.GetSleepChartDataAsync(userProfileId, startdate, enddate);
                return Ok(chart);
            }
            else
            {
                return BadRequest(new { message = "Loại biểu đồ không hợp lệ." });
            }
        }
    }
}
