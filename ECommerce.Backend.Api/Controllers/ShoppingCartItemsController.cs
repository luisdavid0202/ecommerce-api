using System.Collections.Generic;
using System.Linq;
using ECommerce.Backend.Api.DbContext;
using ECommerce.Backend.Api.Helpers;
using ECommerce.Backend.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShoppingCartItemsController : ControllerBase
    {
        #region Private Attributes
        private readonly ECommerceDbContext _dbContext;
        #endregion

        #region Constructor
        public ShoppingCartItemsController(ECommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion

        #region API
        // GET: api/ShoppingCartItems
        [HttpGet("{userId}")]
        [ValidateUser]
        public IActionResult Get(int userId)
        {
            if (HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId").Value != userId.ToString()) return Unauthorized();

            var user = _dbContext.ShoppingCartItems.Where(s => s.CustomerId == userId);

            if (user == null)
            {
                return NotFound();
            }

            var shoppingCartItems = from s in _dbContext.ShoppingCartItems.Where(s => s.CustomerId == userId)
                                    join p in _dbContext.Products on s.ProductId equals p.Id
                                    select new
                                    {
                                        Id = s.Id,
                                        Price = s.Price,
                                        TotalAmount = s.TotalAmount,
                                        Qty = s.Qty,
                                        ProductName = p.Name
                                    };

            return Ok(shoppingCartItems);
        }

        // GET: api/ShoppingCartItems/SubTotal/5
        [HttpGet("[action]/{userId}")]
        [ValidateUser]
        public IActionResult SubTotal(int userId)
        {
            var subTotal = (from cart in _dbContext.ShoppingCartItems
                            where cart.CustomerId == userId
                            select cart.TotalAmount).Sum();

            return Ok(new { SubTotal = subTotal });
        }

        // GET: api/ShoppingCartItems/TotalItems/5
        [HttpGet("[action]/{userId}")]
        [ValidateUser]
        public IActionResult TotalItems(int userId)
        {
            var cartItems = (from cart in _dbContext.ShoppingCartItems
                             where cart.CustomerId == userId
                             select cart.Qty).Sum();

            return Ok(new { TotalItems = cartItems });
        }

        // POST: api/ShoppingCartItems
        [HttpPost]
        public IActionResult Post([FromBody] ShoppingCartItem shoppingCartItem)
        {
            var shoppingCart = _dbContext.ShoppingCartItems.FirstOrDefault(s => s.ProductId == shoppingCartItem.ProductId && s.CustomerId == shoppingCartItem.CustomerId);

            if (shoppingCart != null)
            {
                shoppingCart.Qty += shoppingCartItem.Qty;
                shoppingCart.TotalAmount = shoppingCart.Price * shoppingCart.Qty;
            }
            else
            {
                var sCart = new ShoppingCartItem()
                {
                    CustomerId = shoppingCartItem.CustomerId,
                    ProductId = shoppingCartItem.ProductId,
                    Price = shoppingCartItem.Price,
                    Qty = shoppingCartItem.Qty,
                    TotalAmount = shoppingCartItem.TotalAmount
                };

                _dbContext.ShoppingCartItems.Add(sCart);
            }

            _dbContext.SaveChanges();

            return StatusCode(StatusCodes.Status201Created);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{userId}")]
        [ValidateUser]
        public IActionResult Delete(int userId)
        {
            var shoppingCart = _dbContext.ShoppingCartItems.Where(s => s.CustomerId == userId);

            _dbContext.ShoppingCartItems.RemoveRange(shoppingCart);
            _dbContext.SaveChanges();

            return NoContent();
        }
        #endregion
    }
}
