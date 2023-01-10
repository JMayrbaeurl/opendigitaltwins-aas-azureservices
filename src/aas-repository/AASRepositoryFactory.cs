using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;
using System.Net.Http;
using AAS_Services_Support.ADT_Support;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AAS.API.Repository
{
    public class AASRepositoryFactory : IAASRepositoryFactory
    {
        private readonly IAdtInteractions _adtInteractions;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public AASRepositoryFactory(IAdtInteractions adtInteractions, IMapper mapper, ILogger logger)
        {
            _adtInteractions = adtInteractions;
            _mapper = mapper;
            _logger = logger;
        }
        
        public AASRepository CreateAASRepositoryForADT(string adtInstanceURL)
        {
            var credentials = new DefaultAzureCredential();
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceURL),
                        credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(new HttpClient()) });

            return new ADTAASRepository(client, _adtInteractions,_mapper, _logger);
        }
    }
}
