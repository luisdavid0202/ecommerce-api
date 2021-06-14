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
    public class CategoriesController : ControllerBase
    {
        #region Private Attributes
        private readonly ECommerceDbContext _dbContext;
        #endregion

        #region Constructor
        public CategoriesController(ECommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion

        #region API
        #region User
        // GET: api/Categories
        [HttpGet]
        public IActionResult Get()
        {
            var categories = from c in _dbContext.Categories
                             select new
                             {
                                 Id = c.Id,
                                 Name = c.Name,
                                 ImageUrl = c.ImageUrl
                             };

            return Ok(categories);
        }
        #endregion

        #region Admin
        // GET: api/Categories/5
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var category = (from c in _dbContext.Categories
                            where c.Id == id
                            select new
                            {
                                Id = c.Id,
                                Name = c.Name,
                                ImageUrl = c.ImageUrl
                            }).FirstOrDefault();

            return Ok(category);
        }

        // POST: api/Categories
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromBody] Category category)
        {
            var stream = new MemoryStream(category.ImageArray);
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
                category.ImageUrl = file;
                _dbContext.Categories.Add(category);
                _dbContext.SaveChanges();

                return StatusCode(StatusCodes.Status201Created,
                    new
                    {
                        Id = category.Id,
                        Name = category.Name,
                        ImageUrl = category.ImageUrl
                    });
            }
        }

        // PUT: api/Categories/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Category category)
        {
            var entity = _dbContext.Categories.Find(id);

            if (entity == null)
            {
                return NotFound("No category found against this id...");
            }

            var stream = new MemoryStream(category.ImageArray);
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

                entity.Name = category.Name;
                entity.ImageUrl = file;
                _dbContext.SaveChanges();

                FilesHelperExtensions.DeleteImage(oldImagePath);

                return Ok(new { Id = entity.Id, Name = entity.Name, ImageUrl = entity.ImageUrl });
            }
        }

        // DELETE: api/ApiWithActions/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var category = _dbContext.Categories.Find(id);

            if (category == null)
            {
                return NotFound("No category found against this id...");
            }
            else
            {
                _dbContext.Categories.Remove(category);
                _dbContext.SaveChanges();

                FilesHelperExtensions.DeleteImage(category.ImageUrl);

                return NoContent();
            }
        }
        #endregion
        #endregion        
    }
}
