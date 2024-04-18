using Microsoft.EntityFrameworkCore;
using ProductUser.Microservice.Model;

namespace ProductUser.Microservice.Data
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

        public DbSet<ProductOfferDetail> ProductOffers { get; set; }
    }
}
