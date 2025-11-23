using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Reminds;
using SeviceSmartHopitail.Schemas.RM;

namespace SeviceSmartHopitail.Services.Remind
{
    public class RmAllrSevice
    {
        private readonly AppDbContext _db;

        public RmAllrSevice(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RemindAll> CreateAsync(CrAllRemind re)
        {
            var remind = new RemindAll
            {
                TkId = re.TkId,
                Title = re.Title,
                TimeRemind = re.TimeRemind,
                Content = re.Content
            };
            _db.RemindAlls.Add(remind);
            await _db.SaveChangesAsync();
            return remind;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var remind = await _db.RemindAlls.FirstOrDefaultAsync(r => r.Id == id);
            if (remind == null) return false;

            _db.RemindAlls.Remove(remind);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<List<object>?> GetByTkIdAsync(int tkId)
        {
            var reminders = await _db.RemindAlls
                                    .Where(r => r.TkId == tkId)
                                    .ToListAsync();
            if (reminders == null || reminders.Count == 0) return null;
            var result = reminders
                .Select(r => (object)new
                {
                    Id = r.Id,
                    TkId = r.TkId,
                    Title = r.Title,
                    TimeRemind = r.TimeRemind,
                    Content = r.Content,
                    TimeVert = r.TimeRemind.HasValue ? ConvertToHourMinute((double)r.TimeRemind.Value) : null
                })
                .ToList();
            return result;
        }

        public string ConvertToHourMinute(double hours)
        {
            int h = (int)hours;
            int m = (int)Math.Round((hours - h) * 60);
            return $"{h} giờ {m} phút";
        }
    }
}
