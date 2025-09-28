using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Schemas;

namespace SeviceSmartHopitail.Services.Profiles
{
    public class HealthService
    {
        private readonly AppDbContext _db;

        public HealthService(AppDbContext db)
        {
            _db = db;
        }

        // ===================== Lấy record hôm nay =====================
        public async Task<HealthRecord?> GetTodayRecordAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await _db.HealthRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= today &&
                            r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();
        }

        // ===================== Lấy record hôm qua =====================
        public async Task<HealthRecord?> GetYesterdayRecordAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            return await _db.HealthRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= yesterday &&
                            r.RecordedAt < today)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();
        }

        // ===================== So sánh hôm nay vs hôm qua =====================
        public string CompareWithPrevious(HealthRecord today, HealthRecord? prev)
        {
            if (prev == null) return "Không có dữ liệu hôm trước";

            var result = new List<string>();

            // --- Nhịp tim ---
            if (today.HeartRate > prev.HeartRate) result.Add("Nhịp tim tăng");
            else if (today.HeartRate < prev.HeartRate) result.Add("Nhịp tim giảm");
            else result.Add("Nhịp tim giữ nguyên");

            // --- Đường huyết ---
            if (today.BloodSugar.HasValue && prev.BloodSugar.HasValue)
            {
                if (today.BloodSugar > prev.BloodSugar) result.Add("Đường huyết tăng");
                else if (today.BloodSugar < prev.BloodSugar) result.Add("Đường huyết giảm");
                else result.Add("Đường huyết giữ nguyên");
            }
            else
            {
                result.Add("Đường huyết không đủ dữ liệu");
            }

            // --- Huyết áp ---
            if (today.Systolic.HasValue && prev.Systolic.HasValue)
            {
                if (today.Systolic > prev.Systolic) result.Add("Huyết áp tâm thu tăng");
                else if (today.Systolic < prev.Systolic) result.Add("Huyết áp tâm thu giảm");
                else result.Add("Huyết áp tâm thu giữ nguyên");
            }
            else
            {
                result.Add("Huyết áp tâm thu không đủ dữ liệu");
            }

            if (today.Diastolic.HasValue && prev.Diastolic.HasValue)
            {
                if (today.Diastolic > prev.Diastolic) result.Add("Huyết áp tâm trương tăng");
                else if (today.Diastolic < prev.Diastolic) result.Add("Huyết áp tâm trương giảm");
                else result.Add("Huyết áp tâm trương giữ nguyên");
            }
            else
            {
                result.Add("Huyết áp tâm trương không đủ dữ liệu");
            }

            // --- Giấc ngủ ---
            if (today.TimeSleep.HasValue && prev.TimeSleep.HasValue)
            {
                if (today.TimeSleep > prev.TimeSleep) result.Add("Thời gian ngủ tăng");
                else if (today.TimeSleep < prev.TimeSleep) result.Add("Thời gian ngủ giảm");
                else result.Add("Thời gian ngủ giữ nguyên");
            }
            else
            {
                result.Add("Giấc ngủ không đủ dữ liệu");
            }

            return string.Join(" | ", result);
        }
        // ================== Create (Thêm mới hôm nay) ==================
        public async Task<HealthRecord> CreateTodayAsync(CreateHealthRecord record)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Kiểm tra nếu hôm nay đã có record => không cho thêm nữa
            var exists = await _db.HealthRecords
                .AnyAsync(r => r.UserProfileId == record.UserProfileId &&
                               r.RecordedAt >= today &&
                               r.RecordedAt < tomorrow);

            if (exists)
                throw new InvalidOperationException("Hôm nay đã có bản ghi. Vui lòng dùng Update.");

            var health = new HealthRecord
            {
                UserProfileId = record.UserProfileId,
                HeartRate = record.HeartRate,
                BloodSugar = record.BloodSugar,
                Systolic = record.Systolic,
                Diastolic = record.Diastolic,
                TimeSleep = record.TimeSleep,
                Note = record.Note,
                RecordedAt = record.RecordedAt == default ? DateTime.UtcNow : record.RecordedAt
            };

            _db.HealthRecords.Add(health);
            await _db.SaveChangesAsync();

            return health;
        }

        // ================== Update (Sửa record hôm nay) ==================
        public async Task<HealthRecord?> UpdateTodayAsync(int userProfileId, CreateHealthRecord updated)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var record = await _db.HealthRecords
                .FirstOrDefaultAsync(r => r.UserProfileId == userProfileId &&
                                          r.RecordedAt >= today &&
                                          r.RecordedAt < tomorrow);

            if (record == null) return null;

            // Cập nhật các giá trị
            record.HeartRate = updated.HeartRate;
            record.BloodSugar = updated.BloodSugar;
            record.Systolic = updated.Systolic;
            record.Diastolic = updated.Diastolic;
            record.TimeSleep = updated.TimeSleep;
            record.Note = updated.Note;
            record.RecordedAt = DateTime.UtcNow; // cập nhật thời gian mới

            await _db.SaveChangesAsync();
            return record;
        }

        // ================== Delete (Xóa record hôm nay) ==================
        public async Task<bool> DeleteTodayAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var record = await _db.HealthRecords
                .FirstOrDefaultAsync(r => r.UserProfileId == userProfileId &&
                                          r.RecordedAt >= today &&
                                          r.RecordedAt < tomorrow);

            if (record == null) return false;

            _db.HealthRecords.Remove(record);
            await _db.SaveChangesAsync();
            return true;
        }

        // ================== Lấy dữ liệu tâm thu tâm trương ==================
        public async Task<object?> GetBloodPressureChartDataAsync(int userProfileId)
        {
            var now = DateTime.UtcNow;
            var oneMonthAgo = now.AddMonths(-1);

            // Lấy dữ liệu trong vòng 1 tháng
            var records = await _db.HealthRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now &&
                            r.Systolic.HasValue &&
                            r.Diastolic.HasValue)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            // Nếu trong 1 tháng có ít hơn 10 record thì bỏ qua
            if (records.Count < 10) return null;

            // Labels = ngày đo (ví dụ: 27/09)
            var labels = records.Select(r => r.RecordedAt.ToString("dd/MM")).ToList();

            // Dataset Systolic
            var systolicData = records.Select(r => r.Systolic!.Value).ToList();

            // Dataset Diastolic
            var diastolicData = records.Select(r => r.Diastolic!.Value).ToList();

            var data = new
            {
                labels,
                datasets = new[]
                {
            new {
                label = "Systolic (mmHg)",
                fill = false,
                borderColor = "#42A5F5",
                yAxisID = "y",
                tension = 0.4,
                data = systolicData
            },
            new {
                label = "Diastolic (mmHg)",
                fill = false,
                borderColor = "#66BB6A",
                yAxisID = "y1",
                tension = 0.4,
                data = diastolicData
            }
        }
            };

            return data;
        }
    }
}
