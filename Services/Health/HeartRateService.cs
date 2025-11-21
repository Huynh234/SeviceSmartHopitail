using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Health;
using SeviceSmartHopitail.Schemas.HR;
using SeviceSmartHopitail.Services.Profiles;
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
            var today = DateTime.Now.Date;
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
            var today = DateTime.Now.Date;
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
            var today = DateTime.Now.Date;

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
                RecordedAt = DateTime.Now
            };

            _db.HeartRateRecords.Add(record);
            await _db.SaveChangesAsync();

            var pri = await _db.PriWarnings
        .FirstOrDefaultAsync(p => p.UserProfileId == model.UserProfileId);

            // --- Nếu không có ngưỡng cá nhân thì dùng mặc định ---
            int minHR = pri?.MinHeartRate ?? 60;   // nhịp tim thấp nhất bình thường
            int maxHR = pri?.MaxHeartRate ?? 100;  // nhịp tim cao nhất bình thường

            string? message = null;
            string icon = "";
            string title = "Cảnh báo nhịp tim";
            string point = "HeartRate";

            if (model.HeartRate > maxHR)
            {
                message = $"Nhịp tim {model.HeartRate} bpm vượt ngưỡng tối đa ({maxHR} bpm).";
                icon = "arrow-up";
            }
            else if (model.HeartRate < minHR)
            {
                message = $"Nhịp tim {model.HeartRate} bpm thấp hơn ngưỡng tối thiểu ({minHR} bpm).";
                icon = "arrow-down";
            }

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

            return record;
        }

        // ========== Cập nhật ==========
        public async Task<HeartRateRecord?> UpdateTodayAsync(int userProfileId, UpdateHeartRateR model)
        {
            var today = DateTime.Now.Date;
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
            record.RecordedAt = DateTime.Now;
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
        public async Task<object?> GetHeartRateChartDataAsync(int userProfileId, DateTime oneMonthAgo, DateTime now)
        {
            var records = await _db.HeartRateRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= oneMonthAgo &&
                            r.RecordedAt <= now)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            if (records.Count < 5) return null;

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
            var now = DateTime.Now;
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
            var record = await _db.HeartRateRecords.Where(x => x.UserProfileId == ProID).OrderByDescending(x => x.RecordedAt).FirstOrDefaultAsync();

            if (record == null)
                return null;
            var today = DateTime.Now;
            var pri = await _db.PriWarnings
                .FirstOrDefaultAsync(p => p.UserProfileId == ProID);

            return new
            {
                Record = record,
                HeartRateAlert = _alertService.GetHeartRateAlert(record.HeartRate, pri),
                writeHours = (today - record.RecordedAt).ToString(@"hh\:mm"),
            };
        }
        // ===================== Báo cáo tổng hợp 7 ngày nhịp tim =====================
        public async Task<object?> Get7DaySummaryAsync(int userProfileId)
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-7);

            // Lấy dữ liệu 7 ngày gần nhất
            var records = await _db.HeartRateRecords
                .Where(r => r.UserProfileId == userProfileId &&
                            r.RecordedAt >= startDate &&
                            r.RecordedAt <= endDate)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            if (records.Count == 0)
                return null;

            // Tính toán cơ bản
            var avg = Math.Round(records.Average(r => r.HeartRate), 2);
            var max = records.Max(r => r.HeartRate);
            var min = records.Min(r => r.HeartRate);
            var latest = records.OrderByDescending(r => r.RecordedAt).First();

            // Lấy ngưỡng cảnh báo người dùng
            var pri = await _db.PriWarnings.FirstOrDefaultAsync(p => p.UserProfileId == userProfileId);

            // Đánh giá theo cảnh báo
            string avgAlert = _alertService.GetHeartRateAlert((decimal)avg, pri);
            string currentAlert = _alertService.GetHeartRateAlert(latest.HeartRate, pri);

            // Tạo đánh giá tổng quan
            string evaluation;
            if (avg > (pri?.MaxHeartRate ?? 100))
                evaluation = "Nhịp tim trung bình cao — có thể bạn đang căng thẳng hoặc vận động nhiều.";
            else if (avg < (pri?.MinHeartRate ?? 60))
                evaluation = "Nhịp tim trung bình thấp — nên theo dõi nếu có dấu hiệu mệt mỏi hoặc chóng mặt.";
            else
                evaluation = "Nhịp tim trung bình ổn định trong phạm vi bình thường.";

            // Kết quả trả về
            return new
            {
                UserProfileId = userProfileId,
                From = startDate.ToString("dd/MM/yyyy"),
                To = endDate.ToString("dd/MM/yyyy"),
                Average = avg,
                Max = max,
                Min = min,
                Current = new
                {
                    Value = latest.HeartRate,
                    RecordedAt = latest.RecordedAt.ToString("dd/MM/yyyy HH:mm"),
                    Alert = currentAlert
                },
                AverageAlert = avgAlert,
                Evaluation = evaluation,
                RecordCount = records.Count
            };
        }

    }
}
