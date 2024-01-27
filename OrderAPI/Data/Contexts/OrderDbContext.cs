using Microsoft.EntityFrameworkCore;
using OrderAPI.Models;

namespace OrderAPI.Data.Contexts
{
    public class OrderDbContext: DbContext
    {
        public OrderDbContext(DbContextOptions options): base(options)
        {

        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
