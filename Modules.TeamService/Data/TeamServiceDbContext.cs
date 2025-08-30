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
        /// Configures the model for the TeamService module.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema for Teams module
            modelBuilder.HasDefaultSchema("teams");

            // Configure Team entity
            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("app_teams");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
                
                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(t => t.Description)
                    .HasMaxLength(1000);

                entity.Property(t => t.IsActive)
                    .HasDefaultValue(true);

                entity.Property(t => t.CreatedAt)
                    .HasDefaultValueSql("NOW()");
                
                entity.Property(t => t.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                // Configure relationship with TeamLeader (User - cross-schema reference)
                // Note: We don't configure the User entity here, just the foreign key
                entity.Property(t => t.TeamLeaderId)
                    .IsRequired();

                // Add indexes for better performance
                entity.HasIndex(t => t.TeamLeaderId);
                entity.HasIndex(t => t.IsActive);
                entity.HasIndex(t => t.Name);
                entity.HasIndex(t => t.CreatedAt);
            });

            // Configure TeamMember entity
            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.ToTable("app_team_members");
                entity.HasKey(tm => tm.Id);
                entity.Property(tm => tm.Id).ValueGeneratedOnAdd();
                
                entity.Property(tm => tm.TeamRole)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(tm => tm.IsActive)
                    .HasDefaultValue(true);

                entity.Property(tm => tm.CreatedAt)
                    .HasDefaultValueSql("NOW()");
                
                entity.Property(tm => tm.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                // Configure relationship with Team (same schema)
                entity.HasOne(tm => tm.Team)
                    .WithMany(t => t.TeamMembers)
                    .HasForeignKey(tm => tm.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure foreign key to User (cross-schema reference)
                // Note: We don't configure the User entity here, just the foreign key
                entity.Property(tm => tm.UserId)
                    .IsRequired();

                // Add unique constraint to prevent duplicate team memberships
                entity.HasIndex(tm => new { tm.TeamId, tm.UserId })
                    .IsUnique()
                    .HasDatabaseName("IX_app_team_members_team_user_unique");

                // Add indexes for better performance
                entity.HasIndex(tm => tm.TeamId);
                entity.HasIndex(tm => tm.UserId);
                entity.HasIndex(tm => tm.TeamRole);
                entity.HasIndex(tm => tm.IsActive);
                entity.HasIndex(tm => tm.CreatedAt);
            });

            // NOTE: We do NOT configure User entity here to avoid table creation conflicts
            // The navigation properties will work via foreign keys without full entity configuration
        }
    }
}