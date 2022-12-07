using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using AAS_Services_Support.ADT_Support;

namespace AAS.API.Repository
{
    public class AASRepositoryFactory
    {
        private readonly IAdtInteractions _adtInteractions;

        public AASRepositoryFactory(IAdtInteractions adtInteractions)
        {
            _adtInteractions = adtInteractions;
        }
        
        public AASRepository CreateAASRepositoryForADT(string adtInstanceURL)
        {
            var credentials = new DefaultAzureCredential();
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceURL),
                        credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(new HttpClient()) });

            return new ADTAASRepository(client, _adtInteractions);
        }
    }
}
