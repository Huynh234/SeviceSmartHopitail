using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models.Reminds;
using SeviceSmartHopitail.Schemas.RM;

namespace SeviceSmartHopitail.Services.Remind
{
    public class RmExerciseService
    {
        private readonly AppDbContext _db;

        public RmExerciseService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RemindExercise> CreateAsync(CrAllRemind re)
        {
            var remind = new RemindExercise
            {
                TkId = re.TkId,
                Title = re.Title,
                TimeRemind = re.TimeRemind
            };
            _db.RemindExercises.Add(remind);
            await _db.SaveChangesAsync();
            return remind;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var remind = await _db.RemindExercises.FirstOrDefaultAsync(r => r.Id == id);
            if (remind == null) return false;

            _db.RemindExercises.Remove(remind);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
