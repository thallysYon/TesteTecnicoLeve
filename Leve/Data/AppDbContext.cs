using Leve.Models;
using Microsoft.EntityFrameworkCore;

namespace Leve.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Tarefa> Tarefas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Tarefa>()
            .HasOne(t => t.Responsavel)
            .WithMany()
            .HasForeignKey(t => t.ResponsavelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tarefa>()
            .HasOne(t => t.GestorCriador)
            .WithMany()
            .HasForeignKey(t => t.GestorCriadorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}