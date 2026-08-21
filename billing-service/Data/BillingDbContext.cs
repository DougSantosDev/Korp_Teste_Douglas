using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<InvoiceSequence> InvoiceSequences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>()
            .HasIndex(invoice => invoice.Number)
            .IsUnique();

        modelBuilder.Entity<InvoiceSequence>()
            .HasData(new InvoiceSequence
            {
                Id = 1,
                LastNumber = 0
            });
    }
}
