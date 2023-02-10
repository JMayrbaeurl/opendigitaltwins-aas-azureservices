using System.Net.Http;

namespace AAS.API.Services.ADT
{
    public interface IAzureDigitalTwinsHttpClient
    {
        public HttpClient Client { get; set; }

    }
}
