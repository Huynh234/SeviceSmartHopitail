using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Reminds;
using Microsoft.Extensions.DependencyInjection;

namespace SeviceSmartHopitail.Services.MAIL
{
    public class RemindALLService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RemindALLService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var mailService = scope.ServiceProvider.GetRequiredService<MailServices>();

                        DateTime now = DateTime.Now;
                        int currentHour = now.Hour;
                        DateOnly today = DateOnly.FromDateTime(now);
                        string currentDay = now.DayOfWeek.ToString(); // "Monday", "Tuesday"...

                        // Lấy các reminder có giờ trùng với giờ hiện tại
                        decimal currentTimeDecimal = now.Hour + (now.Minute / 60m);

                        // Cho phép sai lệch nhỏ (khoảng ±1 phút = 0.0167 giờ)
                        var reminders = db.RemindAlls
                            .Where(r => r.TimeRemind.HasValue &&
                                        Math.Abs(r.TimeRemind.Value - currentTimeDecimal) < 0.02m)
                            .ToList();

                        foreach (var reminder in reminders)
                        {
                            bool shouldSend = false;

                            // Nếu có DayOfWkeek → chỉ gửi vào những ngày trong danh sách
                            if (!string.IsNullOrEmpty(reminder.DayOfWkeek))
                            {
                                var allowedDays = reminder.DayOfWkeek
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Select(d => d.ToLower())
                                    .ToList();

                                if (allowedDays.Contains(currentDay.ToLower()))
                                    shouldSend = true;
                            }
                            else
                            {
                                // Nếu DayOfWkeek rỗng → gửi hàng ngày
                                shouldSend = true;
                            }

                            // Nếu không cần gửi hôm nay → bỏ qua
                            if (!shouldSend)
                                continue;

                            // Nếu hôm nay đã gửi rồi → bỏ qua
                            if (reminder.LastSent.HasValue && DateOnly.FromDateTime(reminder.LastSent.Value) == today)
                                continue;

                            // Lấy tài khoản tương ứng
                            var user = db.TaiKhoans.FirstOrDefault(u => u.Id == reminder.TkId && u.Status == true);
                            if (user == null) continue;

                            string subject = reminder.Title ?? "Nhắc nhở định kỳ";
                            string body = reminder.Content ?? "Hãy nhớ chăm sóc sức khỏe mỗi ngày nhé!";

                            try
                            {
                                mailService.SendEmail(user.Email, subject, body);
                                Console.WriteLine($"[RemindALL] Đã gửi email cho {user.Email} ({currentDay}, {currentHour}h)");

                                // Cập nhật ngày gửi cuối cùng
                                reminder.LastSent = now;
                                db.RemindAlls.Update(reminder);
                            }
                            catch (Exception mailEx)
                            {
                                Console.WriteLine($"[RemindALL] Lỗi gửi email cho {user.Email}: {mailEx.Message}");
                            }
                        }

                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RemindALL] Lỗi khi chạy dịch vụ: {ex.Message}");
                }

                // Chờ đến đầu giờ kế tiếp (ví dụ 10:00:05)
                DateTime nextHour = DateTime.Now.AddHours(1);
                DateTime nextRun = new DateTime(nextHour.Year, nextHour.Month, nextHour.Day, nextHour.Hour, 0, 5);
                TimeSpan delay = nextRun - DateTime.Now;

                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}

