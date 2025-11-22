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
                        decimal currentTimeDecimal = now.Hour + (now.Minute / 60m);
                        DateOnly today = DateOnly.FromDateTime(now);
                        string currentDay = now.DayOfWeek.ToString().ToLower();

                        // Sai số 3 phút = 0.05 giờ
                        const decimal timeTolerance = 0.05m;

                        // Lấy tất cả reminder có TimeRemind, rồi lọc tại client
                        var reminders = db.RemindAlls
                            .Where(r => r.TimeRemind.HasValue)
                            .ToList()  // EF không hỗ trợ Math.Abs(decimal)
                            .Where(r => Math.Abs(r.TimeRemind.Value - currentTimeDecimal) <= timeTolerance)
                            .ToList();

                        foreach (var reminder in reminders)
                        {
                            bool shouldSend = false;

                            // Nếu DayOfWeek có giá trị → chỉ gửi ngày đó
                            if (!string.IsNullOrWhiteSpace(reminder.DayOfWkeek))
                            {
                                var allowedDays = reminder.DayOfWkeek
                                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                    .Select(d => d.ToLower())
                                    .ToList();

                                if (allowedDays.Contains(currentDay))
                                    shouldSend = true;
                            }
                            else
                            {
                                // Nếu rỗng → gửi hằng ngày
                                shouldSend = true;
                            }

                            // Nếu không gửi hôm nay → bỏ qua
                            if (!shouldSend)
                                continue;

                            // Nếu đã gửi hôm nay → bỏ qua
                            if (reminder.LastSent.HasValue &&
                                DateOnly.FromDateTime(reminder.LastSent.Value) == today)
                                continue;

                            // Lấy tài khoản
                            var user = db.TaiKhoans.FirstOrDefault(u => u.Id == reminder.TkId && u.Status == true);
                            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                                continue;

                            string subject = "Thông báo nhắc nhở " + (reminder.Content ?? "Nhắc nhở");
                            string body = "Hãy " + (reminder.Title ?? "Nhắc nhở định kỳ.") + " nào!";

                            try
                            {
                                mailService.SendEmail(user.Email, subject, body);
                                reminder.LastSent = now;

                                db.RemindAlls.Update(reminder);
                            }
                            catch (Exception mailEx)
                            {
                                Console.WriteLine($"[RemindALL] Lỗi gửi email: {mailEx.Message}");
                            }
                        }

                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RemindALL] Lỗi dịch vụ: {ex.Message}");
                }

                // Chờ 5 phút rồi chạy lại
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}