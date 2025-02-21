using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Final.Data.Configuration;
using Final.Entity;
using Microsoft.EntityFrameworkCore;

namespace Final.Data
{
    public class ShopContext : DbContext
    {
        // Default constructor
        public ShopContext()
        {
        }

        // Constructor to accept options
        public ShopContext(DbContextOptions<ShopContext> options) : base(options)
        {
        }
        // DbSet properties for the entities
        public required DbSet<Company> Companies { get; set; }
        public required DbSet<MqttTopic> MqttTopics { get; set; }
        public required DbSet<MqttTool> MqttTools { get; set; }
        public required DbSet<CompanyRole> CompanyRoles { get; set; }
        //public required DbSet<TopicDataType> TopicDataTypes { get; set; } Gereksiz araştır
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CompanyConfiguration());
            modelBuilder.ApplyConfiguration(new MqttToolConfiguration());
            modelBuilder.ApplyConfiguration(new MqttTopicConfiguration());

            // If you have seed data, you can do it here or in the configurations.
            modelBuilder.Seed();
            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Enable lazy loading proxies
            optionsBuilder.UseLazyLoadingProxies();

            // Call base configuration
            base.OnConfiguring(optionsBuilder);
        }
        
    }
}