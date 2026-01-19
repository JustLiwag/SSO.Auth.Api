using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Models;


namespace SSO.Auth.Api.Data
{

    /// Application DbContext for EF Core. Exposes DbSets for application tables
    /// and maps a database view to a model for read-only queries.
    public class AppDbContext : DbContext
    {

        /// Construct the context using DI-provided options.
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets map to database tables created by migrations.
        public DbSet<User> Users => Set<User>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Attendance> Attendance => Set<Attendance>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        // This DbSet maps to a database view (vw_PersonnelDivisionDetails).
        // It is configured as keyless in OnModelCreating.
        public DbSet<PersonnelDivisionView> PersonnelDivisionDetails { get; set; }

        /// Model configuration: configure the PersonnelDivisionView as a view without a primary key.
        /// Keep other model configuration here if needed (indexes, relationships, constraints).
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PersonnelDivisionView>(entity =>
            {
                // This represents a read-only DB view (no primary key)
                entity.HasNoKey();
                entity.ToView("vw_PersonnelDivisionDetails");
            });
        }
    }
}
