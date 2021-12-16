using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace AAS.API.Services.ADT
{
    public interface DigitalTwinsClientFactory
    {
        public DigitalTwinsClient CreateClient();
    }

    public class StdDigitalTwinsClientFactory : DigitalTwinsClientFactory
    {
        private IConfiguration _config;

        public StdDigitalTwinsClientFactory(IConfiguration config)
        {
            _config = config;
        }

        public DigitalTwinsClient CreateClient()
        {
            var credentials = new DefaultAzureCredential();
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(_config["ADT_SERVICE_URL"]),
                        credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(new HttpClient()) });

            return client;
        }
    }
}
