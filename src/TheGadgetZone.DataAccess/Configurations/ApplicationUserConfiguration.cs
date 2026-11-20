using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheGadgetZone.DataAccess.Identity;

namespace TheGadgetZone.DataAccess.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasMany(user => user.CartItems)
            .WithOne()
            .HasForeignKey(cartItem => cartItem.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Orders)
            .WithOne()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
