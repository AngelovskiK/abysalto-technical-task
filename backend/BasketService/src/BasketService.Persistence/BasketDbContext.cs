using BasketService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BasketService.Persistence;

/// <summary>
/// EF Core DbContext for the Basket Service.
/// </summary>
public class BasketDbContext : DbContext
{
    public BasketDbContext(DbContextOptions<BasketDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // ── Cart ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.UserId).IsRequired();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(c => c.UserId).IsUnique();
        });

        // ── CartItem ────────────────────────────────────────────────────────
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.ProductName).IsRequired().HasMaxLength(255);
            entity.Property(ci => ci.UnitPrice).HasPrecision(18, 2);
            entity.Property(ci => ci.ImageUrl).HasMaxLength(2048);

            entity.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
