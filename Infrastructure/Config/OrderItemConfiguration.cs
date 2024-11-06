using Core.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        _ = builder.OwnsOne(x => x.ItemOrdered, o => o.WithOwner());
        _ = builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
    }
}
