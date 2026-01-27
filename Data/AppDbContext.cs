using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Models;


namespace SSO.Auth.Api.Data
{

    using Microsoft.EntityFrameworkCore;
    using SSO.Auth.Api.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<PMS_personnel_information> PMS_personnel_information { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;



        // 1️⃣ Add this for the view
        public DbSet<PersonnelDivisionView> PersonnelDivisionView { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table entity
            modelBuilder.Entity<PMS_personnel_information>()
                .HasKey(p => p.hris_id);

            // View entity
            modelBuilder.Entity<PersonnelDivisionView>()
                .HasNoKey()
                .ToView("vw_PersonnelDivisionDetails"); // map to view
        }
    }

}
