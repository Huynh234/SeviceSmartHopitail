using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Health;
using SeviceSmartHopitail.Models.Infomation;
using SeviceSmartHopitail.Schemas.HR;
using SeviceSmartHopitail.Services.Profiles;
using System.Globalization;

namespace SeviceSmartHopitail.Services.Health
{
    public class SleepService
    {
        private readonly AppDbContext _db;
        private readonly HealthAlertService _alertService;

        public SleepService(AppDbContext db)
        {
            _db = db;
            _alertService = new HealthAlertService();
        }

        public static DateTime ConvertStringToDateTime(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                throw new ArgumentException("Invalid time string");

            string[] formats = {
            "ddd MMM dd yyyy HH:mm:ss 'GMT'K",
            "ddd MMM d yyyy HH:mm:ss 'GMT'K"
        };

            if (DateTime.TryParseExact(
                    timeString,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal,
                    out DateTime result))
            {
                return result;
            }

            // fallback nếu format không khớp hoàn toàn
            return DateTime.Parse(timeString, CultureInfo.InvariantCulture);
        }

        public static decimal CalculateSleepHours(DateTime sleepTime, DateTime wakeTime)
        {
            var duration = (wakeTime - sleepTime).TotalHours;
            if (duration < 0)
                throw new InvalidOperationException("Giờ thức dậy phải sau giờ đi ngủ.");

            return Math.Round((decimal)duration, 2);
        }


        public async Task<object?> GetTodayAsync(int userProfileId)
        {
            var today = DateTime.Now.Date;
            var tomorow = today.AddDays(1);
            var record = await _db.SleepRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new { Record = record, SleepAlert = _alertService.GetSleepAlert(record.HoursSleep, pri) };
        }

        public async Task<object?> GetYesterdayAsync(int userProfileId)
        {
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var record = await _db.SleepRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= yesterday && r.RecordedAt < today)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new { Record = record, SleepAlert = _alertService.GetSleepAlert(record.HoursSleep, pri) };
        }

        public async Task<SleepRecord> CreateAsync(CreateSleepRecord model)
        {
            var today = DateTime.Now.Date;
            bool exists = await _db.SleepRecords
                .AnyAsync(r => r.UserProfileId == model.UserProfileId && r.RecordedAt >= today);
            if (exists)
                throw new InvalidOperationException("Hôm nay đã có bản ghi giấc ngủ.");

            var sleepTim = ConvertStringToDateTime(model.TimeSleep);
            var wakeTim = ConvertStringToDateTime(model.TimeWake);
            var hoursSlee = CalculateSleepHours(sleepTim, wakeTim);

            var rec = new SleepRecord
            {
                UserProfileId = model.UserProfileId,
                SleepTime = sleepTim,
                WakeTime = wakeTim,
                HoursSleep = hoursSlee,
                Note = model.Note,
                RecordedAt = DateTime.Now
            };

            _db.SleepRecords.Add(rec);
            await _db.SaveChangesAsync();
            return rec;
        }

        public async Task<SleepRecord?> UpdateTodayAsync(int userProfileId, UpdateSleepR model)
        {
            var today = DateTime.Now.Date;
            var tomorow = today.AddDays(1);
            var record = await _db.SleepRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            var sleepTim = ConvertStringToDateTime(model.TimeSleep);
            var wakeTim = ConvertStringToDateTime(model.TimeWake);
            var hoursSlee = CalculateSleepHours(sleepTim, wakeTim);

            record.SleepTime = sleepTim;
            record.WakeTime = wakeTim;
            record.HoursSleep = hoursSlee;
            record.Note = model.Note;
            record.RecordedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return record;
        }

        // ===================== So sánh giấc ngủ =====================
        public async Task<object> CompareWithPrevious(int ProID)
        {
            var toda = DateTime.Now.Date;
            var tomorow = toda.AddDays(1);
            var today = await _db.SleepRecords
                .Where(r => r.UserProfileId == ProID && r.RecordedAt >= toda && r.RecordedAt < tomorow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            var yesterday = toda.AddDays(-1);
            var prev = await _db.SleepRecords
                .Where(r => r.UserProfileId == ProID && r.RecordedAt >= yesterday && r.RecordedAt < toda)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (prev == null || today == null)
                return "Không có dữ liệu";

            // So sánh thời lượng ngủ
            var diffHours = today.HoursSleep - prev.HoursSleep;
            var percent = (diffHours / prev.HoursSleep) * 100;

            string sleepDurationCompare;
            if (diffHours > 0)
                sleepDurationCompare = $"Thời gian ngủ tăng {diffHours:F1}h (+{percent:F1}%)";
            else if (diffHours < 0)
                sleepDurationCompare = $"Thời gian ngủ giảm {-diffHours:F1}h ({percent:F1}%)";
            else
                sleepDurationCompare = "Thời gian ngủ giữ nguyên";

            // So sánh giờ bắt đầu ngủ
            var timeDiff = (today.SleepTime - prev.SleepTime).TotalHours;
            string sleepTimeCompare;

            if (Math.Abs(timeDiff) < 0.25) // chênh < 15 phút
                sleepTimeCompare = "Giờ đi ngủ gần như giống hôm trước";
            else if (timeDiff > 0)
                sleepTimeCompare = $"Hôm nay đi ngủ muộn hơn {timeDiff:F1}h";
            else
                sleepTimeCompare = $"Hôm nay đi ngủ sớm hơn {-timeDiff:F1}h";

            // Gộp kết quả
            return new
            {
                SleepTime = sleepTimeCompare,
                SleepDuration = sleepDurationCompare,
                time = today.HoursSleep
            };
        }

        // ===================== Biểu đồ giấc ngủ =====================
        public async Task<object?> GetSleepChartDataAsync(int userProfileId)
        {
            var now = DateTime.Now;
            var oneMonthAgo = now.AddMonths(-1);

            var records = await _db.SleepRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            if (records.Count < 10) return null;

            var labels = records.Select(r => r.RecordedAt.ToString("dd/MM")).ToList();
            var sleepData = records.Select(r => r.HoursSleep).ToList();

            var data = new
            {
                labels,
                datasets = new[]
                {
                    new {
                        label = "Sleep Duration (hours)",
                        fill = true,
                        borderColor = "#26A69A",
                        backgroundColor = "rgba(38,166,154,0.2)",
                        tension = 0.4,
                        data = sleepData
                    }
                }
            };
            return data;
        }

        // ===================== Trung bình giấc ngủ tháng =====================
        public async Task<double> GetAverageSleepAsync(int userProfileId)
        {
            var now = DateTime.Now;
            var oneMonthAgo = now.AddMonths(-1);

            var records = await _db.SleepRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now)
                .ToListAsync();

            return (double)(records.Count == 0 ? 0 : records.Average(r => r.HoursSleep));
        }
        //=== lấy dữ liệu gần nhất =====
        public async Task<object?> GetRecentlyAsync(int ProID)
        {
            var record = await _db.SleepRecords.Where(x => x.UserProfileId == ProID).OrderByDescending(x => x.RecordedAt).FirstOrDefaultAsync();

            if (record == null)
                return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == ProID);

            return new
            {
                Record = record,
                SleepAlert = _alertService.GetBloodSugarAlert(record.HoursSleep, pri)
            };
        }

        internal object CompareWithPrevious(object record1, object record2)
        {
            throw new NotImplementedException();
        }

    }
}
