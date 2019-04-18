using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo.WebApi.Controllers
{    
    using Model;    

    /// <summary>
    /// Контроллер предоставлющий доступ к каталогу товаров.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private CatalogContext Context { get; }
        /// <summary>
        /// Создает и идентифицирует объек типа <see cref="CatalogController"/>.
        /// </summary>
        public CatalogController()
        {
            Context = new CatalogContext();
        }

        /// <summary>
        /// Вовзращает список всех вендоров.
        /// </summary>
        /// <returns></returns>
        [HttpGet("vendors/all")]
        public ActionResult<IEnumerable<Vendor>> GetVendors()
        {
            return Ok(Context.Vendors);
        }

        /// <summary>
        /// Вовзращает вендора по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор вендора.</param>
        /// <returns></returns>
        [HttpGet("vendors/{id}")]
        public ActionResult<Vendor> GetVendor(long id)
        {
            var vendor = Context.Vendors.FirstOrDefault(v => v.Id == id);
            if (vendor == null)
                return NotFound();

            return Ok(vendor);
        }        

        /// <summary>
        /// Возврщает список товаров с пейджинацией.
        /// </summary>
        /// <param name="name">Фильтр по наименованию.</param>
        /// <param name="minPrice">Фильтр по нижней границе стоимости.</param>
        /// <param name="maxPrice">Фильтр по верхней границе стоимости.</param>
        /// <param name="start">Стартовая позиция.</param>
        /// <param name="count">Кол-во выбираемых товаров.</param>
        /// <returns></returns>
        [HttpGet("products/all")]
        public ActionResult<IEnumerable<Product>> GetProducts(string name = null, decimal minPrice = 0M, decimal maxPrice = 0M, 
            [Range(0, int.MaxValue)] int start = 0, [Range(0, int.MaxValue)] int count = 0)
        {
            var products = Context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                products = products.Where(p => p.Name.Contains(name));

            if (minPrice > 0)
                products = products.Where(p => p.Price >= minPrice);

            if (maxPrice > 0)
                products = products.Where(p => p.Price <= maxPrice);

            if (start > 0)
                products = products.Skip(start);

            if (count > 0)
                products = products.Take(count);

            return Ok(Context.Products);
        }

        /// <summary>
        /// Возвращает товар по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("products/{id}")]
        public ActionResult<Product> GetProduct(long id)
        {
            return Ok(Context.Products.FirstOrDefault(p => p.Id == id));
        }

        /// <summary>
        /// Добавялет товар.
        /// </summary>
        /// <param name="product">Новый товар</param>
        /// <returns></returns>
        [HttpPost("products")]
        public ActionResult AddProduct([FromBody]Product product)
        {
            var vendor = Context.Vendors.FirstOrDefault(v => v.Id == product.VendorId);
            if (vendor == null)
                return NotFound($"Vendor with Id {product.VendorId} not found.");

            if (Context.Products.Any(p => p.Name == product.Name))
                return Conflict($"Product with name '{product.Name}' always exists");

            product.Id = GetLastId(Context.Products);

            Context.Products.Add(product);
            Context.SaveChanges();
            return Created($"api/products/{product.Id}", product);
        }

        /// <summary>
        /// Удаляет товар по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>s
        [HttpDelete("products/{id}")]
        public ActionResult DeleteProduct(long id)
        {
            var product = Context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound($"Product with Id {id} not found.");

            Context.Products.Remove(product);
            Context.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Обновляет инфу о товаре.
        /// </summary>
        /// <param name="product">Оновление инфы о товаре.</param>
        /// <returns></returns>
        [HttpPut("products/{id}")]
        public ActionResult UpdateProduct([FromBody]Product product)
        {
            if (!Context.Products.Any(p => p.Id == product.Id))
                return NotFound($"Product with Id {product.Id} not found.");

            Context.Products.Update(product);
            Context.SaveChanges();

            return Ok();
        } 

        /// <summary>
        /// Вовзращает коллекцию фото для товара.
        /// </summary>
        /// <param name="productId">Идентификатор товара.</param>
        /// <returns></returns>
        [HttpGet("products/{productId}/photo/all")]
        public ActionResult<IEnumerable<Photo>> GetPhotos(long productId)
        {
            return Ok(Context.Photos.Where(p => p.ProductId == productId));
        }

        /// <summary>
        /// Возвращает фото по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор фото.</param>
        /// <returns></returns>
        [HttpGet("photo/{id}")]
        public ActionResult<Photo> GetPhoto(long id)
        {
            var photo = Context.Photos.FirstOrDefault(p => p.Id == id);
            if (photo != null)
                return NotFound($"Photo with Id {id} not found.");

            return Ok(photo);
        }

        /// <summary>
        /// Добавляет фото для товара.
        /// </summary>
        /// <param name="photo">Новое фото.</param>
        /// <returns></returns>
        [HttpPost("photo")]
        public ActionResult AddPhoto([FromBody] Photo photo)
        {
            var product = Context.Products.FirstOrDefault(p => p.Id == photo.ProductId);
            if (product == null)
                return NotFound($"Product with Id {photo.ProductId} not found.");

            photo.Id = GetLastId(Context.Photos);            
            
            Context.Photos.Add(photo);
            Context.SaveChanges();

            return Created($"api/photo/{photo.Id}", photo);
        }

        /// <summary>
        /// Удаляет фото по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор фото.</param>
        /// <returns></returns>
        [HttpDelete("photo/{id}")]
        public ActionResult DeletePhoto(long id)
        {
            var photo = Context.Photos.FirstOrDefault(p => p.Id == id);
            if (photo == null)
                return NotFound($"Photo with Id {id} noot found.");

            Context.Photos.Remove(photo);
            Context.SaveChanges();

            return Ok();
        }

        private static long GetLastId<T>(DbSet<T> dbSet) where T:class, IIdentity
        {
            var lastItem = dbSet.OrderByDescending(x => x.Id).FirstOrDefault();
            return lastItem == null ? 1 : lastItem.Id + 1;
        }
    }
}