using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Data
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
            : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(eb =>
            {
                eb.HasKey(o => o.Id);
                eb.Property(o => o.Status).HasConversion<string>();
                eb.Property(o => o.CreatedAt).HasDefaultValueSql("NOW()");
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
