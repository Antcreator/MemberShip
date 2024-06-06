using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MemberShip.Models;

namespace MemberShip.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<UserModel>(options)
    {
        public DbSet<Package> Packages { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Package>().HasData(
                new Package { Id = 1, Name = "Bronze", Price = 50000 },
                new Package { Id = 2, Name = "Silver", Price = 100000 },
                new Package { Id = 3, Name = "Gold", Price = 200000 }
            );
        }
    }
}
