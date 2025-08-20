using Microsoft.EntityFrameworkCore;
using Modules.TeamService.Models;
using Modules.UserService.Models;

namespace Modules.TeamService.Data
{
    /// <summary>
    /// Database context for the TeamService module.
    /// </summary>
    public class TeamServiceDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the TeamServiceDbContext.
        /// </summary>
        /// <param name="options">The options for configuring the context.</param>
        public TeamServiceDbContext(DbContextOptions<TeamServiceDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Teams DbSet.
        /// </summary>
        public DbSet<Team> Teams { get; set; }

        /// <summary>
        /// Gets or sets the TeamMembers DbSet.
        /// </summary>
        public DbSet<TeamMember> TeamMembers { get; set; }

        /// <summary>
        /// Gets or sets the Users DbSet (for navigation properties).
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Configures the model for the TeamService module.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Team entity
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);
                
                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(t => t.Description)
                    .HasMaxLength(1000);
                


                // Configure relationship with TeamLeader (User)
                entity.HasOne(t => t.TeamLeader)
                    .WithMany()
                    .HasForeignKey(t => t.TeamLeaderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add indexes for better performance
                entity.HasIndex(t => t.TeamLeaderId);
                entity.HasIndex(t => t.IsActive);
            });

            // Configure TeamMember entity
            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.HasKey(tm => tm.Id);
                
                entity.Property(tm => tm.TeamRole)
                    .IsRequired()
                    .HasMaxLength(50);
                


                // Configure relationship with Team
                entity.HasOne(tm => tm.Team)
                    .WithMany(t => t.TeamMembers)
                    .HasForeignKey(tm => tm.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure relationship with User
                entity.HasOne(tm => tm.User)
                    .WithMany()
                    .HasForeignKey(tm => tm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add unique constraint to prevent duplicate team memberships
                entity.HasIndex(tm => new { tm.TeamId, tm.UserId }).IsUnique();

                // Add indexes for better performance
                entity.HasIndex(tm => tm.TeamId);
                entity.HasIndex(tm => tm.UserId);
                entity.HasIndex(tm => tm.TeamRole);
                entity.HasIndex(tm => tm.IsActive);
            });

            // Configure User entity (for navigation properties)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(u => u.Role)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
} 