using System;
using System.Collections.Generic;
using System.Net.Http;

namespace GlacierCrates.AspNetCore.ApiProxy
{
    public class ApiProxyOptions
    {
        public IEnumerable<ApiEndpointOption> ProxiedEndpoints { get; set; }
        public HttpMessageHandler BackChannelMessageHandler { get; set; }
    }
}
