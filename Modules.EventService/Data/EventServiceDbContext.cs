using Microsoft.EntityFrameworkCore;
using Modules.EventService.Models;
using Modules.UserService.Models;
using Modules.TeamService.Models;

namespace Modules.EventService.Data
{
    /// <summary>
    /// Database context for the EventService module.
    /// </summary>
    public class EventServiceDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the EventServiceDbContext.
        /// </summary>
        /// <param name="options">The options for configuring the context.</param>
        public EventServiceDbContext(DbContextOptions<EventServiceDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Events DbSet.
        /// </summary>
        public DbSet<Event> Events { get; set; }

        /// <summary>
        /// Configures the model for the EventService module.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema for Events module
            modelBuilder.HasDefaultSchema("events");

            // Configure Event entity
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("app_events");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.Description)
                    .IsRequired();
                
                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(500);
                
                entity.Property(e => e.Category)
                    .HasMaxLength(100);
                
                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Draft");

                entity.Property(e => e.IsPublic)
                    .HasDefaultValue(true);

                entity.Property(e => e.IsPublished)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");
                
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                // Configure foreign keys to other schemas without configuring the entities
                entity.Property(e => e.OrganizerId)
                    .IsRequired();

                entity.Property(e => e.TeamId); // Optional foreign key

                // Add indexes for better performance
                entity.HasIndex(e => e.OrganizerId);
                entity.HasIndex(e => e.TeamId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.IsPublic);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Ignore navigation properties to prevent EF from creating local tables
            modelBuilder.Ignore<User>();
            modelBuilder.Ignore<Team>();
            modelBuilder.Ignore<TeamMember>();
            
            // NOTE: We do NOT configure User or Team entities here to avoid table creation conflicts
            // Cross-schema relationships will be handled via foreign key properties only
        }
    }
}