using System;
using Final.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Final.Data.Configuration
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.BaseTopic)
                   .IsRequired()
                   .HasMaxLength(200);

            // When a Company is deleted, its Tools are deleted.
            builder.HasMany(c => c.Tools)
                   .WithOne(t => t.Company)
                   .HasForeignKey(t => t.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade);

            // When a Company is deleted, its Topics are deleted.
            builder.HasMany(c => c.Topics)
                   .WithOne(t => t.Company)
                   .HasForeignKey(t => t.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
