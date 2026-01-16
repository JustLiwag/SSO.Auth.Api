using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Models;


namespace SSO.Auth.Api.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Attendance> Attendance => Set<Attendance>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<PersonnelDivisionView> PersonnelDivisionDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PersonnelDivisionView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_PersonnelDivisionDetails");
            });
        }

    }
}
