using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Symulator_Nozownika.Services
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DemoBlockAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _message;

        public DemoBlockAttribute(string message = "This feature is not available in demo mode")
        {
            _message = message;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isDemo = context.HttpContext.User.FindFirst("IsDemo")?.Value == "true";
            
            if (isDemo)
            {
                context.Result = new JsonResult(new 
                { 
                    success = false, 
                    error = _message 
                }) { StatusCode = 403 };
                return;
            }

            await next();
        }
    }

    public class DemoHelper
    {
        public static bool IsDemo(ClaimsPrincipal user)
        {
            return user.FindFirst("IsDemo")?.Value == "true";
        }

        public static int? GetDemoUserId(ClaimsPrincipal user)
        {
            var isDemo = IsDemo(user);
            if (!isDemo)
                return null;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out int id) ? id : null;
        }
    }
}
