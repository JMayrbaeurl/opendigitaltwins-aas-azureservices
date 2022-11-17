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

        public const string AASREGISTRYDBNAME = "aasregistrydb";

        public const string SHELLSCONTAINERNAME = "shells";

        public const string SUBMODELSCONTAINERNAME = "submodels";

        public CosmosDBAASRegistry(CosmosClient dbClient, ILogger<CosmosDBAASRegistry> log)
        {
            _logger = log;
            _cosmosClient = dbClient;

            if (_cosmosClient != null)
            {
                var database = _cosmosClient.GetDatabase(AASREGISTRYDBNAME);
                _shellsContainer = database.CreateContainerIfNotExistsAsync(SHELLSCONTAINERNAME, "/shellDesc/identification").GetAwaiter().GetResult().Container;
                _submodelsContainer = database.CreateContainerIfNotExistsAsync(SUBMODELSCONTAINERNAME, "/shellDesc/identification").GetAwaiter().GetResult().Container;
            }
        }

        public async Task<AssetAdministrationShellDescriptor> CreateAssetAdministrationShellDescriptor(AssetAdministrationShellDescriptor aasDesc)
        {
            if (aasDesc == null)
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasDesc' must not be empty");

                throw new AASRegistryException($"Parameter 'aasDesc' must not be empty", new ArgumentNullException(nameof(aasDesc)));
            }

            if (string.IsNullOrEmpty(aasDesc.Identification))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasDesc.Identification' must not be empty");

                throw new AASRegistryException($"Parameter 'aasDesc.Identification' must not be empty", new ArgumentNullException(nameof(aasDesc.Identification)));
            }

            if (_shellsContainer == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_shellsContainer' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_shellsContainer' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"CreateAssetAdministrationShellDescriptor called for id '{aasDesc.Identification}'");

            AssetAdministrationShellDescriptor result = null;

            try
            {
                DBAssetAdministrationShellDescriptor dbDesc = await ReadShellDescWithAASId(aasDesc.Identification);
                if (dbDesc == null)
                {
                    dbDesc = new DBAssetAdministrationShellDescriptor(aasDesc);
                    await _shellsContainer.CreateItemAsync<DBAssetAdministrationShellDescriptor>(dbDesc, 
                                                                    new PartitionKey(aasDesc.Identification));
                    result = dbDesc.Desc;
                }
                else
                {
                    _logger?.LogInformation($"Can not create Asset Shell descriptor for '{aasDesc.Identification}', because there's already an entry with this identification stored");
                }
            } 
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while trying to create an entry for '{aasDesc.Identification}': {ex.Message}");

                throw new AASRegistryException($"Exception while trying to create an entry for '{aasDesc.Identification}'", ex);
            }

            return result;
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
                result = (await ReadShellDescWithAASId(aasIdentifier))?.Desc;
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while reading descriptor for id '{aasIdentifier}' from the database: {ex.Message}");

                throw new AASRegistryException($"Exception while reading descriptor for id '{aasIdentifier}' from the database", ex);
            }

            return result;
        }

        public async Task<List<AssetAdministrationShellDescriptor>> GetAllAssetAdministrationShellDescriptors(int maxItems)
        {
            List<AssetAdministrationShellDescriptor> result = new List<AssetAdministrationShellDescriptor> ();

            if (_shellsContainer == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_shellsContainer' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_shellsContainer' configured");
            }

            if (_logger != null)
                _logger.LogTrace("GetAllAssetAdministrationShellDescriptors called");

            try
            {
                var query = new QueryDefinition(query: "SELECT TOP @limit * FROM shells").WithParameter("@limit", maxItems);
                using FeedIterator<DBAssetAdministrationShellDescriptor> feed =
                    _shellsContainer.GetItemQueryIterator<DBAssetAdministrationShellDescriptor>(queryDefinition: query);

                while (feed.HasMoreResults)
                {
                    FeedResponse<DBAssetAdministrationShellDescriptor> response = await feed.ReadNextAsync();
                    foreach (DBAssetAdministrationShellDescriptor item in response)
                    {
                        result.Add(item.Desc);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while reading all shell descriptors from the database: {ex.Message}");

                throw new AASRegistryException($"Exception while reading all shell descriptors from the database: {ex.Message}", ex);
            }

            return result;
        }

        public async Task<AssetAdministrationShellDescriptor> UpdateAssetAdministrationShellDescriptorById(AssetAdministrationShellDescriptor aasDesc)
        {
            if (aasDesc == null)
            {
                _logger?.LogError($"Parameter 'aasDesc' must not be empty");

                throw new AASRegistryException($"Parameter 'aasDesc' must not be empty", new ArgumentNullException(nameof(aasDesc)));
            }

            if (string.IsNullOrEmpty(aasDesc.Identification))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasDesc.Identification' must not be empty");

                throw new AASRegistryException($"Parameter 'aasDesc.Identification' must not be empty", new ArgumentNullException(nameof(aasDesc.Identification)));
            }

            if (_shellsContainer == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_shellsContainer' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_shellsContainer' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"UpdateAssetAdministrationShellDescriptorById called for '{aasDesc?.Identification}'");

            try
            {
                var existingShellDesc = await ReadShellDescWithAASId(aasDesc.Identification);
                if (existingShellDesc == null)
                {
                    _logger?.LogInformation($"UpdateAssetAdministrationShellDescriptorById: No entry found for '{aasDesc?.Identification}'");
                    return null;
                }
                else
                {
                    var newEntry = new DBAssetAdministrationShellDescriptor(aasDesc);
                    await _shellsContainer.ReplaceItemAsync<DBAssetAdministrationShellDescriptor>(
                        newEntry, newEntry.Id, new PartitionKey(aasDesc.Identification));
                    return aasDesc;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Exception while trying to update '{aasDesc.Identification}': {ex.Message}");

                throw new AASRegistryException($"Exception while trying to update '{aasDesc.Identification}': {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAssetAdministrationShellDescriptorById(string aasIdentifier)
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
                        dbDesc.Id, new PartitionKey(dbDesc.Desc.Identification));
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while removing '{aasIdentifier}' from the cache: {ex.Message}");

                throw new AASRegistryException("Exception while removing a value from the cache", ex);
            }
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
