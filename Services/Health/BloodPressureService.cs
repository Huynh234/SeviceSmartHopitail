using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Health;
using SeviceSmartHopitail.Schemas.HR;
using SeviceSmartHopitail.Services.Profiles;
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
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var record = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();
            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new
            {
                Record = record,
                BloodPressureAlert = _alertService.GetBloodPressureAlert(record.Systolic, record.Diastolic, pri)
            };
        }

        public async Task<Object?> GetYesterdayAsync(int userProfileId)
        {
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var record = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= yesterday && r.RecordedAt < today)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null) return null;

            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);
            return new
            {
                Record = record,
                BloodPressureAlert = _alertService.GetBloodPressureAlert(record.Systolic, record.Diastolic, pri)
            };
        }

        public async Task<BloodPressureRecord> CreateAsync(CreateBloodPressureRecord model)
        {
            var today = DateTime.Now.Date;
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
                RecordedAt = DateTime.Now
            };

            _db.BloodPressureRecords.Add(rec);
            await _db.SaveChangesAsync();

            var pri = await _db.PriWarnings
        .FirstOrDefaultAsync(p => p.UserProfileId == model.UserProfileId);

            // --- Nếu không có ngưỡng cá nhân thì dùng mặc định ---
            int minSys = pri?.MinSystolic ?? 90;    // huyết áp tâm thu tối thiểu
            int maxSys = pri?.MaxSystolic ?? 140;   // huyết áp tâm thu tối đa
            int minDia = pri?.MinDiastolic ?? 60;   // huyết áp tâm trương tối thiểu
            int maxDia = pri?.MaxDiastolic ?? 90;   // huyết áp tâm trương tối đa

            string? message = null;
            string icon = "";
            string title = "Cảnh báo huyết áp";
            string point = "BloodPressure";

            // Kiểm tra vượt ngưỡng
            if (model.Systolic > maxSys)
            {
                message = $"Huyết áp tâm thu {model.Systolic} mmHg vượt ngưỡng tối đa ({maxSys} mmHg).";
                icon = "pi-chart-line";
            }
            else if (model.Systolic < minSys)
            {
                message = $"Huyết áp tâm thu {model.Systolic} mmHg thấp hơn ngưỡng tối thiểu ({minSys} mmHg).";
                icon = "pi-chart-scatter";
            }
            else if (model.Diastolic > maxDia)
            {
                message = $"Huyết áp tâm trương {model.Diastolic} mmHg vượt ngưỡng tối đa ({maxDia} mmHg).";
                icon = "pi-chart-line";
            }
            else if (model.Diastolic < minDia)
            {
                message = $"Huyết áp tâm trương {model.Diastolic} mmHg thấp hơn ngưỡng tối thiểu ({minDia} mmHg).";
                icon = "pi-chart-scatter";
            }

            // Nếu có cảnh báo thì lưu vào bảng AutoWarning
            if (message != null)
            {
                var warning = new AutoWarning
                {
                    UserProfileId = model.UserProfileId,
                    point = point,
                    icon = icon,
                    title = title,
                    node = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    mess = message,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _db.Add(warning);
                await _db.SaveChangesAsync();
            }

            return rec;
        }

        public async Task<BloodPressureRecord?> UpdateTodayAsync(int userProfileId, UpdateBloodPressureR model)
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var record = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();
            if (record == null) return null;

            record.Systolic = model.Systolic;
            record.Diastolic = model.Diastolic;
            record.Note = model.Note;
            record.RecordedAt = DateTime.Now;

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
        public async Task<object?> GetBloodPressureChartDataAsync(int userProfileId, DateTime oneMonthAgo, DateTime now)
        {

            var records = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            if (records.Count < 5) return null;

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

        public async Task<object?> GetBloodPressureChartBydayAsync(int userProfileId, int about)
        {
            var now = DateTime.Now;
            var oneMonthAgo = now.AddMonths(-1);

            if (about == 7)
            {
                oneMonthAgo = now.AddDays(-7);
            }
            else
            {
                oneMonthAgo = now.AddMonths(-1);
            }


            var records = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            if (records.Count < 5) return null;

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
            var endDate = DateTime.Now;
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

        // ===================== Lấy bản ghi gần đây nhất =====================
        public async Task<object?> GetRecentlyAsync(int userProfileId)
        {
            var record = await _db.BloodPressureRecords
                .Where(r => r.UserProfileId == userProfileId)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (record == null)
                return null;
            var today = DateTime.Now;
            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);

            return new
            {
                Record = record,
                BloodPressureAlert = _alertService.GetBloodPressureAlert(record.Systolic, record.Diastolic, pri),
                writeHours = (today - record.RecordedAt).ToString(@"hh\:mm"),
            };
        }


    }
}
