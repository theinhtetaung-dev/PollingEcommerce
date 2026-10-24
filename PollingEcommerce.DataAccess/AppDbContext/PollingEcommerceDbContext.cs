using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PollingEcommerce.DataAccess.Models;

namespace PollingEcommerce.DataAccess.AppDbContext;

public partial class PollingEcommerceDbContext : DbContext
{
    public PollingEcommerceDbContext(DbContextOptions<PollingEcommerceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<tbl_order> tbl_orders { get; set; }

    public virtual DbSet<tbl_order_item> tbl_order_items { get; set; }

    public virtual DbSet<tbl_payment_transaction> tbl_payment_transactions { get; set; }

    public virtual DbSet<tbl_product> tbl_products { get; set; }

    public virtual DbSet<tbl_product_category> tbl_product_categories { get; set; }

    public virtual DbSet<tbl_user> tbl_users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<tbl_order>(entity =>
        {
            entity.HasKey(e => e.order_id).HasName("PK__tbl_orde__46596229C57FF8E6");

            entity.ToTable("tbl_order");

            entity.HasIndex(e => new { e.user_id, e.add_date }, "IX_order_user").IsDescending(false, true);

            entity.HasIndex(e => e.order_number, "UQ_order_number").IsUnique();

            entity.Property(e => e.add_date)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.modify_date).HasPrecision(0);
            entity.Property(e => e.order_number).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.order_status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("PendingPayment");
            entity.Property(e => e.total_amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.user).WithMany(p => p.tbl_orders)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_user");
        });

        modelBuilder.Entity<tbl_order_item>(entity =>
        {
            entity.HasKey(e => e.order_item_id).HasName("PK__tbl_orde__3764B6BC99DEC7D9");

            entity.ToTable("tbl_order_item");

            entity.HasIndex(e => e.order_id, "IX_item_order");

            entity.Property(e => e.add_date)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.line_total)
                .HasComputedColumnSql("(CONVERT([decimal](18,2),[quantity]*[unit_price]))", true)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.modify_date).HasPrecision(0);
            entity.Property(e => e.unit_price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.order).WithMany(p => p.tbl_order_items)
                .HasForeignKey(d => d.order_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_item_order");

            entity.HasOne(d => d.product).WithMany(p => p.tbl_order_items)
                .HasForeignKey(d => d.product_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_item_product");
        });

        modelBuilder.Entity<tbl_payment_transaction>(entity =>
        {
            entity.HasKey(e => e.payment_transaction_id).HasName("PK__tbl_paym__3DE9AF96D55DFB03");

            entity.ToTable("tbl_payment_transaction");

            entity.HasIndex(e => new { e.payment_status, e.last_polled_date }, "IX_tx_polling");

            entity.HasIndex(e => new { e.order_id, e.attempt_number }, "UQ_tx_attempt").IsUnique();

            entity.HasIndex(e => e.payment_reference, "UX_tx_reference")
                .IsUnique()
                .HasFilter("([payment_reference] IS NOT NULL)");

            entity.Property(e => e.add_date)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.completed_date).HasPrecision(0);
            entity.Property(e => e.failure_reason).HasMaxLength(500);
            entity.Property(e => e.last_polled_date).HasPrecision(0);
            entity.Property(e => e.modify_date).HasPrecision(0);
            entity.Property(e => e.payment_status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.order).WithMany(p => p.tbl_payment_transactions)
                .HasForeignKey(d => d.order_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tx_order");
        });

        modelBuilder.Entity<tbl_product>(entity =>
        {
            entity.HasKey(e => e.product_id).HasName("PK__tbl_prod__47027DF51B86BA76");

            entity.ToTable("tbl_product");

            entity.HasIndex(e => new { e.product_category_id, e.is_active, e.delete_flag }, "IX_product_category");

            entity.Property(e => e.add_date)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.description).HasMaxLength(1000);
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.modify_date).HasPrecision(0);
            entity.Property(e => e.product_name).HasMaxLength(200);
            entity.Property(e => e.unit_price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.product_category).WithMany(p => p.tbl_products)
                .HasForeignKey(d => d.product_category_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_product_category");
        });

        modelBuilder.Entity<tbl_product_category>(entity =>
        {
            entity.HasKey(e => e.product_category_id).HasName("PK__tbl_prod__1F8847F9F2B95D8F");

            entity.ToTable("tbl_product_category");

            entity.HasIndex(e => e.category_name, "UQ_category_name").IsUnique();

            entity.Property(e => e.add_date)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.category_name).HasMaxLength(100);
            entity.Property(e => e.description).HasMaxLength(500);
            entity.Property(e => e.modify_date).HasPrecision(0);
        });

        modelBuilder.Entity<tbl_user>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("PK__tbl_user__B9BE370F68BA69FD");

            entity.ToTable("tbl_user");

            entity.HasIndex(e => e.email, "UQ_user_email").IsUnique();

            entity.Property(e => e.add_date)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.full_name).HasMaxLength(150);
            entity.Property(e => e.modify_date).HasPrecision(0);
            entity.Property(e => e.phone_number).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
