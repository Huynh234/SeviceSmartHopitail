using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SeviceSmartHopitail.Datas;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeviceSmartHopitail.Services.MAIL
{
    public class ScheduledEmailService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ScheduledEmailService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                DateTime nextRun = now.Date.AddHours(8); // 8h sáng hôm nay

                if (now > nextRun)
                {
                    nextRun = nextRun.AddDays(1); // nếu quá giờ thì hẹn ngày mai
                }

                TimeSpan delay = nextRun - now;
                await Task.Delay(delay, stoppingToken);

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var mailServices = scope.ServiceProvider.GetRequiredService<MailServices>();

                        // Lấy danh sách email đang active
                        var users = db.TaiKhoans.Where(t => t.Status == true).ToList();

                        Parallel.ForEach(users, user =>
                        {
                            mailServices.SendEmail(user.Email, "Thông báo hằng ngày",
                                $"Xin chào {user.UserName}\nBạn đừng quên vào cập nhập thông tin sức khỏe hàng ngày nhé");
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending scheduled emails: " + ex.Message);
                }

                // Chờ thêm 24h rồi chạy lại
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                //await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // test 1 phút gửi 1 lần
            }
        }
    }
}
