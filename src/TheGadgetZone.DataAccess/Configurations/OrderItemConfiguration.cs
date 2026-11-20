using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheGadgetZone.Business.Entities;

namespace TheGadgetZone.DataAccess.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", table =>
        {
            table.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "[UnitPrice] >= 0");
            table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "[Quantity] > 0");
            table.HasCheckConstraint("CK_OrderItems_LineTotal_NonNegative", "[LineTotal] >= 0");
        });

        builder.HasKey(orderItem => orderItem.Id);

        builder.Property(orderItem => orderItem.ProductName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(orderItem => orderItem.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(orderItem => orderItem.LineTotal)
            .HasPrecision(18, 2);

        builder.HasIndex(orderItem => orderItem.ProductId);
    }
}
