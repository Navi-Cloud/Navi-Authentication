using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace NaviAuth.Extension;

[ExcludeFromCodeCoverage]
public static class HttpContextExtension
{
    private const string UserIdKey = "userId";

    public static string? GetAccessToken(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
        {
            return null;
        }

        var authHeader = authHeaderValue.First();
        var regexMatch = Regex.Match(authHeader, "[Bb]earer (.+)");

        if (!regexMatch.Success) return null;

        return regexMatch.Groups[1].Value;
    }

    public static void SetUserId(this HttpContext httpContext, string userId)
    {
        httpContext.Items[UserIdKey] = userId;
    }

    public static string GetUserId(this HttpContext httpContext)
    {
        return httpContext.Items[UserIdKey] as string;
    }
}