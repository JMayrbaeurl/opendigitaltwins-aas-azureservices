using System;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace AAS.API.Services.ADT
{
    public interface DigitalTwinsClientFactory
    {
        public DigitalTwinsClient CreateClient();
    }

    public class StdDigitalTwinsClientFactory : DigitalTwinsClientFactory
    {
        private IConfiguration _config;
        private readonly IAzureDigitalTwinsHttpClient _httpClient;

        public StdDigitalTwinsClientFactory(IConfiguration config, IAzureDigitalTwinsHttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        public DigitalTwinsClient CreateClient()
        {
            var credentials = new DefaultAzureCredential();
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(_config["ADT_SERVICE_URL"]),
                        credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(_httpClient.Client) });

            return client;
        }
    }
}
