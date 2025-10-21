using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Services.Profiles;
using SeviceSmartHopitail.Schemas.HR;

namespace SeviceSmartHopitail.Services.Health
{
    public class BloodSugarService
    {
        private readonly AppDbContext _db;
        private readonly HealthAlertService _alertService;

        public BloodSugarService(AppDbContext db)
        {
            _db = db;
            _alertService = new HealthAlertService();
        }

        public async Task<Object?> GetTodayAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var record = await _db.BloodSugarRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new { Record = record, BloodSugarAlert = _alertService.GetBloodSugarAlert(record.BloodSugar, pri) };
        }

        public async Task<Object?> GetYesterdayAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var record = await _db.BloodSugarRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= yesterday && r.RecordedAt < today)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new { Record = record, BloodSugarAlert = _alertService.GetBloodSugarAlert(record.BloodSugar, pri) };
        }

        public async Task<BloodSugarRecord> CreateAsync(CreateBloodSugarRecord model)
        {
            var today = DateTime.UtcNow.Date;
            bool exists = await _db.BloodSugarRecords
                .AnyAsync(r => r.UserProfileId == model.UserProfileId && r.RecordedAt >= today);
            if (exists)
                throw new InvalidOperationException("Hôm nay đã có bản ghi đường huyết.");

            var rec = new BloodSugarRecord
            {
                UserProfileId = model.UserProfileId,
                BloodSugar = model.BloodSugar,
                Note = model.Note,
                RecordedAt = DateTime.UtcNow
            };

            _db.BloodSugarRecords.Add(rec);
            await _db.SaveChangesAsync();
            return rec;
        }

        public async Task<BloodSugarRecord?> UpdateTodayAsync(int userProfileId, UpdateBloodSugarR model)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var record = await _db.BloodSugarRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();
            if (record == null) return null;

            record.BloodSugar = model.BloodSugar;
            record.Note = model.Note;
            record.RecordedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return record;
        }
        // ===================== So sánh đường huyết =====================
        public string CompareWithPrevious(BloodSugarRecord today, BloodSugarRecord? prev)
        {
            if (prev == null) return "Không có dữ liệu hôm trước";

            if (today.BloodSugar > prev.BloodSugar) return "Đường huyết tăng";
            else if (today.BloodSugar < prev.BloodSugar) return "Đường huyết giảm";
            else return "Đường huyết giữ nguyên";
        }
        // ===================== Biểu đồ đường huyết =====================
        public async Task<object?> GetBloodSugarChartDataAsync(int userProfileId)
        {
            var now = DateTime.UtcNow;
            var oneMonthAgo = now.AddMonths(-1);

            var records = await _db.BloodSugarRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            if (records.Count < 10) return null;

            var labels = records.Select(r => r.RecordedAt.ToString("dd/MM")).ToList();
            var sugarData = records.Select(r => r.BloodSugar).ToList();

            var data = new
            {
                labels,
                datasets = new[]
                {
                    new {
                        label = "Blood Sugar (mg/dL)",
                        fill = false,
                        borderColor = "#AB47BC",
                        tension = 0.4,
                        data = sugarData
                    }
                }
            };

            return data;
        }
    }
}
