using Microsoft.EntityFrameworkCore;
using JsonPlaceholderAPI.Models;
using System;

namespace JsonPlaceholderAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Genellikle burada yapılandırma yapılmaz; appsettings.json ve Program.cs/Startup.cs kullanılır.
            // Ancak, örnek olarak yapılandırma burada da gösterilmektedir.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLazyLoadingProxies()  // Lazy loading'i etkinleştir
                    .UseSqlServer(
                        "Server=localhost;Database=UserDatabase;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True;",
                        sqlServerOptions =>
                        {
                            sqlServerOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorNumbersToAdd: null);
                        });
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Address)
                .WithMany()
                .HasForeignKey(u => u.AddressId);
        }
    }
}
