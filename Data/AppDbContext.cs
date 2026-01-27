using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Models;

namespace SSO.Auth.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<PersonnelDivisionView> PersonnelDivisionDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonnelDivisionView>()
            .HasNoKey()
            .ToView("vw_PersonnelDivisionDetails");

        base.OnModelCreating(modelBuilder);
    }

}
