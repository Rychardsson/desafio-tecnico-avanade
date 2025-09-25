using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Senha).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CriadoEm).HasDefaultValueSql("GETDATE()");
        });

        // Criar usuário administrador padrão
        var adminUser = new Usuario
        {
            Id = 1,
            Nome = "Administrador",
            Email = "admin@techchallenge.com",
            Senha = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin",
            CriadoEm = DateTime.UtcNow
        };

        modelBuilder.Entity<Usuario>().HasData(adminUser);
    }
}