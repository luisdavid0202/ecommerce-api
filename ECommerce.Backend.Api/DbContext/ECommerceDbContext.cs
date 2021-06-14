using ECommerce.Backend.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Backend.Api.DbContext
{
    public class ECommerceDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) 
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}
