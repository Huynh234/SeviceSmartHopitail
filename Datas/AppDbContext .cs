using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using SeviceSmartHopitail.Models.Health;
using SeviceSmartHopitail.Models.AI;
using SeviceSmartHopitail.Models.Reminds;
using SeviceSmartHopitail.Models.Infomation;

namespace SeviceSmartHopitail.Datas
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<IcdCode> IcdCodes { get; set; } = null!;
        public DbSet<IndexTerm> IndexTerms { get; set; } = null!;
        public DbSet<TextChunk> TextChunks { get; set; } = null!;
        public DbSet<QuestionLog> QuestionLogs { get; set; } = null!;
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<PriWarning> PriWarnings { get; set; } = null!;
        public DbSet<RemindAll> RemindAlls { get; set; } = null!;
        public DbSet<HeartRateRecord> HeartRateRecords { get; set; }
        public DbSet<BloodPressureRecord> BloodPressureRecords { get; set; }
        public DbSet<BloodSugarRecord> BloodSugarRecords { get; set; }
        public DbSet<SleepRecord> SleepRecords { get; set; }
        public DbSet<AutoWarning> AutoWarnings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.UserProfile)
                .WithOne(h => h.TaiKhoan)
                .HasForeignKey<UserProfile>(h => h.TaiKhoanId);

            // --- UserProfile <-> PriWarning (1-1)
            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.PriWarning)
                .WithOne(pw => pw.UserProfile)
                .HasForeignKey<PriWarning>(pw => pw.UserProfileId);

            // --- UserProfile <-> HealthHeartRate (1-n)
            modelBuilder.Entity<UserProfile>()
                .HasMany(up => up.HealthHeartRates)
                .WithOne(hr => hr.UserProfile)
                .HasForeignKey(hr => hr.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- UserProfile <-> HealthBloodSugar (1-n)
            modelBuilder.Entity<UserProfile>()
                .HasMany(up => up.HealthBloodSugars)
                .WithOne(bs => bs.UserProfile)
                .HasForeignKey(bs => bs.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- UserProfile <-> HealthBloodPressure (1-n)
            modelBuilder.Entity<UserProfile>()
                .HasMany(up => up.HealthBloodPressures)
                .WithOne(bp => bp.UserProfile)
                .HasForeignKey(bp => bp.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- UserProfile <-> RemindersSleep (1-n)
            modelBuilder.Entity<UserProfile>()
                .HasMany(up => up.SleepRecords)
                .WithOne(s => s.UserProfile)
                .HasForeignKey(s => s.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình 1-1: TaiKhoan ↔ RemindDrinkWater
            modelBuilder.Entity<TaiKhoan>()
                .HasMany(up => up.RemindAlls)
                .WithOne(rw => rw.TaiKhoan)
                .HasForeignKey(rw => rw.TkId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuestionLog>()
                .HasOne(q => q.TaiKhoan)
                .WithMany(t => t.QuestionLogs)
                .HasForeignKey(q => q.TkId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AutoWarning>()
                .HasOne(aw => aw.UserProfile)
                .WithMany(up => up.AutoWarnings)
                .HasForeignKey(aw => aw.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
