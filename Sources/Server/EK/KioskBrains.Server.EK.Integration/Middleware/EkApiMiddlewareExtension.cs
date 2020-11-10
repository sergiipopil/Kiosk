using Microsoft.AspNetCore.Builder;

namespace KioskBrains.Server.EK.Integration.Middleware
{
    public static class EkApiMiddlewareExtension
    {
        public static IApplicationBuilder UseEkApi(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EkApiMiddleware>();
        }
    }
}