using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Services.Profiles;
using SeviceSmartHopitail.Schemas.HR;
namespace SeviceSmartHopitail.Services.Health
{
    public class HeartRateService
    {
        private readonly AppDbContext _db;
        private readonly HealthAlertService _alertService;

        public HeartRateService(AppDbContext db)
        {
            _db = db;
            _alertService = new HealthAlertService();
        }

        // ========== Lấy record hôm nay ==========
        public async Task<Object?> GetTodayAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var record = await _db.HeartRateRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= today &&
                            r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new { Record = record, HeartRateAlert = _alertService.GetHeartRateAlert(record.HeartRate, pri) };
        }

        public async Task<Object?> GetYesterdayAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var record = await _db.HeartRateRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= yesterday &&
                            r.RecordedAt < today)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new { Record = record, HeartRateAlert = _alertService.GetHeartRateAlert(record.HeartRate, pri) };
        }

        // ========== Tạo record ==========
        public async Task<HeartRateRecord> CreateAsync(CreateHeartRateRecord model)
        {
            var today = DateTime.UtcNow.Date;

            // kiểm tra trùng trong ngày
            bool exists = await _db.HeartRateRecords
                .AnyAsync(r => r.UserProfileId == model.UserProfileId &&
                               r.RecordedAt >= today);

            if (exists)
                throw new InvalidOperationException("Hôm nay đã có bản ghi nhịp tim.");

            var record = new HeartRateRecord
            {
                UserProfileId = model.UserProfileId,
                HeartRate = model.HeartRate,
                Note = model.Note,
                RecordedAt = DateTime.UtcNow
            };

            _db.HeartRateRecords.Add(record);
            await _db.SaveChangesAsync();
            return record;
        }

        // ========== Cập nhật ==========
        public async Task<HeartRateRecord?> UpdateTodayAsync(int userProfileId, UpdateHeartRateR model)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var record = await _db.HeartRateRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= today &&
                            r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();
            if (record == null) return null;

            record.HeartRate = model.HeartRate;
            record.Note = model.Note;
            record.RecordedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return record;
        }
        // ===================== So sánh nhịp tim =====================
        public string CompareWithPrevious(HeartRateRecord today, HeartRateRecord? prev)
        {
            if (prev == null) return "Không có dữ liệu hôm trước";

            if (today.HeartRate > prev.HeartRate) return "Nhịp tim tăng";
            else if (today.HeartRate < prev.HeartRate) return "Nhịp tim giảm";
            else return "Nhịp tim giữ nguyên";
        }
        // ===================== Biểu đồ nhịp tim =====================
        public async Task<object?> GetHeartRateChartDataAsync(int userProfileId)
        {
            var now = DateTime.UtcNow;
            var oneMonthAgo = now.AddMonths(-1);

            var records = await _db.HeartRateRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            if (records.Count < 10) return null;

            var labels = records.Select(r => r.RecordedAt.ToString("dd/MM")).ToList();
            var heartRateData = records.Select(r => r.HeartRate).ToList();

            var data = new
            {
                labels,
                datasets = new[]
                {
                    new {
                        label = "Heart Rate (bpm)",
                        fill = false,
                        borderColor = "#EF5350",
                        tension = 0.4,
                        data = heartRateData
                    }
                }
            };

            return data;
        }

        public async Task<double> GetAverageHeartRateAsync(int userProfileId)
        {
            var now = DateTime.UtcNow;
            var oneMonthAgo = now.AddMonths(-1);
            var records = await _db.HeartRateRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now)
                .ToListAsync();
            return records.Count == 0 ? 0 : records.Average(r => r.HeartRate);
        }

        public async Task<object?> GetRecentlyAsync(int ProID)
        {
            var record = await _db.HeartRateRecords.Where(x => x.UserProfileId == ProID).OrderBy(x => x.RecordedAt).FirstOrDefaultAsync();

            if (record == null)
                return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == ProID);

            return new
            {
                Record = record,
                HeartRateAlert = _alertService.GetHeartRateAlert(record.HeartRate, pri)
            };
        }

    }
}
