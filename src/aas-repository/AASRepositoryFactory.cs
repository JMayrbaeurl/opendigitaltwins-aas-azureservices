using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AAS.API.Repository
{
    public class AASRepositoryFactory
    {
        public AASRepository CreateAASRepositoryForADT(string adtInstanceURL)
        {
            var credentials = new DefaultAzureCredential();
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceURL),
                        credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(new HttpClient()) });

            return new ADTAASRepository(client);
        }
    }
}
