using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Reminds;
using SeviceSmartHopitail.Schemas.RM;

namespace SeviceSmartHopitail.Services.Remind
{
    public class RmMedicineService
    {
        private readonly AppDbContext _db;

        public RmMedicineService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RemindTakeMedicine> CreateAsync(CrAllRemind re)
        {
            var remind = new RemindTakeMedicine
            {
                TkId = re.TkId,
                Title = re.Title,
                TimeRemind = re.TimeRemind
            };
            _db.RemindTakeMedicines.Add(remind);
            await _db.SaveChangesAsync();
            return remind;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var remind = await _db.RemindTakeMedicines.FirstOrDefaultAsync(r => r.Id == id);
            if (remind == null) return false;

            _db.RemindTakeMedicines.Remove(remind);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
