using System;
using System.IO;
using System.Threading.Tasks;
using KioskBrains.Waf.Actions.Processing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace KioskBrains.Waf.Extensions
{
    public class WafWebApplicationBuilder
    {
        private readonly IApplicationBuilder _applicationBuilder;

        internal WafWebApplicationBuilder(IApplicationBuilder applicationBuilder)
        {
            _applicationBuilder = applicationBuilder;
        }

        internal void AddWafApiRoutes()
        {
            var actionWebProcessor = _applicationBuilder.ApplicationServices.GetService<WafActionWebProcessor>();
            if (actionWebProcessor == null)
            {
                throw new WafStartupException($"'{nameof(WafExtensions.UseWaf)}' can be invoked only with '{nameof(WafHostEnum.Web)}' host ('{nameof(WafExtensions.AddWaf)}' parameter).");
            }
            actionWebProcessor.AddWafApiRoutes(_applicationBuilder);
        }

        /// <summary>
        /// If GET request doesn't target file, "/index.html" content is returned.
        /// </summary>
        public WafWebApplicationBuilder UseWafSpa()
        {
            _applicationBuilder.Use(async (httpContext, next) =>
                {
                    if (httpContext.Request.Method == "GET"
                        && !Path.HasExtension(httpContext.Request.Path.Value))
                    {
                        await WriteWafSpaAsync(httpContext);
                    }
                    else
                    {
                        await next();
                    }
                });
            return this;
        }

        private async Task WriteWafSpaAsync(HttpContext httpContext)
        {
            var memoryCache = httpContext.RequestServices.GetService<IMemoryCache>();
            if (memoryCache == null)
            {
                throw new InvalidOperationException($"{nameof(IMemoryCache)} is not provided.");
            }

            var spaFileContent = await memoryCache.GetOrCreateAsync(
                "WafSpaFile",
                async cacheEntry =>
                    {
                        try
                        {
                            var hostingEnvironment = httpContext.RequestServices.GetService<IHostingEnvironment>();
                            var fileProvider = hostingEnvironment.WebRootFileProvider;
                            var fileInfo = fileProvider.GetFileInfo("/index.html");
                            using (var streamReader = new StreamReader(fileInfo.CreateReadStream()))
                            {
                                return await streamReader.ReadToEndAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    });

            httpContext.Response.ContentType = "text/html";
            await httpContext.Response.WriteAsync(spaFileContent);
        }
    }
}