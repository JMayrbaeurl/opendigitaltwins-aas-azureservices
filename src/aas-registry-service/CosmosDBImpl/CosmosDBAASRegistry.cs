using AAS.API.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Container = Microsoft.Azure.Cosmos.Container;

namespace AAS.API.Registry.CosmosDBImpl
{
    public class CosmosDBAASRegistry : AASRegistry
    {
        private readonly ILogger _logger;

        private readonly CosmosClient _cosmosClient;

        private readonly Container _shellsContainer;

        private readonly Container _submodelsContainer;

        public CosmosDBAASRegistry(CosmosClient dbClient, ILogger<CosmosDBAASRegistry> log)
        {
            _logger = log;
            _cosmosClient = dbClient;

            if (_cosmosClient != null)
            {
                var database = _cosmosClient.GetDatabase("aasregistrydb");
                _shellsContainer = database.CreateContainerIfNotExistsAsync("shells", "/shellDesc/identification").GetAwaiter().GetResult().Container;
                _submodelsContainer = database.CreateContainerIfNotExistsAsync("submodels", "/shellDesc/identification").GetAwaiter().GetResult().Container;
            }
        }

        public Task<AssetAdministrationShellDescriptor> CreateAssetAdministrationShellDescriptor(AssetAdministrationShellDescriptor aasDesc)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAssetAdministrationShellDescriptorById(string aasIdentifier)
        {
            if (string.IsNullOrEmpty(aasIdentifier))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasIdentifier' must not be empty");

                throw new AASRegistryException($"Parameter 'aasDesc' must not be empty", new ArgumentNullException(nameof(aasIdentifier)));
            }

            if (_shellsContainer == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_shellsContainer' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_shellsContainer' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"DeleteAssetAdministrationShellDescriptorById called for id '{aasIdentifier}'");

            try
            {
                DBAssetAdministrationShellDescriptor dbDesc = await ReadShellDescWithAASId(aasIdentifier);
                if (dbDesc != null)
                {
                    await _shellsContainer.DeleteItemAsync<DBAssetAdministrationShellDescriptor>(
                        dbDesc.Id, new PartitionKey("/shellDesc/identification"));
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while removing '{aasIdentifier}' from the cache: {ex.Message}");

                throw new AASRegistryException("Exception while removing a value from the cache", ex);
            }
        }

        public async Task<AssetAdministrationShellDescriptor> GetAssetAdministrationShellDescriptorById(string aasIdentifier)
        {
            AssetAdministrationShellDescriptor result = null;

            if (string.IsNullOrEmpty(aasIdentifier))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasIdentifier' must not be empty");

                throw new AASRegistryException($"Parameter 'aasIdentifier' must not be empty", new ArgumentNullException(nameof(aasIdentifier)));
            }

            if (_shellsContainer == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_shellsContainer' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_shellsContainer' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"GetAssetAdministrationShellDescriptorById called with id '{aasIdentifier}'");

            try
            {
                result = (await ReadShellDescWithAASId(aasIdentifier)).Desc;
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while reading descriptor for id '{aasIdentifier}' from the cache: {ex.Message}");

                throw new AASRegistryException($"Exception while reading descriptor for id '{aasIdentifier}' from the cache", ex);
            }

            return result;
        }

        private async Task<DBAssetAdministrationShellDescriptor> ReadShellDescWithAASId(string aasIdentifier)
        {
            DBAssetAdministrationShellDescriptor result = null;

            var query = new QueryDefinition(query: "SELECT * FROM shells where shells.shellDesc.identification = @aasId")
                    .WithParameter("@aasId", aasIdentifier);
            using FeedIterator<DBAssetAdministrationShellDescriptor> feed =
                _shellsContainer.GetItemQueryIterator<DBAssetAdministrationShellDescriptor>(queryDefinition: query);

            while (feed.HasMoreResults && result == null)
            {
                FeedResponse<DBAssetAdministrationShellDescriptor> response = await feed.ReadNextAsync();
                foreach (DBAssetAdministrationShellDescriptor item in response)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }
        public Task<List<AssetAdministrationShellDescriptor>> GetAllAssetAdministrationShellDescriptors()
        {
            throw new NotImplementedException();
        }
        public Task UpdateAssetAdministrationShellDescriptorById(AssetAdministrationShellDescriptor aasDesc, string aasIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<SubmodelDescriptor> CreateSubmodelDescriptor(SubmodelDescriptor submodelDescriptor)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSubmodelDescriptorById(string idsubmodelIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<List<SubmodelDescriptor>> GetAllSubmodelDescriptors()
        {
            throw new NotImplementedException();
        }
        
        public Task<SubmodelDescriptor> GetSubmodelDescriptorById(string submodelIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSubmodelDescriptorById(string submodelIdentifier, SubmodelDescriptor submodelDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}
