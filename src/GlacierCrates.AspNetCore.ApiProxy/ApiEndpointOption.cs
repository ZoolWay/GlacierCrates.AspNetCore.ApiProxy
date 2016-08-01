using System;

namespace GlacierCrates.AspNetCore.ApiProxy
{
    public class ApiEndpointOption
    {
        public string Endpoint { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string TargetEndpoint { get; set; }
    }
}
