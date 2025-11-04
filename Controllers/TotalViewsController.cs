using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Writers;
using SeviceSmartHopitail.Models.Health;
using SeviceSmartHopitail.Services.Health;
namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TotalViewsController : ControllerBase
    {
        private readonly BloodPressureService _bps;
        private readonly BloodSugarService _bss;
        private readonly HeartRateService _hrs;
        private readonly SleepService _ss;

        public TotalViewsController(BloodPressureService bps, BloodSugarService bss, HeartRateService hrs, SleepService ss)
        {
            _bps = bps;
            _bss = bss;
            _hrs = hrs;
            _ss = ss;
        }

        [HttpGet("totalView/{prId}")]
        public async Task<IActionResult> GetTotalViews(int prId)
        {
            var bp = await _bps.GetRecentlyAsync(prId);
            var bs = await _bss.GetRecentlyAsync(prId);
            var hr = await _hrs.GetRecentlyAsync(prId);
            var s = await _ss.GetRecentlyAsync(prId);

            if (bp == null || bs == null || hr == null || s == null)
            {
                return NotFound("No recent health data found for the given profile ID.");
            }

            var totalViews = new
            {
                BloodPressure = bp,
                BloodSugar = bs,
                HeartRate = hr,
                Sleep = s
            };
            return Ok(totalViews);
        }

        [HttpGet("chartbloodpressure/{prId}")]
        public async Task<IActionResult> GetHealthSummary(int prId, [FromQuery] int about) // Corrected attribute name
        {
            var chat = await _bps.GetBloodPressureChartBydayAsync(prId, about);
            if (chat == null)
            {
                return NotFound("No blood pressure chart data found for the given profile ID and time frame.");
            }
            return Ok(chat);
        }

        [HttpGet("compare/{prId}")]
        public async Task<IActionResult> GetSleepSummary(int prId) // Corrected attribute name
        {
            
            var com = await  _ss.CompareWithPrevious(prId);
            var b = await _bss.Get7DaySummaryAsync(prId);
            var h = await _hrs.Get7DaySummaryAsync(prId);
            if (com == null || b == null || h == null)
            {
                return NotFound("No sleep chart data found for the given profile ID and time frame.");
            }
            return Ok(new
            {
                SleepComparison = com,
                BloodSugarSummary = b,
                HeartRateSummary = h
            });
        }

        [HttpGet("comment/{prId}")]
        public async Task<IActionResult> GetHealthComments(int prId)
        {
            var bp = await _bps.GetRecentlyAsync(prId);
            var bs = await _bss.GetRecentlyAsync(prId);
            var hr = await _hrs.GetRecentlyAsync(prId);
            var s = await _ss.GetRecentlyAsync(prId);

            if (bp == null && bs == null && hr == null && s == null)
                return NotFound("Không có dữ liệu sức khỏe gần đây.");

            var positiveComments = new List<string>();
            var warningComments = new List<string>();

            // ===== HUYẾT ÁP =====
            if (bp != null)
            {
                var bpAlert = bp.GetType().GetProperty("BloodPressureAlert")?.GetValue(bp)?.ToString();
                if (bpAlert != null && bpAlert.Contains("bình thường", StringComparison.OrdinalIgnoreCase))
                    positiveComments.Add("💙 Huyết áp của bạn đang trong mức ổn định.");
                else
                    warningComments.Add($"⚠️ Cảnh báo huyết áp: {bpAlert}");
            }

            // ===== ĐƯỜNG HUYẾT =====
            if (bs != null)
            {
                var bsAlert = bs.GetType().GetProperty("BloodSugarcord")?.GetValue(bs)?.ToString();
                if (bsAlert != null && bsAlert.Contains("bình thường", StringComparison.OrdinalIgnoreCase))
                    positiveComments.Add("🍬 Đường huyết hiện tại ở mức cân bằng tốt.");
                else
                    warningComments.Add($"⚠️ Cảnh báo đường huyết: {bsAlert}");
            }

            // ===== NHỊP TIM =====
            if (hr != null)
            {
                var hrAlert = hr.GetType().GetProperty("HeartRateAlert")?.GetValue(hr)?.ToString();
                if (hrAlert != null && hrAlert.Contains("bình thường", StringComparison.OrdinalIgnoreCase))
                    positiveComments.Add("💓 Nhịp tim ổn định và phù hợp với trạng thái cơ thể.");
                else
                    warningComments.Add($"⚠️ Cảnh báo nhịp tim: {hrAlert}");
            }

            // ===== GIẤC NGỦ =====
            if (s != null)
            {
                var sleepAlert = s.GetType().GetProperty("SleepAlert")?.GetValue(s)?.ToString();
                if (sleepAlert != null && sleepAlert.Contains("bình thường", StringComparison.OrdinalIgnoreCase))
                    positiveComments.Add("😴 Bạn đang duy trì thời gian ngủ hợp lý.");
                else
                    warningComments.Add($"⚠️ Cảnh báo giấc ngủ: {sleepAlert}");
            }

            var result = new
            {
                ProfileId = prId,
                positive = positiveComments.Any() ? positiveComments : new List<string> { "Chưa có cải thiện rõ rệt." },
                warn = warningComments.Any() ? warningComments : new List<string> { "Không có cảnh báo đáng chú ý." },
                Time = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")
            };

            return Ok(result);
        }

    }
}
