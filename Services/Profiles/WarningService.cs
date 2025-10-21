using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Schemas.HR;

namespace SeviceSmartHopitail.Services.Profiles
{
    public class WarningService
    {
        private readonly AppDbContext _db;

        public WarningService(AppDbContext db)
        {
            _db = db;
        }

        // Thêm mới cảnh báo cá nhân
        public async Task<PriWarning> CreateAsync(CPWarning warning)
        {
            var newWarning = new PriWarning
            {
                UserProfileId = warning.UserProfileId!.Value,
                MinHeartRate = warning.MinHeartRate,
                MaxHeartRate = warning.MaxHeartRate,
                MinBloodSugar = warning.MinBloodSugar,
                MaxBloodSugar = warning.MaxBloodSugar,
                MinSystolic = warning.MinSystolic,
                MaxSystolic = warning.MaxSystolic,
                MinDiastolic = warning.MinDiastolic,
                MaxDiastolic = warning.MaxDiastolic,
                MinSleep = warning.MinSleep,
                MaxSleep = warning.MaxSleep,
                CreatedAt = warning.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.PriWarnings.AddAsync(newWarning);
            await _db.SaveChangesAsync();

            return newWarning;
        }

        // Cập nhật cảnh báo
        public async Task<PriWarning?> UpdateAsync(int WId, CPWarning warning)
        {
            var existing = await _db.PriWarnings
                .FirstOrDefaultAsync(x => x.WarningId == WId);

            if (existing == null) return null;

            existing.MinHeartRate = warning.MinHeartRate;
            existing.MaxHeartRate = warning.MaxHeartRate;
            existing.MinBloodSugar = warning.MinBloodSugar;
            existing.MaxBloodSugar = warning.MaxBloodSugar;
            existing.MinSystolic = warning.MinSystolic;
            existing.MaxSystolic = warning.MaxSystolic;
            existing.MinDiastolic = warning.MinDiastolic;
            existing.MaxDiastolic = warning.MaxDiastolic;
            existing.MinSleep = warning.MinSleep;
            existing.MaxSleep = warning.MaxSleep;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return existing;
        }

        // Xóa cảnh báo
        public async Task<bool> DeleteAsync(int warningId)
        {
            var existing = await _db.PriWarnings
                .FirstOrDefaultAsync(x => x.WarningId == warningId);

            if (existing == null) return false;

            _db.PriWarnings.Remove(existing);
            await _db.SaveChangesAsync();

            return true;
        }

        // Lấy cảnh báo theo UserProfileId
        public async Task<PriWarning?> GetByUserProfileIdAsync(int userProfileId)
        {
            return await _db.PriWarnings
                .FirstOrDefaultAsync(x => x.UserProfileId == userProfileId);
        }
    }
}
