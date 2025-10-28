using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeviceSmartHopitail.Datas;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeviceSmartHopitail.Services.MAIL
{
    public class ReminderSleepService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReminderSleepService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // Đây là hàm BackgroundService yêu cầu override
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

                        // Lấy các reminder có giờ trùng với giờ hiện tại
                        var reminders = db.RemindersSleeps
                            .Where(r => r.TimeRemind.HasValue && (int)r.TimeRemind.Value == currentHour)
                            .ToList();

                        foreach (var reminder in reminders)
                        {
                            // Nếu hôm nay đã gửi rồi thì bỏ qua
                            if (reminder.LastSent.HasValue && DateOnly.FromDateTime(reminder.LastSent.Value) == today)
                                continue;

                            // Lấy tài khoản tương ứng
                            var user = db.TaiKhoans.FirstOrDefault(u => u.Id == reminder.TkId && u.Status == true);
                            if (user == null) continue;

                            string subject = "Nhắc nhở giấc ngủ định kỳ";
                            string body = reminder.Title ?? "Hãy nghỉ ngơi đúng giờ để có giấc ngủ chất lượng.";

                            try
                            {
                                mailService.SendEmail(user.Email, subject, body);
                                Console.WriteLine($"Đã gửi email cho {user.Email} lúc {now}");

                                // Cập nhật ngày gửi cuối cùng
                                reminder.LastSent = now;
                                db.RemindersSleeps.Update(reminder);
                            }
                            catch (Exception mailEx)
                            {
                                Console.WriteLine($"Lỗi gửi email cho {user.Email}: {mailEx.Message}");
                            }
                        }

                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi chạy ReminderSleepService: {ex.Message}");
                }

                // Đợi đến đầu giờ kế tiếp
                DateTime nextHour = DateTime.Now.AddHours(1);
                DateTime nextRun = new DateTime(nextHour.Year, nextHour.Month, nextHour.Day, nextHour.Hour, 0, 5);
                TimeSpan delay = nextRun - DateTime.Now;

                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
