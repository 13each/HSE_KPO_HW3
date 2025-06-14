using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Data
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
            : base(options) { }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(eb =>
            {
                eb.HasKey(a => a.Id);
                eb.HasIndex(a => a.UserId).IsUnique();
            });
            
            modelBuilder.Entity<Payment>(eb =>
                        {
                            eb.HasKey(p => p.Id);
                            eb.Property(p => p.OrderId).IsRequired();
                            eb.Property(p => p.Amount).IsRequired();
                            eb.Property(p => p.Status).IsRequired();
                            eb.Property(p => p.CreatedAt)
                                  .HasDefaultValueSql("NOW()");
                        });

            modelBuilder.Entity<InboxMessage>(eb =>
            {
                eb.HasKey(x => x.Id);
                eb.Property(x => x.ReceivedAt).HasDefaultValueSql("NOW()");
                eb.Property(x => x.Type).IsRequired();
                eb.Property(x => x.Payload).IsRequired();
            });
            modelBuilder.Entity<OutboxMessage>(eb =>
            {
                eb.HasKey(x => x.Id);
                eb.Property(x => x.OccurredAt).HasDefaultValueSql("NOW()");
                eb.Property(x => x.Type).IsRequired();
                eb.Property(x => x.Payload).IsRequired();
            });
        }
    }
}