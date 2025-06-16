using Common;
using Microsoft.EntityFrameworkCore;
using Model;

namespace DalLayer
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<BeverageCategory> BeverageCategories { get; set; }
        public DbSet<BeverageDetails> BeverageDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BeverageCategory>()
                .ToTable("BEVERAGE_CATEGORY")
                .HasKey(pk => pk.BEVERAGE_CATEGORY_ID);

            modelBuilder.Entity<ExceptionDetails>().HasNoKey();

            modelBuilder.Entity<BeverageDetails>()
                .ToTable("BEVERAGE_DETAILS")
                .HasKey(pk=>pk.BEVERAGE_DETAILS_ID);
        }

    }
}
