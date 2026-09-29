using Microsoft.EntityFrameworkCore;
using PageTurner.Models;

namespace PageTurner.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>()
            .Property(b => b.Status)
            .HasConversion<string>();
    }
}