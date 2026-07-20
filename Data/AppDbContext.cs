using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Borrower> Borrowers => Set<Borrower>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<TransactionArchive> TransactionArchives { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Item>()
            .HasIndex(i => i.ItemCode)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();
        modelBuilder.Entity<Item>()
        .HasIndex(i => i.SerialNumber)
        .IsUnique();
    }
}