using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GlacierCrates.AspNetCore.ApiProxy
{
    public class ApiProxyMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ApiProxyOptions options;
        private readonly HttpClient client;

        public ApiProxyMiddleware(RequestDelegate next, IOptions<ApiProxyOptions> options)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (options == null) throw new ArgumentNullException(nameof(options));

            this.next = next;
            this.options = options.Value;

            if (this.options.ProxiedEndpoints?.Count() <= 0) throw new ArgumentException("Options do not contain any proxied endpoints", nameof(options));
            foreach (var ep in this.options.ProxiedEndpoints)
            {
                if (!ep.Endpoint.StartsWith("/")) throw new Exception("Endpoint path must start with a slash '/'");
            }

            this.client = new HttpClient();
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            var matchedEp = this.options.ProxiedEndpoints.FirstOrDefault((ep) => path.StartsWithSegments(ep.Endpoint));
            if (matchedEp == null)
            {
                await this.next(context);
                return;
            }


            var requestMessage = new HttpRequestMessage();

            // copy body (if any)
            if (RequestMethodContainsBody(context.Request))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            // copy the headers
            foreach (var header in context.Request.Headers)
            {
                if ( (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray())) && (requestMessage.Content != null) )
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.Headers.Host = $"{matchedEp.Host}:{matchedEp.Port}";
            string targetPath = context.Request.Path.ToString();
            if (!String.IsNullOrWhiteSpace(matchedEp.TargetEndpoint))
            {
                targetPath = targetPath.Replace(matchedEp.Endpoint, matchedEp.TargetEndpoint);
            }
            string uriString = $"{matchedEp.Scheme ?? context.Request.Scheme}://{matchedEp.Host ?? context.Request.Host.Host ?? "localhost"}:{matchedEp.Port ?? Convert.ToString(context.Request.Host.Port.GetValueOrDefault(80))}{context.Request.PathBase}{targetPath}{context.Request.QueryString}";
            requestMessage.RequestUri = new Uri(uriString);
            requestMessage.Method = new HttpMethod(context.Request.Method);
            using (var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
            {
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                foreach (var header in responseMessage.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (var header in responseMessage.Content.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                context.Response.Headers.Remove("transfer-encoding");
                await responseMessage.Content.CopyToAsync(context.Response.Body);
            }
        }

        private bool RequestMethodContainsBody(HttpRequest request)
        {
            return (!String.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(request.Method, "HEAD", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(request.Method, "DELETE", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(request.Method, "TRACE", StringComparison.OrdinalIgnoreCase));
        }
    }
}
