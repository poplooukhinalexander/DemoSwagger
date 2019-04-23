using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Demo.WebApi.Model
{
    /// <summary>
    /// Контекст каталога.
    /// </summary>
    public class CatalogContext : DbContext
    {

        /// <summary>
        /// Создает и инициализирует объек типа <see cref="CatalogContext"/>.
        /// </summary>
        /// <param name="opt">Настройки для контекста.</param>
        public CatalogContext(DbContextOptions<CatalogContext> opt) : base(opt)
        {
            if (!Users.Any())
            {
                Users.AddRange(new User[]
                {
                    new User {Id = 10, Username = "dark_sidius", Password = "123", Role = "Default"},
                    new User {Id = 11, Username = "r2d2", Password = "010101", Role = "admin"}
                });
                SaveChanges();
            }
        }

        /// <summary>
        /// Вендоры.
        /// </summary>
        public virtual DbSet<Vendor> Vendors { get; set; }

        /// <summary>
        /// Товары.
        /// </summary>
        public virtual DbSet<Product> Products { get; set; }

        /// <summary>
        /// Фото товаров.
        /// </summary>
        public virtual DbSet<Photo> Photos { get; set; }

        /// <summary>
        /// Пользователи.
        /// </summary>
        public virtual DbSet<User> Users { get; set; }
    }
}
