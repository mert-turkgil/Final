using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            // Relationship to Company (if required)
            // Already configured in CompanyConfiguration, but you can add more constraints here if needed

            // Relationship to MqttTopics
            builder.HasMany(t => t.Topics)
                   .WithOne(tp => tp.MqttTool)
                   .HasForeignKey(tp => tp.MqttToolId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Additional configurations or seed data
        }
    }
}