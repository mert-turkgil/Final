using System;
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

            // The relationships to Company and MqttTool are configured in their respective configurations.
        }
    }
}
