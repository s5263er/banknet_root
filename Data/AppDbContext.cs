using Microsoft.EntityFrameworkCore;
using BankingApp.Models;

namespace BankingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<BankingApp.Models.User> Users { get; set; }
        public DbSet<BankingApp.Models.UserStock> UserStock { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
