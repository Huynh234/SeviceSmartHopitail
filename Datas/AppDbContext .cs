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
        public DbSet<RemindersSleep> RemindersSleeps { get; set; } = null!;
        public DbSet<RemindExercise> RemindExercises { get; set; } = null!;
        public DbSet<RemindDrinkWater> RemindDrinkWaters { get; set; } = null!;
        public DbSet<RemindTakeMedicine> RemindTakeMedicines { get; set; } = null!;

        public DbSet<HeartRateRecord> HeartRateRecords { get; set; }
        public DbSet<BloodPressureRecord> BloodPressureRecords { get; set; }
        public DbSet<BloodSugarRecord> BloodSugarRecords { get; set; }
        public DbSet<SleepRecord> SleepRecords { get; set; }

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

            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.RemindersSleep)
                .WithOne(r => r.TaiKhoan)
                .HasForeignKey<RemindersSleep>(r => r.TkId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.RemindExercise)
                .WithOne(r => r.TaiKhoan)
                .HasForeignKey<RemindExercise>(r => r.TkId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình 1-1: TaiKhoan ↔ RemindDrinkWater
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.RemindDrinkWater)
                .WithOne(r => r.TaiKhoan)
                .HasForeignKey<RemindDrinkWater>(r => r.TkId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình 1-1: TaiKhoan ↔ RemindTakeMedicine
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.RemindTakeMedicine)
                .WithOne(r => r.TaiKhoan)
                .HasForeignKey<RemindTakeMedicine>(r => r.TkId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<QuestionLog>()
                .HasOne(q => q.TaiKhoan)
                .WithMany(t => t.QuestionLogs)
                .HasForeignKey(q => q.TkId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
