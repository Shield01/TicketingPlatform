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

            // Set default schema for User module
            modelBuilder.HasDefaultSchema("users");

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("app_users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_app_users_email");
                
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);
                
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Attendee");
                
                entity.Property(e => e.EmailVerified)
                    .HasDefaultValue(false);
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");
                
                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // Add indexes for better performance
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.EmailVerified);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
} 