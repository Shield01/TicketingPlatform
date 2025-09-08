using Microsoft.EntityFrameworkCore;
using Modules.TicketService.Models;

namespace Modules.TicketService.Data
{
    /// <summary>
    /// Database context for the TicketService module.
    /// </summary>
    public class TicketServiceDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the TicketServiceDbContext.
        /// </summary>
        /// <param name="options">The options for configuring the context.</param>
        public TicketServiceDbContext(DbContextOptions<TicketServiceDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Tickets DbSet.
        /// </summary>
        public DbSet<Ticket> Tickets { get; set; }

        /// <summary>
        /// Gets or sets the TicketTiers DbSet.
        /// </summary>
        public DbSet<TicketTier> TicketTiers { get; set; }

        /// <summary>
        /// Configures the model for the TicketService module.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema for Tickets module
            modelBuilder.HasDefaultSchema("tickets");

            // Configure TicketTier entity
            modelBuilder.Entity<TicketTier>(entity =>
            {
                entity.ToTable("app_ticket_tiers");
                entity.HasKey(tt => tt.Id);
                entity.Property(tt => tt.Id).ValueGeneratedOnAdd();

                entity.Property(tt => tt.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(tt => tt.Description)
                    .HasMaxLength(500);

                entity.Property(tt => tt.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(tt => tt.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                entity.Property(tt => tt.MaxQuantity)
                    .IsRequired();

                entity.Property(tt => tt.SoldQuantity)
                    .HasDefaultValue(0);

                entity.Property(tt => tt.IsAvailable)
                    .HasDefaultValue(true);

                entity.Property(tt => tt.IsActive)
                    .HasDefaultValue(true);

                entity.Property(tt => tt.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(tt => tt.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                // Add unique constraint for tier names per event
                entity.HasIndex(tt => new { tt.EventId, tt.Name })
                    .IsUnique()
                    .HasDatabaseName("IX_app_ticket_tiers_event_name_unique");

                // Add indexes for better performance
                entity.HasIndex(tt => tt.EventId);
                entity.HasIndex(tt => tt.IsAvailable);
                entity.HasIndex(tt => tt.IsActive);
                entity.HasIndex(tt => tt.Price);
                entity.HasIndex(tt => tt.CreatedAt);
            });

            // Configure Ticket entity
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("app_tickets");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();

                // TicketTierId is configured as foreign key through relationships below

                entity.Property(t => t.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(t => t.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                entity.Property(t => t.TicketCode)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(t => t.QRCodeData)
                    .HasMaxLength(1000);

                entity.Property(t => t.IsUsed)
                    .HasDefaultValue(false);

                entity.Property(t => t.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("UNUSED");

                entity.Property(t => t.PaymentId);

                entity.Property(t => t.IsActive)
                    .HasDefaultValue(true);

                entity.Property(t => t.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(t => t.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                // Add unique constraint for ticket codes
                entity.HasIndex(t => t.TicketCode)
                    .IsUnique()
                    .HasDatabaseName("IX_app_tickets_ticket_code_unique");

                // Configure foreign key relationship to TicketTier
                entity.HasOne(t => t.TicketTier)
                    .WithMany(tt => tt.Tickets)
                    .HasForeignKey(t => t.TicketTierId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add indexes for better performance
                entity.HasIndex(t => t.EventId);
                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.TicketTierId);
                entity.HasIndex(t => t.PaymentId);
                entity.HasIndex(t => t.IsUsed);
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => t.IsActive);
                entity.HasIndex(t => t.CreatedAt);
            });
        }
    }
}
