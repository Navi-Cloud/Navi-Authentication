using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NaviAuth.Extension;
using NaviAuth.Model.Response;
using NaviAuth.Repository;

namespace NaviAuth.Attribute;

[ExcludeFromCodeCoverage]
public class NaviAuthorization : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var accessTokenRepository = httpContext.RequestServices.GetService<IAccessTokenRepository>();

        var accessToken = httpContext.GetAccessToken();
        if (accessToken == null)
        {
            context.Result = new UnauthorizedObjectResult(new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized!",
                DetailedMessage = "This API needs authorization but authorization failed!"
            });

            return;
        }

        var accessTokenObject = accessTokenRepository.FindAccessTokenByTokenAsync(accessToken).GetAwaiter().GetResult();
        httpContext.SetUserId(accessTokenObject.UserId);
        base.OnActionExecuting(context);
    }
}