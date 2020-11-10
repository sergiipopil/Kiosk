using Microsoft.AspNetCore.Builder;

namespace KioskBrains.Proxy.Web.ReverseProxy
{
    public static class ReverseProxyMiddlewareExtension
    {
        public static IApplicationBuilder RunReverseProxy(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ReverseProxyMiddleware>();
        }
    }
}