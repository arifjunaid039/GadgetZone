using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheGadgetZone.Business.Entities;

namespace TheGadgetZone.DataAccess.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", table =>
            table.HasCheckConstraint("CK_Orders_Subtotal_NonNegative", "[Subtotal] >= 0"));

        builder.HasKey(order => order.Id);

        builder.Property(order => order.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(order => order.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(order => order.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(order => order.DeliveryAddress)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(order => order.PhoneNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(order => new { order.UserId, order.OrderDate });

        builder.HasMany(order => order.OrderItems)
            .WithOne(orderItem => orderItem.Order)
            .HasForeignKey(orderItem => orderItem.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
