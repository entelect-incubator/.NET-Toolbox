namespace PaymentSaga.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PaymentSaga.Domain.Entities;
using PaymentSaga.Domain.ValueObjects;
using PaymentSaga.Infrastructure.Saga;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentSagaState> PaymentSagaStates => Set<PaymentSagaState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.CorrelationId).IsRequired().HasMaxLength(36);
            entity.HasIndex(p => p.CorrelationId).IsUnique();
            entity.Property(p => p.PayerId).IsRequired().HasMaxLength(100);
            entity.Property(p => p.PayeeId).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(p => p.FailureReason).HasMaxLength(1000);

            entity.OwnsOne(p => p.Amount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 4).IsRequired();
                money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
            });
        });

        modelBuilder.Entity<PaymentSagaState>(entity =>
        {
            entity.HasKey(s => s.CorrelationId);
            entity.Property(s => s.CurrentState).IsRequired().HasMaxLength(30);
            entity.Property(s => s.PayerId).IsRequired().HasMaxLength(100);
            entity.Property(s => s.PayeeId).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Amount).HasPrecision(18, 4);
            entity.Property(s => s.Currency).HasMaxLength(3);
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.Property(s => s.ExternalTransactionId).HasMaxLength(100);
            entity.Property(s => s.FailureReason).HasMaxLength(1000);
            entity.Property(s => s.RowVersion).IsRowVersion();
        });
    }
}
