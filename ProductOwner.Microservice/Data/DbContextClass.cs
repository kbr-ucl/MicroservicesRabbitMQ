using Microsoft.EntityFrameworkCore;
using ProductOwner.Microservice.Model;

namespace ProductOwner.Microservice.Data
{
    public class DbContextClass : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DbContextClass(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<ProductDetails> Products { get; set; }
    }
}
