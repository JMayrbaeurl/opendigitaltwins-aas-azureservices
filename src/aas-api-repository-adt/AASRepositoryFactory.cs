using AAS.ADT;
using AutoMapper;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace AAS.API.Repository.Adt
{
    public class AASRepositoryFactory : IAASRepositoryFactory
    {
        private readonly IAdtAasConnector _adtAasConnector;
        private readonly IMapper _mapper;
        private readonly ILogger<ADTAASRepository> _logger;
        private readonly IAasWriteAssetAdministrationShell _writeShell;
        private readonly IAasDeleteAdt _deleteShell;
        public AASRepositoryFactory(IAdtAasConnector adtAasConnector, IMapper mapper,
            IAasWriteAssetAdministrationShell writeShell, ILogger<ADTAASRepository> logger, IAasDeleteAdt deleteShell)
        {
            _adtAasConnector = adtAasConnector;
            _mapper = mapper;
            _writeShell = writeShell;
            _logger = logger;
            _deleteShell = deleteShell;
        }
        
        public AASRepository CreateAASRepositoryForADT(string adtInstanceURL)
        {
            var credentials = new DefaultAzureCredential();
            DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceURL),
                        credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(new HttpClient()) });

            return new ADTAASRepository(client, _adtAasConnector,_mapper, _logger,_writeShell, _deleteShell);
        }
    }
}
