using System.Net.Http;

namespace AAS.API.Registry.Clients
{
    public class AzureDigitalTwinsHttpClient : IAzureDigitalTwinsHttpClient
    {
        public HttpClient Client { get; set; }
       

        public AzureDigitalTwinsHttpClient(HttpClient client)
        {
            Client = client;
        }

    }
}
