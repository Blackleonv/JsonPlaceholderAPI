using Microsoft.EntityFrameworkCore;
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
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=localhost;Database=MyDatabase;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True;",
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
            }
        }

        // DbSet<TEntity> tanımlamaları buraya gelecek
        public DbSet<User> Users { get; set; }
    }
}
