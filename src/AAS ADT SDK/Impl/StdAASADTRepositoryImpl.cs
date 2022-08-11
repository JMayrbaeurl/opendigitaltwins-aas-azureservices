using AAS.ADT.Models;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.ADT.Impl
{
    public class StdAASADTRepositoryImpl : IAASADTRepository
    {
        private DigitalTwinsClient dtClient;

        protected readonly ILogger _logger;

        public StdAASADTRepositoryImpl(ILogger logger)
        {
            _logger = logger;
        }

        public StdAASADTRepositoryImpl(DigitalTwinsClient client)
        {
            dtClient = client;
        }

        public async Task<AssetAdministrationShell> ReadAASWithDtIdAsync(string dtId)
        {
            return await this.dtClient.GetDigitalTwinAsync<AssetAdministrationShell>(dtId);
        }

        public Task<AssetInformation> ReadAssetInformationForAAS(string aasDtId)
        {
            throw new NotImplementedException();
        }
    }
}
