using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace SeviceSmartHopitail.Services
{
    public class MangementAccount
    {
        private readonly AppDbContext _db;

        public MangementAccount(AppDbContext db)
        {
            _db = db;
        }

        // --- mã hóa mật khẩu password ---
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Lấy tất cả tài khoản
        public async Task<List<TaiKhoan>> GetAllAsync()
        {
            return await _db.TaiKhoans.Include(u => u.UserProfile).ToListAsync();
        }

        // Tìm tài khoản theo Id
        public async Task<TaiKhoan?> GetByIdAsync(int ?id = 0, string ?email = "")
        {
            if (!string.IsNullOrEmpty(email))
            {
                return await _db.TaiKhoans.Include(u => u.UserProfile)
                                          .FirstOrDefaultAsync(t => t.Email == email);
            }
            return await _db.TaiKhoans.Include(u => u.UserProfile)
                                      .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Sửa tài khoản
        //public async Task<bool> UpdateAsync(TaiKhoan model)
        //{
        //    var tk = await _db.TaiKhoans.FindAsync(model.Id);
        //    if (tk == null) return false;

        //    tk.UserName = model.UserName;
        //    tk.Email = model.Email;
        //    tk.Status = model.Status;

        //    await _db.SaveChangesAsync();
        //    return true;
        //}

        // Xóa tài khoản
        public async Task<bool> DeleteAsync(int id)
        {
            var tk = await _db.TaiKhoans.FindAsync(id);
            if (tk == null) return false;

            _db.TaiKhoans.Remove(tk);
            await _db.SaveChangesAsync();
            return true;
        }

        // Khóa tài khoản
        public async Task<bool> LockAccountAsync(int id)
        {
            var tk = await _db.TaiKhoans.FindAsync(id);
            if (tk == null) return false;

            tk.Status = false; // 0 = khóa
            await _db.SaveChangesAsync();
            return true;
        }

        // Mở khóa tài khoản
        public async Task<bool> UnlockAccountAsync(int id)
        {
            var tk = await _db.TaiKhoans.FindAsync(id);
            if (tk == null) return false;

            tk.Status = true; // 1 = mở
            await _db.SaveChangesAsync();
            return true;
        }

        // Reset mật khẩu
        public async Task<bool> ResetPasswordAsync(int id, string newPassword)
        {
            var tk = await _db.TaiKhoans.FindAsync(id);
            if (tk == null) return false;

            tk.PasswordHash = HashPassword(newPassword);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
