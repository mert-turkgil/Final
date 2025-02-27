using System;
using Final.Data.Configuration;
using Final.Entity;
using Microsoft.EntityFrameworkCore;

namespace Final.Data
{
    public class ShopContext : DbContext
    {
        // Default constructor
        public ShopContext() { }

        // Constructor to accept options
        public ShopContext(DbContextOptions<ShopContext> options) : base(options) { }

        // DbSet properties for your domain entities.
        public required DbSet<Company> Companies { get; set; }
        public required DbSet<MqttTopic> MqttTopics { get; set; }
        public required DbSet<MqttTool> MqttTools { get; set; }
        public required DbSet<CompanyRole> CompanyRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply your domain configurations.
            modelBuilder.ApplyConfiguration(new CompanyConfiguration());
            modelBuilder.ApplyConfiguration(new MqttToolConfiguration());
            modelBuilder.ApplyConfiguration(new MqttTopicConfiguration());

            // Remove the relationship mapping to ApplicationRole.
            // This tells EF Core to ignore the Role navigation property when creating the model.
            modelBuilder.Entity<CompanyRole>()
                .Ignore(cr => cr.Role);

            // Seed your domain data.
            modelBuilder.Seed();

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Enable lazy loading proxies if needed.
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }
    }
}
