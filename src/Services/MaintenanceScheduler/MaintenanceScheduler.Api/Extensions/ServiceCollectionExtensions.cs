using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using MaintenanceScheduler.Api.Middleware;

namespace MaintenanceScheduler.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}