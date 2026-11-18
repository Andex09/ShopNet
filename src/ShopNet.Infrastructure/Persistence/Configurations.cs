using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;
using ShopNet.Domain.Users;

namespace ShopNet.Infrastructure.Persistence;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.HasData(
            new { Id = 1, Name = "Platzi", Price = 400000m },
            new { Id = 2, Name = "Mx 3 Mouse", Price = 350000m },
            new { Id = 3, Name = "Audifonos Logitech", Price = 200000m });
    }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.UserName).HasMaxLength(User.UserNameMaxLength).IsRequired();
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.Property(u => u.Role).HasMaxLength(20).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(64).IsRequired();
        builder.Property(u => u.PasswordSalt).HasMaxLength(128).IsRequired();
    }
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.UnitPrice).HasPrecision(18, 2);
        builder.Property(o => o.Subtotal).HasPrecision(18, 2);
        builder.Property(o => o.TaxRate).HasPrecision(5, 4);
        builder.Property(o => o.Tax).HasPrecision(18, 2);
        builder.Property(o => o.Total).HasPrecision(18, 2);

        // Restrict (not cascade): deleting a user or product must never erase order history.
        builder.HasOne<User>().WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
