using API.FornitureStore.Shared;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace API.FornitureStore.Data
{
    public class APIFornitureStoreContext : IdentityDbContext 
    {
        public APIFornitureStoreContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// SI decidiste usar MYSQL, entonces comenta este función
        /// ya que la configuracion MYSQL ya se hizo en Program.cs,
        /// de lo contrario descoméntala para permitirte usar SQLite.
        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite();
        }*/

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => new
                {
                    od.OrderId,
                    od.ProductId
                });
        }
    }
}
