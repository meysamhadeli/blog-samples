using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Integration.MassTransit;

public static class DurableLocalQueueModelBuilderExtensions
{
    public static ModelBuilder AddDurableLocalQueueEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DurableMessage>(entity =>
        {
            entity.ToTable("durable_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TypeName).IsRequired().HasMaxLength(512);
            entity.Property(e => e.Payload).IsRequired().HasColumnType("text");
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.EnqueuedAtUtc);
        });
        return modelBuilder;
    }
}
