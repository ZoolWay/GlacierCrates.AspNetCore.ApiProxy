using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace GlacierCrates.AspNetCore.ApiProxy
{
    public static class ApiProxyExtension
    {
        public static IApplicationBuilder RunApiProxy(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            return app.UseMiddleware<ApiProxyMiddleware>();
        }

        public static IApplicationBuilder RunApiProxy(this IApplicationBuilder app, ApiProxyOptions options)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (options == null) throw new ArgumentNullException(nameof(options));
            return app.UseMiddleware<ApiProxyMiddleware>(Options.Create(options));
        }
    }
}
