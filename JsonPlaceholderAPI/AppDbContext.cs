using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;



namespace JsonPlaceholderAPI.Models
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
