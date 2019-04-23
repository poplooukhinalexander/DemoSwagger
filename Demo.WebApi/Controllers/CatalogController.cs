using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;


namespace Demo.WebApi.Controllers
{
    using Filters;
    using Model;

    /// <summary>
    /// Контроллер предоставлющий доступ к каталогу товаров.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
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
        [Produces("application/json")]
        public ActionResult<IEnumerable<Vendor>> GetVendors()
        {          
            return Ok(Context.Vendors);
        }

        /// <summary>
        /// Возвращает вендора по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор вендора.</param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="404">Вендор не найден.</response>
        [HttpGet("vendors/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Vendor), 200)]
        [SwaggerResponse(200, "OK", Type = typeof(Vendor))]
        [AllowAnonymous]
        public ActionResult<Vendor> GetVendor(long id)
        {
            var vendor = Context.Vendors.FirstOrDefault(v => v.Id == id);
            if (vendor == null)
                return NotFound();

            return Ok(vendor);
        }

        /// <summary>
        /// Добавляет нового вендора.
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        /// <response code="400">Невалидная модель для вендора.</response>
        /// <response code="409">Вендор уже существует.</response>
        [HttpPost("vendors")]
        [Produces("application/json")]
        [Authorize(Roles = "admin")]
        [MapToApiVersion("2.0")]
        [SwaggerResponse(201, Type = typeof(Vendor), Description = "Вендор был добавлен")]
        public ActionResult AddVendor([FromForm] Vendor vendor)
        {
            if (Context.Vendors.Any(v => v.Name.ToLower() == vendor.Name.ToLower()))
                return Conflict($"Vendor with name {vendor.Name} always exists.");

            vendor.Validate();

            vendor.Id = GetLastId(Context.Vendors);
            Context.Vendors.Add(vendor);
            Context.SaveChanges();
            return Created($"/api/v{RouteData.Values["version"]}/vendors/{vendor.Id}", vendor);
        }

        /// <summary>
        /// Добавляет нового вендора.
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        /// <response code="200">Вендор обновлен.</response>
        /// <response code="400">Невалидная модель для вендора.</response>
        /// <response code="404">Вендор не найден.</response>
        /// <response code="409">Вендор уже существует.</response>
        [HttpPut("vendors")]
        [Produces("application/json")]
        [Authorize(Roles = "admin")]
        [MapToApiVersion("2.0")]
        [SwaggerResponse(201, Type = typeof(Vendor), Description = "Вендор был добавлен")]
        public ActionResult UpdateVendor([FromForm] Vendor vendor)
        {
            if (!Context.Vendors.Any(v => v.Id == vendor.Id))
                return NotFound();
            if (Context.Vendors.Any(v => v.Id != vendor.Id && v.Name.ToLower() == vendor.Name.ToLower()))
                return Conflict($"Vendor with name {vendor.Name} always exists.");

            vendor.Validate();

            vendor.Id = GetLastId(Context.Vendors);
            Context.Vendors.Update(vendor);
            Context.SaveChanges();
            return Ok(vendor);
        }

        /// <summary>
        /// Удаляет вендора по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Вендор удален</response>
        /// <response code="404">Вендор не найден.</response>
        [HttpDelete("vendors/{id}")]
        [Authorize(Roles = "admin")]
        [Produces("application/json")]
        [MapToApiVersion("2.0")]
        public ActionResult DeleteVendor(long id)
        {
            var vendor = Context.Vendors.FirstOrDefault(v => v.Id == id);
            if (vendor == null)
                return NotFound($"Vendor with Id {id} not found.");

            Context.Vendors.Remove(vendor);
            Context.SaveChanges();

            return Ok();
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
        [Produces("application/json")]
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
        /// <response code="404">Товар не найден.</response>
        [HttpGet("products/{id}")]
        [Produces("application/json")]
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
        /// <response code="400">Невалидная модель для товара.</response>
        /// <response code="404">Вендор не найден.</response>
        /// <response code="409">Товар уже существует.</response>       
        [HttpPost("products")]
        [SwaggerResponse(201, Type = typeof(Product), Description = "Product was added")]
        [Produces("application/json")]
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
            return Created($"api/v{RouteData.Values["version"]}/products/{product.Id}", product);
        }

        /// <summary>
        /// Удаляет товар по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Товар удален</response>
        /// <response code="404">Товар не найден.</response>
        [HttpDelete("products/{id}")]
        [Authorize(Roles = "admin")]
        [Produces("application/json")]
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
        /// <response code="200">Товар обновлен.</response>
        /// <response code="400">Невалидная модель для товара.</response>
        /// <response code="404">Товар не найден.</response>
        /// <response code="409">Товар уже существует.</response> 
        [HttpPut("products/{id}")]
        [Produces("application/json")]
        [Authorize(Roles = "admin")]
        public ActionResult UpdateProduct([FromBody]Product product)
        {
            if (!Context.Products.Any(p => p.Id == product.Id))
                return NotFound($"Product with Id {product.Id} not found.");

            if (Context.Products.Any(p => p.Id != product.Id && p.Name.ToLower() == product.Name.ToLower()))
                return Conflict($"The product with name {product.Name} always exists.");

            product.Validate();

            Context.Products.Update(product);
            Context.SaveChanges();

            return Ok(product);
        } 

        /// <summary>
        /// Возвращает коллекцию фото для товара.
        /// </summary>
        /// <param name="productId">Идентификатор товара.</param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        [HttpGet("products/{productId}/photo/all")]
        [AllowAnonymous]
        [Produces("application/json")]
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
        /// <response code="404">Фото не найдено.</response>
        [HttpGet("file/{id}")]
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
        /// <param name="productId">Идентификатор товара.</param>
        /// <param name="file">Новое фото.</param>
        /// <param name="description"></param>
        /// <returns></returns>
        /// <response code="404">Товар не найден.</response>
        [HttpPost("products/{productId}/photo")]
        //[Authorize(Roles="admin")]
        [SwaggerResponse(201, Type = typeof(Photo), Description = "Photo was uploaded.")]       
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        public ActionResult AddPhoto(long productId, string description, IFormFile file)
        {
            var product = Context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound($"Product with Id {productId} not found.");

            using (var stream = file.OpenReadStream())
            using (var memoryReader = new MemoryStream())
            {
                stream.CopyTo(memoryReader);
                var photo = new Photo
                {
                    Id = GetLastId(Context.Photos),
                    Description = string.Empty,
                    Extension = Path.GetExtension(file.FileName),
                    ProductId = 10,
                    Content = memoryReader.ToArray()
                };

                return Created($"api/v{RouteData.Values["version"]}/photo/{photo.Id}", photo);
            }                       
        }

        /// <summary>
        /// Удаляет фото по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор фото.</param>
        /// <returns></returns>
        /// <response code="200">Photo was deleted.</response>
        /// <response code="404">Not Found.</response>
        [HttpDelete("file/{id}")]
        [Authorize(Roles = "admin")]
        [SwaggerResponseContentType("application/json", Exclusive = true)]
        public ActionResult DeletePhoto(long id)
        {
            var photo = Context.Photos.FirstOrDefault(p => p.Id == id);
            if (photo == null)
                return NotFound($"Photo with Id {id} not found.");

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