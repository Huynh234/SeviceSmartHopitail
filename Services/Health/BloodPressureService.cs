using SeviceSmartHopitail.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Services.Profiles;
using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Schemas.HR;
namespace SeviceSmartHopitail.Services.Health
{
    public class BloodPressureService
    {
        private readonly AppDbContext _db;
        private readonly HealthAlertService _alertService;

        public BloodPressureService(AppDbContext db)
        {
            _db = db;
            _alertService = new HealthAlertService();
        }

        public async Task<Object?> GetTodayAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var record = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();
            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new{
                Record = record,
                BloodPressureAlert = _alertService.GetBloodPressureAlert(record.Systolic, record.Diastolic, pri)
                };
        }

        public async Task<Object?> GetYesterdayAsync(int userProfileId)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var record = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= yesterday && r.RecordedAt < today)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new{
                Record = record,
                BloodPressureAlert = _alertService.GetBloodPressureAlert(record.Systolic, record.Diastolic, pri)
                };
        }

        public async Task<BloodPressureRecord> CreateAsync(CreateBloodPressureRecord model)
        {
            var today = DateTime.UtcNow.Date;
            bool exists = await _db.BloodPressureRecords
                .AnyAsync(r => r.UserProfileId == model.UserProfileId && r.RecordedAt >= today);
            if (exists)
                throw new InvalidOperationException("Hôm nay đã có bản ghi huyết áp.");

            var rec = new BloodPressureRecord
            {
                UserProfileId = model.UserProfileId,
                Systolic = model.Systolic,
                Diastolic = model.Diastolic,
                Note = model.Note,
                RecordedAt = DateTime.UtcNow
            };

            _db.BloodPressureRecords.Add(rec);
            await _db.SaveChangesAsync();
            return rec;
        }

        public async Task<BloodPressureRecord?> UpdateTodayAsync(int userProfileId, UpdateBloodPressureR model)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var record = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();
            if (record == null) return null;
            
            record.Systolic = model.Systolic;
            record.Diastolic = model.Diastolic;
            record.Note = model.Note;
            record.RecordedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return record;
        }
         // ===================== So sánh huyết áp =====================
        public string CompareWithPrevious(BloodPressureRecord today, BloodPressureRecord? prev)
        {
            if (prev == null) return "Không có dữ liệu hôm trước";

            var result = new List<string>();

            if (today.Systolic > prev.Systolic) result.Add("Huyết áp tâm thu tăng");
            else if (today.Systolic < prev.Systolic) result.Add("Huyết áp tâm thu giảm");
            else result.Add("Huyết áp tâm thu giữ nguyên");

            if (today.Diastolic > prev.Diastolic) result.Add("Huyết áp tâm trương tăng");
            else if (today.Diastolic < prev.Diastolic) result.Add("Huyết áp tâm trương giảm");
            else result.Add("Huyết áp tâm trương giữ nguyên");

            return string.Join(" | ", result);
        }

        // ===================== Biểu đồ huyết áp =====================
        public async Task<object?> GetBloodPressureChartDataAsync(int userProfileId)
        {
            var now = DateTime.UtcNow;
            var oneMonthAgo = now.AddMonths(-1);

            var records = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            if (records.Count < 10) return null;

            var labels = records.Select(r => r.RecordedAt.ToString("dd/MM")).ToList();
            var systolicData = records.Select(r => r.Systolic).ToList();
            var diastolicData = records.Select(r => r.Diastolic).ToList();

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

        // ===================== Trung bình huyết áp (30 ngày gần nhất) =====================
        public async Task<object?> GetAverageBloodPressureAsync(int userProfileId, int days = 30)
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var records = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= startDate && r.RecordedAt <= endDate)
                .ToListAsync();

            if (records.Count == 0)
                return null;

            var avgSystolic = Math.Round(records.Average(r => r.Systolic), 2);
            var avgDiastolic = Math.Round(records.Average(r => r.Diastolic), 2);

            return new
            {
                UserProfileId = userProfileId,
                Days = days,
                AverageSystolic = avgSystolic,
                AverageDiastolic = avgDiastolic,
                RecordCount = records.Count,
                From = startDate.ToString("dd/MM/yyyy"),
                To = endDate.ToString("dd/MM/yyyy")
            };
        }
    }
}
