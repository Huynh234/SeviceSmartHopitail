using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Services.Health;
using UglyToad.PdfPig.Graphics.Operations.PathPainting;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public ReportController(ReportService rs, BloodSugarService bs, BloodPressureService bp, HeartRateService hr, SleepService ss)
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
            object tot = report.Last();

            // Lấy phần còn lại
            List<object> da = report.Take(report.Count - 1).ToList();

            // Gói vào object Result
            return Ok(new
            {
                Data = da,
                Total = tot
            });
        }

        [Authorize(Roles = "user")]
        [HttpGet("export-pdf/{userProfileId}")]
        public async Task<IActionResult> ExportPdf(int userProfileId, [FromQuery] string start, [FromQuery] string end = "")
        {
            var data = await _rs.DataDetail(userProfileId, start, end);
            var pdfBytes = _rs.ExportToPdf(data);
            return File(pdfBytes, "application/pdf", "BaoCaoSucKhoe.pdf");
        }

        [Authorize(Roles = "user")]
        [HttpGet("chart/{userProfileId}")]
        public async Task<IActionResult> chartAll(int userProfileId, [FromQuery] string start, [FromQuery] string end = "")
        {
            var enddate = DateTime.Now;

            if (end != "")
            {
                enddate = _rs.ConvertSTD(end);
            }

            var startdate = _rs.ConvertSTD(start);

            var bp = await _bp.GetBloodPressureChartDataAsync(userProfileId, startdate, enddate);

            var bs = await _bs.GetBloodSugarChartDataAsync(userProfileId, startdate, enddate);

            var hr = await _hr.GetHeartRateChartDataAsync(userProfileId, startdate, enddate);

            var ss = await _ss.GetSleepChartDataAsync(userProfileId, startdate, enddate);

            return Ok(new { BloodPressure = bp, BloodSugar = bs, HeartRate = hr, Sleep = ss , sf = $"Start Date: {startdate}, End Date: {enddate}" });
        }

        [Authorize(Roles = "user")]
        [HttpGet("export-chart-bp/{userProfileId}")]
        public async Task<IActionResult> Exportbp(int userProfileId, [FromQuery] string start, [FromQuery] string end = "")
        {
            var enddate = DateTime.Now;

            if (end != "")
            {
                enddate = _rs.ConvertSTD(end);
            }

            var startdate = _rs.ConvertSTD(start);

            var bp = await _bp.GetBloodPressureChartDataAsync(userProfileId, startdate, enddate);

            var pdfBytes = _rs.DrawChart(bp, "Huyết áp");
            return File(pdfBytes, "application/png", "BloodPressureChart.png");
        }

        [Authorize(Roles = "user")]
        [HttpGet("export-chart-bs/{userProfileId}")]
        public async Task<IActionResult> Exportbs(int userProfileId, [FromQuery] string start, [FromQuery] string end = "")
        {
            var enddate = DateTime.Now;

            if (end != "")
            {
                enddate = _rs.ConvertSTD(end);
            }

            var startdate = _rs.ConvertSTD(start);

            var bs = await _bs.GetBloodSugarChartDataAsync(userProfileId, startdate, enddate);
            var pdfBytes = _rs.DrawChart(bs, "Đường huyết");
            return File(pdfBytes, "application/png", "BloodSugarChart.png");
        }

        [Authorize(Roles = "user")]
        [HttpGet("export-chart-hr/{userProfileId}")]
        public async Task<IActionResult> Exporthr(int userProfileId, [FromQuery] string start, [FromQuery] string end = "")
        {
            var enddate = DateTime.Now;

            if (end != "")
            {
                enddate = _rs.ConvertSTD(end);
            }

            var startdate = _rs.ConvertSTD(start);
            var hr = await _hr.GetHeartRateChartDataAsync(userProfileId, startdate, enddate);
            var pdfBytes = _rs.DrawChart(hr, "Nhịp tim");
            return File(pdfBytes, "application/png", "HeartRateChart.png");
        }

        [Authorize(Roles = "user")]
        [HttpGet("export-chart-ss/{userProfileId}")]
        public async Task<IActionResult> Exportss(int userProfileId, [FromQuery] string start, [FromQuery] string end = "")
        {
            var enddate = DateTime.Now;

            if (end != "")
            {
                enddate = _rs.ConvertSTD(end);
            }

            var startdate = _rs.ConvertSTD(start);
            var ss = await _ss.GetSleepChartDataAsync(userProfileId, startdate, enddate);
            var pdfBytes = _rs.DrawChart(ss, "Giờ ngủ");
            return File(pdfBytes, "application/png", "SleepChart.png");
        }
    }
}

