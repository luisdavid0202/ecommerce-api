using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationPlugin;
using ECommerce.Backend.Api.DbContext;
using ECommerce.Backend.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Backend.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        #region Private Attributes
        private readonly IConfiguration _configuration;
        private readonly AuthService _auth;
        private readonly ECommerceDbContext _dbContext;
        #endregion

        #region Constructor
        public AccountsController(ECommerceDbContext dbContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _auth = new AuthService(_configuration);
            _dbContext = dbContext;
        }
        #endregion

        #region API
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(User user)
        {
            var userWithSameEmail = _dbContext.Users.SingleOrDefault(u => u.Email == user.Email);

            if (userWithSameEmail != null) return BadRequest("User with this email already exists");

            var userObj = new User
            {
                Name = user.Name,
                Email = user.Email,
                Password = SecurePasswordHasherHelper.Hash(user.Password),
                Role = "User"
            };

            _dbContext.Users.Add(userObj);
            await _dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(User user)
        {
            var userData = _dbContext.Users.FirstOrDefault(u => u.Email == user.Email);

            if (userData == null) return StatusCode(StatusCodes.Status404NotFound);

            var hashedPassword = userData.Password;

            if (!SecurePasswordHasherHelper.Verify(user.Password, hashedPassword)) return Unauthorized();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, userData.Role),
                new Claim("userId", userData.Id.ToString())
            };
            
            var token = _auth.GenerateAccessToken(claims);

            return new ObjectResult(new
            {
                access_token = token.AccessToken,
                token_type = token.TokenType,
                user_Id = userData.Id,
                user_name = userData.Name,
                expires_in = token.ExpiresIn,
                creation_Time = token.ValidFrom,
                expiration_Time = token.ValidTo,
            });
        }
        #endregion
    }
}
