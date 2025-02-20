using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Final.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Final.Data.Configuration
{
   public class MqttTopicConfiguration : IEntityTypeConfiguration<MqttTopic>
    {
        public void Configure(EntityTypeBuilder<MqttTopic> builder)
        {
            builder.ToTable("MqttTopics");
            builder.HasKey(tp => tp.Id);

            builder.Property(tp => tp.BaseTopic)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(tp => tp.TopicTemplate)
                   .IsRequired()
                   .HasMaxLength(300);

            // EF will store the enum as int by default. 
            // You can store as string:
            // builder.Property(tp => tp.DataType).HasConversion<string>();

            // Relationship to MqttTool
            // Already configured in MqttToolConfiguration, but you can add more constraints here if needed
        }
    }
}