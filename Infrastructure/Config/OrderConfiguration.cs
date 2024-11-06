using Core.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        _ = builder.OwnsOne(x => x.ShippingAddress, o => o.WithOwner());
        _ = builder.OwnsOne(x => x.PaymentSummary, o => o.WithOwner());
        _ = builder.Property(x => x.Status).HasConversion(
            o => o.ToString(),
            o => (OrderStatus)Enum.Parse(typeof(OrderStatus), o)
        );
        _ = builder.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        _ = builder.Property(x => x.Discount).HasColumnType("decimal(18,2)");
        _ = builder.HasMany(x => x.OrderItems).WithOne().OnDelete(DeleteBehavior.Cascade);
        _ = builder.Property(x => x.OrderDate).HasConversion(
            d => d.ToUniversalTime(),
            d => DateTime.SpecifyKind(d, DateTimeKind.Utc)
        );
    }
}
