using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using AAS_Services_Support.ADT_Support;
using AutoMapper;

namespace AAS.API.Repository
{
    public class AASRepositoryFactory
    {
        private readonly IAdtInteractions _adtInteractions;
        private readonly IMapper _mapper;

        public AASRepositoryFactory(IAdtInteractions adtInteractions, IMapper mapper)
        {
            _adtInteractions = adtInteractions;
            _mapper = mapper;
        }
        
        public AASRepository CreateAASRepositoryForADT(string adtInstanceURL)
        {
            var credentials = new DefaultAzureCredential();
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceURL),
                        credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(new HttpClient()) });

            return new ADTAASRepository(client, _adtInteractions,_mapper);
        }
    }
}
