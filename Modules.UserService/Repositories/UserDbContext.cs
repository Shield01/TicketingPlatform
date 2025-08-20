using Microsoft.EntityFrameworkCore;
using Modules.UserService.Models;

namespace Modules.UserService.Repositories
{
    /// <summary>
    /// Entity Framework DbContext for user data management.
    /// </summary>
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet for User entities.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Configures the model for the User entity.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.HasIndex(e => e.Email)
                    .IsUnique();
                
                entity.Property(e => e.PasswordHash)
                    .IsRequired();
                
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                
                entity.Property(e => e.UpdatedAt)
                    .IsRequired();
            });
        }
    }
} 