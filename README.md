# GlacierCrates.AspNetCore.ApiProxy

ASP.NET Core middleware to establish endpoints which proxy to another server, e.g. forward API calls.

Inspired by 
* https://github.com/aspnet/Proxy (Apache 2 License)
* http://stackoverflow.com/questions/34932382/creating-an-api-proxy-in-asp-net-mvc-6

Freely available under the terms of the MIT license.

## Installation

You can install *GlacierCrates.AspNetCore.ApiProxy* via nuget using the UI or the Package Manager Console

    Install-Package GlacierCrates.AspNetCore.ApiProxy

## Configuration

In your `Configure` method you can setup the proxy like this:

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        loggerFactory.AddConsole(Configuration.GetSection("Logging"));
        loggerFactory.AddDebug();

        var options = new ApiProxyOptions();
        options.ProxiedEndpoints = new[] { new ApiEndpointOption() { Endpoint = "/api2", Host = "localhost", TargetEndpoint = "/api" } };
        app.RunApiProxy(options);

        app.UseMvc();
    }

For the endpoint options all parameters except `Endpoint` are optional and default to the current request or localhost or port 80.
