using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Schemas;

namespace SeviceSmartHopitail.Services.Profiles
{
    public class UserProfileSevices
    {
        private readonly AppDbContext _db;
        private readonly CloudinaryServices _cloudinary;
        public UserProfileSevices(AppDbContext db, CloudinaryServices cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }

        public int TinhTuoi(DateOnly? birthDate)
        {
            if (!birthDate.HasValue)
                throw new ArgumentNullException(nameof(birthDate), "Birth date cannot be null.");

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - birthDate.Value.Year;

            if (today < birthDate.Value.AddYears(age))
            {
                age--;
            }

            return age;
        }

        public async Task<bool?> UpdatedAvatarAsync(int id, MemoryStream avatarStream, string fileName)
        {
            var profile = await _db.UserProfiles.FindAsync(id);
            if (profile == null) return null;

            // Upload ảnh lên Cloudinary
            var avatarUrl = await _cloudinary.UploadImg(avatarStream, fileName);
            Console.WriteLine(avatarUrl);

            if (!string.IsNullOrEmpty(avatarUrl))
            {
                profile.AvatarUrl = avatarUrl;
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Hàm độc lập để upload ảnh trả về URL
        public async Task<string?> UploadAvatarAsync(MemoryStream avatarStream, string fileName)
        {
            var avatarUrl = await _cloudinary.UploadImg(avatarStream, fileName);
            return avatarUrl;
        }

        // Lấy profile theo Id
        public async Task<UserProfile?> GetByIdAsync(int id)
        {
            return await _db.UserProfiles.FirstOrDefaultAsync(x => x.HoSoId == id) ?? null;               
        }

        // Lấy profile theo tài khoản
        public async Task<UserProfile?> GetByTaiKhoanIdAsync(int taiKhoanId)
        {
            return await _db.UserProfiles.FirstOrDefaultAsync(x => x.TaiKhoanId == taiKhoanId) ?? null;
               
        }

        // Thêm mới hồ sơ
        public async Task<UserProfile> CreateAsync(CreateUserProfile pf, MemoryStream avatarStream)
        {
            var profile = new UserProfile
            {
                TaiKhoanId = pf.TaiKhoanId,
                FullName = pf.FullName,
                Age = TinhTuoi(pf.Birth),
                Brith = pf.Birth,
                Gender = pf.Gender,
                Address = pf.Adress,
                Height = pf.Height,
                Weight = pf.Weight,
                Check = true
            };
            profile.AvatarUrl = await UploadAvatarAsync(avatarStream, profile.FullName ?? "avarta" + profile.HoSoId);
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();
            return profile;
        }

        // Cập nhật hồ sơ
        public async Task<UserProfile?> UpdateAsync(CreateUserProfile profile, MemoryStream avatarStream, int HoSoId)
        {
            var existing = await _db.UserProfiles.FindAsync(HoSoId);
            if (existing == null) return null;

            // cập nhật từng field
            existing.Age = TinhTuoi(profile.Birth);
            existing.Brith = profile.Birth;
            existing.Gender = profile.Gender;
            existing.Height = profile.Height;
            existing.Address = profile.Adress;
            existing.Weight = profile.Weight;
            existing.AvatarUrl = await UploadAvatarAsync(avatarStream, profile.FullName ?? "avarta" + HoSoId);
            existing.Check = true;
            await _db.SaveChangesAsync();
            return existing;
        }

        // Xóa hồ sơ
        public async Task<bool> DeleteAsync(int id)
        {
            var profile = await _db.UserProfiles.FindAsync(id);
            if (profile == null) return false;

            _db.UserProfiles.Remove(profile);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
