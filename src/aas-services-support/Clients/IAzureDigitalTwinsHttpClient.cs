using System.Net.Http;

namespace AAS.API.Services.Clients
{
    public interface IAzureDigitalTwinsHttpClient
    {
        public HttpClient Client { get; set; }

    }
}
