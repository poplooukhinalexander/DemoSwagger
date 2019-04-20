using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;


namespace Demo.WebApi.Controllers
{   
    using Filters;
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
        /// Создает и инициализирует объекти типа <see cref="CatalogController"/>.
        /// </summary>
        /// <param name="context"></param>
        public CatalogController(CatalogContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Вовзращает список всех вендоров.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        [HttpGet("vendors/all")]          
        [AllowAnonymous]
        [SwaggerResponseContentType("application/json", Exclusive = true)]        
        public ActionResult<IEnumerable<Vendor>> GetVendors()
        {
            return Ok(Context.Vendors);
        }

        /// <summary>
        /// Вовзращает вендора по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор вендора.</param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="404">Not Found.</response>
        [HttpGet("vendors/{id}")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]        
        [AllowAnonymous]
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
        /// <response code="200">OK.</response>
        [HttpGet("products/all")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]
        [AllowAnonymous]
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
        /// <response code="200">OK.</response>
        /// <response code="404">Not Found.</response>
        [HttpGet("products/{id}")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]
        [AllowAnonymous]
        public ActionResult<Product> GetProduct(long id)
        {
            var product = Context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound($"Product with id {id} not found.");
            return Ok(product);
        }

        /// <summary>
        /// Добавялет товар.
        /// </summary>
        /// <param name="product">Новый товар</param>
        /// <returns></returns>
        /// <response code="400">Invalid model for Product.</response>
        /// <response code="404">Vendor not found.</response>
        /// <response code="409">Product with same name always exists.</response>       
        [HttpPost("products")]
        [SwaggerResponse(201, Type = typeof(Product), Description = "Product was added")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]
        [Authorize(Roles = "admin")]
        public ActionResult AddProduct([FromBody]Product product)
        {
            var vendor = Context.Vendors.FirstOrDefault(v => v.Id == product.VendorId);
            if (vendor == null)
                return NotFound($"Vendor with Id {product.VendorId} not found.");

            if (Context.Products.Any(p => p.Name == product.Name))
                return Conflict($"Product with name '{product.Name}' always exists");

            product.Validate();

            product.Id = GetLastId(Context.Products);

            Context.Products.Add(product);
            Context.SaveChanges();
            return Created($"api/products/{product.Id}", product);
        }

        /// <summary>
        /// Удаляет товар по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">OK. Was Deleted.</response>
        /// <response code="404">Not Found.</response>
        [HttpDelete("products/{id}")]
        [Authorize(Roles = "admin")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]
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
        /// <response code="200">OK. Was Updated.</response>
        /// <response code="400">Invalid Model for Product.</response>
        /// <response code="404">Not Found.</response>        
        [HttpPut("products/{id}")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]
        [Authorize(Roles = "admin")]
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
        /// <response code="200">OK.</response>
        [HttpGet("products/{productId}/photo/all")]
        [AllowAnonymous]
        [SwaggerResponseContentType("application/json", Exclusive = true)]
        public ActionResult<IEnumerable<Photo>> GetPhotos(long productId)
        {
            return Ok(Context.Photos.Where(p => p.ProductId == productId));
        }

        /// <summary>
        /// Возвращает фото по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор фото.</param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="404">Not Found.</response>
        [HttpGet("photo/{id}")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]
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
        /// <response code="404">Product not found.</response>
        [HttpPost("photo")]
        [Authorize(Roles="admin")]
        [SwaggerResponse(201, Type = typeof(Photo), Description = "Photo was uploaded.")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]       
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
        /// <response code="200">Photo was deleted.</response>
        /// <response code="404">Not Found.</response>
        [HttpDelete("photo/{id}")]
        [Authorize(Roles = "admin")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]
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