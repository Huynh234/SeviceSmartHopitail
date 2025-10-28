using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Reminds;
using SeviceSmartHopitail.Schemas.RM;

namespace SeviceSmartHopitail.Services.Remind
{
    public class RmWaterSevice
    {
        private readonly AppDbContext _db;

        public RmWaterSevice(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RemindDrinkWater> CreateAsync(CrAllRemind re)
        {
            var remind = new RemindDrinkWater
            {
                TkId = re.TkId,
                Title = re.Title,
                TimeRemind = re.TimeRemind
            };
            _db.RemindDrinkWaters.Add(remind);
            await _db.SaveChangesAsync();
            return remind;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var remind = await _db.RemindDrinkWaters.FirstOrDefaultAsync(r => r.Id == id);
            if (remind == null) return false;

            _db.RemindDrinkWaters.Remove(remind);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
