using Microsoft.AspNetCore.Builder;

namespace KioskBrains.Server.Web.Middlewares
{
    public static class WorkerJobsApiMiddlewareExtension
    {
        public static IApplicationBuilder UseWorkerJobsApi(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WorkerJobsApiMiddleware>();
        }
    }
}
