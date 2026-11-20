using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheGadgetZone.Business.Entities;

namespace TheGadgetZone.DataAccess.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems", table =>
            table.HasCheckConstraint("CK_CartItems_Quantity_Positive", "[Quantity] > 0"));

        builder.HasKey(cartItem => cartItem.Id);

        builder.Property(cartItem => cartItem.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(cartItem => cartItem.AddedAt)
            .IsRequired();

        builder.HasIndex(cartItem => new { cartItem.UserId, cartItem.ProductId })
            .IsUnique();
    }
}
