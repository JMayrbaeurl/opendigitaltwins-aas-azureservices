using AutoMapper;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace AAS.API.Repository.Adt
{
    public class AASRepositoryFactory : IAASRepositoryFactory
    {
        private readonly IAdtInteractions _adtInteractions;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public AASRepositoryFactory(IAdtInteractions adtInteractions, IMapper mapper, ILogger<AASRepositoryFactory> logger)
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
