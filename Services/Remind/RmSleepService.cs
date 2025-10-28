using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Reminds;
using SeviceSmartHopitail.Schemas.RM;

namespace SeviceSmartHopitail.Services.Remind
{
    public class RmSleepService
    {
        private readonly AppDbContext _db;

        public RmSleepService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RemindersSleep> CreateAsync(CrAllRemind re)
        {
            var remind = new RemindersSleep
            {
                TkId = re.TkId,
                Title = re.Title,
                TimeRemind = re.TimeRemind
            };
            _db.RemindersSleeps.Add(remind);
            await _db.SaveChangesAsync();
            return remind;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var remind = await _db.RemindersSleeps.FirstOrDefaultAsync(r => r.Id == id);
            if (remind == null) return false;

            _db.RemindersSleeps.Remove(remind);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
