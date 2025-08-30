using Microsoft.EntityFrameworkCore;
using Modules.PaymentService.Models;

namespace Modules.PaymentService.Data
{
    /// <summary>
    /// Database context for the PaymentService module.
    /// </summary>
    public class PaymentServiceDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the PaymentServiceDbContext.
        /// </summary>
        /// <param name="options">The options for configuring the context.</param>
        public PaymentServiceDbContext(DbContextOptions<PaymentServiceDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Payments DbSet.
        /// </summary>
        public DbSet<Payment> Payments { get; set; }

        /// <summary>
        /// Gets or sets the PaymentItems DbSet.
        /// </summary>
        public DbSet<PaymentItem> PaymentItems { get; set; }

        /// <summary>
        /// Configures the model for the PaymentService module.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema for Payments module
            modelBuilder.HasDefaultSchema("payments");

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("app_payments");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();

                entity.Property(p => p.PaymentReference)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.TransactionId)
                    .HasMaxLength(100);

                entity.Property(p => p.Gateway)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(p => p.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                entity.Property(p => p.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");

                entity.Property(p => p.PaymentMethod)
                    .HasMaxLength(50);

                entity.Property(p => p.Description)
                    .HasMaxLength(500);

                entity.Property(p => p.GatewayMetadata)
                    .HasColumnType("jsonb"); // PostgreSQL JSON support

                entity.Property(p => p.IsActive)
                    .HasDefaultValue(true);

                entity.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(p => p.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                // Add unique constraint for payment references
                entity.HasIndex(p => p.PaymentReference)
                    .IsUnique()
                    .HasDatabaseName("IX_app_payments_reference_unique");

                // Add indexes for better performance
                entity.HasIndex(p => p.UserId);
                entity.HasIndex(p => p.EventId);
                entity.HasIndex(p => p.Gateway);
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.IsActive);
                entity.HasIndex(p => p.CreatedAt);
                entity.HasIndex(p => p.CompletedAt);
            });

            // Configure PaymentItem entity
            modelBuilder.Entity<PaymentItem>(entity =>
            {
                entity.ToTable("app_payment_items");
                entity.HasKey(pi => pi.Id);
                entity.Property(pi => pi.Id).ValueGeneratedOnAdd();

                entity.Property(pi => pi.ItemType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Ticket");

                entity.Property(pi => pi.ItemName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(pi => pi.Quantity)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.Property(pi => pi.UnitPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(pi => pi.TotalPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(pi => pi.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");

                entity.Property(pi => pi.IsActive)
                    .HasDefaultValue(true);

                entity.Property(pi => pi.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(pi => pi.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                // Configure relationship with Payment
                entity.HasOne(pi => pi.Payment)
                    .WithMany(p => p.PaymentItems)
                    .HasForeignKey(pi => pi.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add indexes for better performance
                entity.HasIndex(pi => pi.PaymentId);
                entity.HasIndex(pi => pi.ItemType);
                entity.HasIndex(pi => pi.ItemId);
                entity.HasIndex(pi => pi.IsActive);
                entity.HasIndex(pi => pi.CreatedAt);
            });
        }
    }
}
