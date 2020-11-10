using Microsoft.AspNetCore.Builder;

namespace KioskBrains.Proxy.Web.Authentication
{
    public static class SimpleKeyAuthenticationMiddlewareExtension
    {
        public static IApplicationBuilder UseSimpleKeyAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SimpleKeyAuthenticationMiddleware>();
        }
    }
}