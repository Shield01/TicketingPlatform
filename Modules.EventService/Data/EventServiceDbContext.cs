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
        /// Gets or sets the Users DbSet (for navigation properties).
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the Teams DbSet (for navigation properties).
        /// </summary>
        public DbSet<Team> Teams { get; set; }

        /// <summary>
        /// Configures the model for the EventService module.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Event entity
            modelBuilder.Entity<Event>(entity =>
            {
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
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure relationship with User
                entity.HasOne(e => e.Organizer)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure relationship with Team
                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add indexes for better performance
                entity.HasIndex(e => e.OrganizerId);
                entity.HasIndex(e => e.TeamId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.IsPublic);
            });

            // Configure User entity (for navigation properties)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                
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