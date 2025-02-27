using System;
using Final.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Final.Data.Configuration
{
    public class MqttToolConfiguration : IEntityTypeConfiguration<MqttTool>
    {
        public void Configure(EntityTypeBuilder<MqttTool> builder)
        {
            builder.ToTable("MqttTools");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            // Use Restrict deletion to avoid an additional cascade path.
            // This means that if a Tool has Topics referencing it, the Tool cannot be deleted until those references are removed.
            builder.HasMany(t => t.Topics)
                   .WithOne(tp => tp.MqttTool)
                   .HasForeignKey(tp => tp.MqttToolId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
