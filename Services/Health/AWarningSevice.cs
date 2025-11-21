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

        public async Task<List<AutoWarning>?> getID(int userProfileId, string? fill = "")
        {
            List<AutoWarning> warningFill = new List<AutoWarning>();
            if (string.IsNullOrEmpty(fill))
            {
                warningFill = await _db.AutoWarnings.Where(w => w.UserProfileId == userProfileId).ToListAsync();
            }
            else
            {
                warningFill = await _db.AutoWarnings.Where(w => w.UserProfileId == userProfileId && w.point.Equals(fill)).ToListAsync();
            }
            return warningFill;
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

        public async Task<bool> deleteAll(int id)
        {
            try
            {
                var li = await _db.AutoWarnings.Where(x => x.UserProfileId == id).ToListAsync();
                if (li == null)
                {
                    return false;
                }
                _db.AutoWarnings.RemoveRange(li);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
