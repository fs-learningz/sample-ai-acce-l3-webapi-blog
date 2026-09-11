using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlogApi.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) &&
            authHeader["Bearer ".Length..].Trim() == AdminAuthService.BearerToken)
        {
            return;
        }

        context.Result = new UnauthorizedResult();
    }
}
