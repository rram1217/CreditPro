using CreditPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditPro.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<CreditApplication> CreditApplications { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CreditApplication>(entity =>
            {
                entity.ToTable("credit_applications");

                entity.HasKey(e => e.ApplicationId);

                entity.Property(e => e.ApplicationId)
                    .HasColumnName("application_id");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.CreditAmount)
                    .HasColumnName("credit_amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.ApplicationDate)
                    .HasColumnName("application_date")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(50)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(e => e.CollateralDescription)
                    .HasColumnName("collateral_description");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired();

                entity.HasIndex(e => e.CustomerId)
                    .HasDatabaseName("idx_customer_id");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("idx_status");
            });
        }
    }
}