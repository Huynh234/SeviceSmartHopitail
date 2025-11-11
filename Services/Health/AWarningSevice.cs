using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Health;

namespace SeviceSmartHopitail.Services.Health
{
    public class AWarningSevice
    {
        private readonly AppDbContext _db;
        public AWarningSevice(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<AutoWarning>?> getID(int userProfileId)
        {
            var today = DateTime.Now.Date;
            var warning = await _db.AutoWarnings.Where(w => w.UserProfileId == userProfileId && w.CreatedAt.Date == today).ToListAsync();
            return warning;
        }

        public async Task<bool> delete(int id)
        {
            var aw = await _db.AutoWarnings.FirstOrDefaultAsync(x => x.Id == id);
            if (aw != null)
            {
                _db.AutoWarnings.Remove(aw);
                return true;
            }
            await _db.SaveChangesAsync();
            return false;
        }
    }
}
