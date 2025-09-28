using SeviceSmartHopitail.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
        public DbSet<HealthRecord> HealthRecords { get; set; }
        public DbSet<PriWarning> PriWarnings { get; set; } = null!;
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

            // --- UserProfile <-> HealthRecord (1-n)
            modelBuilder.Entity<UserProfile>()
                .HasMany(up => up.HealthRecords)
                .WithOne(hr => hr.UserProfile)
                .HasForeignKey(hr => hr.UserProfileId);
        }
    }
}
