using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Schemas;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;

namespace SeviceSmartHopitail.Services
{
    public class MangementAccountServices
    {
        private readonly AppDbContext _db;

        public MangementAccountServices(AppDbContext db)
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
            return await _db.TaiKhoans.ToListAsync();
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

        // Update the AddAsync method to remove the reference to UserName from RegisterRequest
        public async Task<(bool, string)> AddMoi(string Email, string? UserName, string Password)
        {
            // Kiểm tra email đã tồn tại chưa
            var existingAccount = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.Email == Email);
            if (existingAccount != null)
            {
                return (false, "Email đã tồn tại.");
            }
            var us = new TaiKhoan
            {
                Email = Email,
                UserName = UserName ?? "chua dat ten", // Default value since UserName is not part of RegisterRequest
                PasswordHash = HashPassword(Password),
                Status = true // Mặc định tài khoản mới tạo là khóa (chưa kích hoạt)
            };
            await _db.TaiKhoans.AddAsync(us);
            await _db.SaveChangesAsync();
            return (true, "Tạo tài khoản thành công.");
        }
    }
}
