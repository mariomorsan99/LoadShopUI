using System.Net;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace Loadshop.DomainServices.Utilities
{
    public class ProxiedHttpMessageHandler : HttpClientHandler
    {
        public ProxiedHttpMessageHandler(IConfiguration config)
        {
            //Only apply the proxied handler in release mode (on the servers)
            UseDefaultCredentials = false;

            var proxyConfig = config["ProxyAddress"];
            if (!string.IsNullOrEmpty(proxyConfig))
            {
                //with proxy
                WebProxy proxy = new WebProxy(proxyConfig, false)
                {
                    UseDefaultCredentials = false
                };
                Proxy = proxy;
            }
        }
    }
}
