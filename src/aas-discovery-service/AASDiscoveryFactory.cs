using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;
using System.Net.Http;

namespace AAS.API.Discovery
{
    public class AASDiscoveryFactory
    {
        public AASDiscovery CreateAASDiscoveryForADT(string adtInstanceURL)
        {
            // First use DefaultAzureCredentials and second EnvironmentCredential to enable local docker execution
            var credentials = new ChainedTokenCredential(new DefaultAzureCredential(), new EnvironmentCredential());
            
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceURL),
                        credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(new HttpClient()) });

            return new ADTAASDiscovery(client);
        }
    }
}
