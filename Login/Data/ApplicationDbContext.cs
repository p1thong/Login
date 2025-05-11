using Login.Models;
using Microsoft.EntityFrameworkCore;

namespace Login.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thêm index cho email để tìm kiếm nhanh hơn
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Thêm index cho GoogleId
            modelBuilder.Entity<User>()
                .HasIndex(u => u.GoogleId);
        }
    }
} 