using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            // Primary Key (EF will use Id by convention, but you can specify explicitly)
            builder.HasKey(c => c.Id);

            // Property configurations
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.BaseTopic)
                   .IsRequired()
                   .HasMaxLength(200);

            // Relationships (if any)
            builder.HasMany(c => c.Tools)
                   .WithOne(t => t.Company)
                   .HasForeignKey(t => t.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}