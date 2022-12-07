using System.Net.Http;

namespace AAS.API.Registry.Clients
{
    public interface IAzureDigitalTwinsHttpClient
    {
        public HttpClient Client { get; set; }

    }
}
