using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PollingPayment.DataAccess.Models;

namespace PollingPayment.DataAccess.AppDbContext;

public partial class PollingPaymentDbContext : DbContext
{
    public PollingPaymentDbContext(DbContextOptions<PollingPaymentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<tbl_payment> tbl_payments { get; set; }

    public virtual DbSet<tbl_payment_event> tbl_payment_events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<tbl_payment>(entity =>
        {
            entity.HasKey(e => e.payment_id).HasName("PK__tbl_paym__ED1FC9EA20C86511");

            entity.ToTable("tbl_payment");

            entity.HasIndex(e => e.merchant_order_number, "IX_payment_merchant");

            entity.HasIndex(e => new { e.payment_status, e.process_after }, "IX_payment_processing");

            entity.HasIndex(e => e.idempotency_key, "UQ_payment_idempotency").IsUnique();

            entity.HasIndex(e => e.payment_reference, "UQ_payment_ref").IsUnique();

            entity.Property(e => e.add_date)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.completed_date).HasPrecision(0);
            entity.Property(e => e.currency_code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("MMK")
                .IsFixedLength();
            entity.Property(e => e.failure_reason).HasMaxLength(500);
            entity.Property(e => e.modify_date).HasPrecision(0);
            entity.Property(e => e.payment_reference).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.payment_status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.process_after).HasPrecision(0);
            entity.Property(e => e.test_mode)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<tbl_payment_event>(entity =>
        {
            entity.HasKey(e => e.payment_event_id).HasName("PK__tbl_paym__BEE2194894F13A0B");

            entity.ToTable("tbl_payment_event");

            entity.HasIndex(e => new { e.payment_id, e.add_date }, "IX_event_payment_date");

            entity.Property(e => e.add_date)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.event_description).HasMaxLength(500);
            entity.Property(e => e.new_status)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.previous_status)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.payment).WithMany(p => p.tbl_payment_events)
                .HasForeignKey(d => d.payment_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_event_payment");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
