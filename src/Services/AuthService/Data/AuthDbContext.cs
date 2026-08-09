using AuthService.Models;
using BITask.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
        });

        // Seed a default administrator so protected write endpoints in ProductService can be
        // exercised immediately after the first run (see README for credentials).
        var hasher = new PasswordHasher<User>();
        var admin = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@bitask.local",
            Role = Roles.Admin,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

        modelBuilder.Entity<User>().HasData(admin);
    }
}
