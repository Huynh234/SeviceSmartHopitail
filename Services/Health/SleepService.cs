using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Services.Profiles;
using SeviceSmartHopitail.Schemas.HR;

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

        public async Task<object?> GetTodayAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
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
            var today = DateTime.UtcNow.Date;
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
            var today = DateTime.UtcNow.Date;
            bool exists = await _db.SleepRecords
                .AnyAsync(r => r.UserProfileId == model.UserProfileId && r.RecordedAt >= today);
            if (exists)
                throw new InvalidOperationException("Hôm nay đã có bản ghi giấc ngủ.");

            var rec = new SleepRecord
            {
                UserProfileId = model.UserProfileId,
                HoursSleep = model.HoursSleep,
                Note = model.Note,
                RecordedAt = DateTime.UtcNow
            };

            _db.SleepRecords.Add(rec);
            await _db.SaveChangesAsync();
            return rec;
        }

        public async Task<SleepRecord?> UpdateTodayAsync(int userProfileId, UpdateSleepR model)
        {
            var today = DateTime.UtcNow.Date;
            var tomorow = today.AddDays(1);
            var record = await _db.SleepRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            record.HoursSleep = model.HoursSleep;
            record.Note = model.Note;
            record.RecordedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return record;
        }

        // ===================== So sánh giấc ngủ =====================
        public string CompareWithPrevious(SleepRecord today, SleepRecord? prev)
        {
             if (prev == null) return "Không có dữ liệu hôm trước";

            var diff = today.HoursSleep - prev.HoursSleep;
            var percent = (diff / prev.HoursSleep) * 100;

            if (diff > 0) return $"Thời gian ngủ tăng {diff:F1}h (+{percent:F1}%)";
            else if (diff < 0) return $"Thời gian ngủ giảm {-diff:F1}h ({percent:F1}%)";
            else return "Thời gian ngủ giữ nguyên";
        }
        // ===================== Biểu đồ giấc ngủ =====================
        public async Task<object?> GetSleepChartDataAsync(int userProfileId)
        {
            var now = DateTime.UtcNow;
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
            var now = DateTime.UtcNow;
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
            var record = await _db.SleepRecords.Where(x => x.UserProfileId == ProID).OrderBy(x => x.RecordedAt).FirstOrDefaultAsync();

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

    }
}
