using Microsoft.EntityFrameworkCore;

namespace Demo.WebApi.Model
{
    public class CatalogContext : DbContext
    {
        public CatalogContext()
        {
            Users.AddRange(new User[] 
            {
                new User { Username = "dark_sidius", Password = "123", Role = "Default" },
                new User { Username = "r2d2", Password = "010101", Role = "Defaul" }
            });
        }

        public CatalogContext(DbContextOptions<CatalogContext> opt) : base(opt) { }

        public virtual DbSet<Vendor> Vendors { get; set; }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<Photo> Photos { get; set; }

        public virtual DbSet<User> Users { get; set; };
    }
}
