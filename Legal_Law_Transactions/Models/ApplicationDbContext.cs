using Microsoft.EntityFrameworkCore;

namespace Legal_Law_Transactions.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<Signature> Signatures { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }
        public DbSet<Evidence> Evidences { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === User - Record ===
            modelBuilder.Entity<Record>()
                .HasOne(r => r.User)
                .WithMany(u => u.Records)
                .HasForeignKey(r => r.user_id)
                .OnDelete(DeleteBehavior.Restrict);

            // === Case - Record ===
            modelBuilder.Entity<Record>()
                .HasOne(r => r.Case)
                .WithMany()
                .HasForeignKey(r => r.case_id)
                .OnDelete(DeleteBehavior.Restrict);

            // === User - Case ===
            modelBuilder.Entity<Case>()
                .HasOne(c => c.User)
                .WithMany(u => u.Cases)
                .HasForeignKey(c => c.user_id)
                .OnDelete(DeleteBehavior.Restrict);

            // === User - License ===
            modelBuilder.Entity<License>()
                .HasOne(l => l.User)
                .WithMany(u => u.Licenses)
                .HasForeignKey(l => l.user_id)
                .OnDelete(DeleteBehavior.Restrict);

            // === User - Signature ===
            modelBuilder.Entity<Signature>()
                .HasOne(s => s.User)
                .WithMany(u => u.Signatures)
                .HasForeignKey(s => s.user_id)
                .OnDelete(DeleteBehavior.Restrict);

            // === Document - Signature ===
            modelBuilder.Entity<Signature>()
                .HasOne(s => s.Document)
                .WithMany(d => d.Signatures)
                .HasForeignKey(s => s.document_id)
                .OnDelete(DeleteBehavior.Restrict);

            // === Case - Schedule ===
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Case)
                .WithMany()
                .HasForeignKey(s => s.case_id)
                .OnDelete(DeleteBehavior.Restrict);

            // === User - Document ===
            modelBuilder.Entity<Document>()
                .HasOne(d => d.User)
                .WithMany(u => u.Documents)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Restrict);

            // === User - Evidence ===
            modelBuilder.Entity<Evidence>()
                .HasOne(e => e.User)
                .WithMany(u => u.Evidences)
                .HasForeignKey(e => e.user_id)
                .OnDelete(DeleteBehavior.Restrict);

            // === Case - Evidence ===
            modelBuilder.Entity<Evidence>()
                .HasOne(e => e.Case)
                .WithMany()
                .HasForeignKey(e => e.case_id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
