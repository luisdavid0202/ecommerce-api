using System;
using System.IO;
using System.Linq;
using ECommerce.Backend.Api.DbContext;
using ECommerce.Backend.Api.Helpers;
using ECommerce.Backend.Api.Models;
using ImageUploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        #region Private Attributes
        private readonly ECommerceDbContext _dbContext;
        #endregion

        #region Constructor
        public ProductsController(ECommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion

        #region API
        #region User
        // GET: api/Products/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var product = _dbContext.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // GET: api/Products/ProductsByCategory/5
        [HttpGet("[action]/{categoryId}")]
        public IActionResult ProductsByCategory(int categoryId)
        {
            var products = from v in _dbContext.Products
                           where v.CategoryId == categoryId
                           select new
                           {
                               Id = v.Id,
                               Name = v.Name,
                               Price = v.Price,
                               Detail = v.Detail,
                               CategoryId = v.CategoryId,
                               ImageUrl = v.ImageUrl
                           };

            return Ok(products);
        }

        // GET: api/Products/PopularProducts
        [HttpGet("[action]")]
        public IActionResult PopularProducts()
        {
            var products = from v in _dbContext.Products
                           where v.IsPopularProduct == true
                           select new
                           {
                               Id = v.Id,
                               Name = v.Name,
                               Price = v.Price,
                               ImageUrl = v.ImageUrl
                           };

            return Ok(products);
        }
        #endregion

        #region Admin
        // GET: api/Products
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_dbContext.Products);
        }

        // POST: api/Products
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromBody] Product product)
        {
            var stream = new MemoryStream(product.ImageArray);
            var guid = Guid.NewGuid().ToString();
            var file = $"{guid}.jpg";
            var folder = "wwwroot";

            var response = FilesHelper.UploadImage(stream, folder, file);

            if (!response)
            {
                return BadRequest();
            }
            else
            {
                product.ImageUrl = file;
                _dbContext.Products.Add(product);
                _dbContext.SaveChanges();

                return StatusCode(StatusCodes.Status201Created, 
                    new 
                    { 
                        Id = product.Id,
                        Name = product.Name,
                        Detail = product.Detail,
                        ImageUrl = product.ImageUrl,
                        Price = product.Price,
                        IsPopularProduct = product.IsPopularProduct,
                        CategoryId = product.CategoryId
                    });
            }
        }

        // PUT: api/Products/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Product product)
        {
            var entity = _dbContext.Products.Find(id);

            if (entity == null)
            {
                return NotFound("No product found against this id...");
            }

            var stream = new MemoryStream(product.ImageArray);
            var guid = Guid.NewGuid().ToString();
            var file = $"{guid}.jpg";
            var folder = "wwwroot";

            var response = FilesHelper.UploadImage(stream, folder, file);

            if (!response)
            {
                return BadRequest();
            }
            else
            {
                var oldImagePath = entity.ImageUrl;

                entity.CategoryId = product.CategoryId;
                entity.Name = product.Name;
                entity.ImageUrl = file;
                entity.Price = product.Price;
                entity.Detail = product.Detail;
                entity.IsPopularProduct = product.IsPopularProduct;
                _dbContext.SaveChanges();

                FilesHelperExtensions.DeleteImage(oldImagePath);

                return Ok(new
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Detail = entity.Detail,
                    ImageUrl = entity.ImageUrl,
                    Price = entity.Price,
                    IsPopularProduct = entity.IsPopularProduct,
                    CategoryId = entity.CategoryId
                });
            }
        }

        // DELETE: api/ApiWithActions/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = _dbContext.Products.Find(id);

            if (product == null)
            {
                return NotFound("No product found against this id...");
            }
            else
            {
                _dbContext.Products.Remove(product);
                _dbContext.SaveChanges();

                FilesHelperExtensions.DeleteImage(product.ImageUrl);

                return NoContent();
            }
        }
        #endregion
        #endregion
    }
}
