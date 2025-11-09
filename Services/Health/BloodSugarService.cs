using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Services.Profiles;
using SeviceSmartHopitail.Schemas.HR;
using SeviceSmartHopitail.Models.Health;

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
            var today = DateTime.Now.Date;
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
            var today = DateTime.Now.Date;
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
            var today = DateTime.Now.Date;
            bool exists = await _db.BloodSugarRecords
                .AnyAsync(r => r.UserProfileId == model.UserProfileId && r.RecordedAt >= today);
            if (exists)
                throw new InvalidOperationException("Hôm nay đã có bản ghi đường huyết.");

            var rec = new BloodSugarRecord
            {
                UserProfileId = model.UserProfileId,
                BloodSugar = model.BloodSugar,
                Note = model.Note,
                RecordedAt = DateTime.Now
            };

            _db.BloodSugarRecords.Add(rec);
            await _db.SaveChangesAsync();
            return rec;
        }

        public async Task<BloodSugarRecord?> UpdateTodayAsync(int userProfileId, UpdateBloodSugarR model)
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var record = await _db.BloodSugarRecords
                .Where(r => r.UserProfileId == userProfileId && r.RecordedAt >= today && r.RecordedAt < tomorrow)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();
            if (record == null) return null;

            record.BloodSugar = model.BloodSugar;
            record.Note = model.Note;
            record.RecordedAt = DateTime.Now;

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
            var now = DateTime.Now;
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
         // ===================== Trung bình đường huyết =====================
        public async Task<object?> GetAverageBloodSugarAsync(int userProfileId, int days = 30)
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-days);

            var records = await _db.BloodSugarRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= startDate &&
                            r.RecordedAt <= endDate)
                .ToListAsync();

            if (records.Count == 0)
                return null;

            var avgBloodSugar = Math.Round(records.Average(r => r.BloodSugar), 2);

            return new
            {
                UserProfileId = userProfileId,
                Days = days,
                AverageBloodSugar = avgBloodSugar,
                RecordCount = records.Count,
                From = startDate.ToString("dd/MM/yyyy"),
                To = endDate.ToString("dd/MM/yyyy")
            };
        }
//================ lấy dữ liệu gần đây ========================
        public async Task<object?> GetRecentlyAsync(int ProID)
        {
            var record = await _db.BloodSugarRecords.Where(x => x.UserProfileId == ProID).OrderByDescending(x => x.RecordedAt).FirstOrDefaultAsync();

            if (record == null)
                return null;
            var today = DateTime.Now;
            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == ProID);

            return new
            {
                Record = record,
                BloodSugarcord = _alertService.GetBloodSugarAlert(record.BloodSugar, pri),
                writeHours = (today - record.RecordedAt).TotalHours
            };
        }
        // ===================== Báo cáo tổng hợp 7 ngày đường huyết =====================
        public async Task<object?> Get7DaySummaryAsync(int userProfileId)
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-7);

            // Lấy dữ liệu trong 7 ngày gần nhất
            var records = await _db.BloodSugarRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= startDate &&
                            r.RecordedAt <= endDate)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            if (records.Count == 0)
                return null;

            // Tính toán
            var avg = Math.Round(records.Average(r => r.BloodSugar), 2);
            var max = records.Max(r => r.BloodSugar);
            var min = records.Min(r => r.BloodSugar);
            var latest = records.OrderByDescending(r => r.RecordedAt).First();

            // Lấy ngưỡng cảnh báo người dùng
            var pri = await _db.PriWarnings.FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);

            // Đánh giá tổng quan
            string avgAlert = _alertService.GetBloodSugarAlert(avg, pri);
            string currentAlert = _alertService.GetBloodSugarAlert(latest.BloodSugar, pri);

            string overallEvaluation;
            if (avg > (pri?.MaxBloodSugar ?? 140))
                overallEvaluation = "Đường huyết trung bình cao — cần điều chỉnh chế độ ăn và sinh hoạt.";
            else if (avg < (pri?.MinBloodSugar ?? 70))
                overallEvaluation = "Đường huyết trung bình thấp — nên theo dõi tình trạng hạ đường huyết.";
            else
                overallEvaluation = "Đường huyết trung bình ổn định trong phạm vi bình thường.";

            // Kết quả trả về
            return new
            {
                UserProfileId = userProfileId,
                From = startDate.ToString("dd/MM/yyyy"),
                To = endDate.ToString("dd/MM/yyyy"),
                Max = max,
                Min = min,
                Current = new
                {
                    Value = latest.BloodSugar,
                    RecordedAt = latest.RecordedAt.ToString("dd/MM/yyyy HH:mm"),
                    Alert = currentAlert
                },
                AverageAlert = avgAlert,
                Evaluation = overallEvaluation,
                RecordCount = records.Count
            };
        }

    }
}
