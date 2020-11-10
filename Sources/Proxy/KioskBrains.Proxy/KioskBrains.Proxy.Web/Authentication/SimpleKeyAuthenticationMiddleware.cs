using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace KioskBrains.Proxy.Web.Authentication
{
    public class SimpleKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public SimpleKeyAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            var secretKey = _configuration["ProxyKey"];
            var requestKey = (string)context.Request.Headers["ProxyKey"];

            if (secretKey != requestKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("401 Unauthorized");
            }
            else
            {
                await _next(context);
            }
        }
    }
}