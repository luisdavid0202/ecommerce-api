using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.Backend.Api.Helpers
{
    public class ValidateUserAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var requestUserId = context.RouteData.Values["userId"] as string;
            var tokenUserId = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId").Value;

            if (requestUserId != tokenUserId)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
