using AAS.API.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AAS.API.Registry
{
    public class RedisAASRegistry : AASRegistry
    {
        private readonly ILogger _logger;

        private readonly Cache _cache;

        public RedisAASRegistry(IDistributedCache cacheProvider, ILogger<RedisAASRegistry> log)
        {
            _logger = log;

            _cache = new Cache(cacheProvider);  
        }

        public async Task<AssetAdministrationShellDescriptor> CreateAssetAdministrationShellDescriptor(AssetAdministrationShellDescriptor aasDesc)
        {
            if (aasDesc == null)
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasDesc' must not be null");

                throw new AASRegistryException($"Parameter 'aasDesc' must not be null", new ArgumentNullException(nameof(aasDesc)));
            }

            if (_cache == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_cache' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_cache' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"CreateAssetAdministrationShellDescriptor called for AAS Descriptor with id '{aasDesc.Identification}'");

            try
            {
                await _cache.Set<AssetAdministrationShellDescriptor>($"aas_{aasDesc.Identification}", aasDesc, new DistributedCacheEntryOptions());

            } catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while setting a value in the cache: {ex.Message}");

                throw new AASRegistryException("Exception while setting a value in the cache", ex);
            }

            return aasDesc;
        }

        public async Task<SubmodelDescriptor> CreateSubmodelDescriptor(SubmodelDescriptor submodelDescriptor)
        {
            if (submodelDescriptor == null)
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'submodelDescriptor' must not be null");

                throw new AASRegistryException($"Parameter 'submodelDescriptor' must not be null", new ArgumentNullException(nameof(submodelDescriptor)));
            }

            if (_cache == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_cache' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_cache' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"CreateSubmodelDescriptor called for submodel Descriptor with id '{submodelDescriptor.Identification}'");

            try
            {
                await _cache.Set<SubmodelDescriptor>($"sm_{submodelDescriptor.Identification}", submodelDescriptor, new DistributedCacheEntryOptions());

            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while setting a value in the cache: {ex.Message}");

                throw new AASRegistryException("Exception while setting a value in the cache", ex);
            }

            return submodelDescriptor;
        }

        public async Task<bool> DeleteAssetAdministrationShellDescriptorById(string aasIdentifier)
        {
            if (string.IsNullOrEmpty(aasIdentifier))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasIdentifier' must not be empty");

                throw new AASRegistryException($"Parameter 'aasDesc' must not be empty", new ArgumentNullException(nameof(aasIdentifier)));
            }

            if (_cache == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_cache' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_cache' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"DeleteAssetAdministrationShellDescriptorById called for id '{aasIdentifier}'");

            try
            {
                await _cache.Clear($"aas_{aasIdentifier}");

            } catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while removing '{aasIdentifier}' from the cache: {ex.Message}");

                throw new AASRegistryException("Exception while removing a value from the cache", ex);
            }

            return true;
        }

        public async Task DeleteSubmodelDescriptorById(string submodelIdentifier)
        {
            if (string.IsNullOrEmpty(submodelIdentifier))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'submodelIdentifier' must not be empty");

                throw new AASRegistryException($"Parameter 'submodelIdentifier' must not be empty", new ArgumentNullException(nameof(submodelIdentifier)));
            }

            if (_cache == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_cache' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_cache' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"DeleteSubmodelDescriptorById called with id '{submodelIdentifier}'");

            try
            {
                await _cache.Clear($"sm_{submodelIdentifier}");

            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while deleting submodel descriptor for id '{submodelIdentifier}' from the cache: {ex.Message}");

                throw new AASRegistryException($"Exception while deleting submodel descriptor for id '{submodelIdentifier}' from the cache", ex);
            }

        }

        public async Task<List<AssetAdministrationShellDescriptor>> GetAllAssetAdministrationShellDescriptors(int maxItems)
        {
            throw new NotImplementedException();
        }

        public async Task<List<SubmodelDescriptor>> GetAllSubmodelDescriptors()
        {
            throw new NotImplementedException();
        }

        public async Task<AssetAdministrationShellDescriptor> GetAssetAdministrationShellDescriptorById(string aasIdentifier)
        {
            if (string.IsNullOrEmpty(aasIdentifier))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasIdentifier' must not be empty");

                throw new AASRegistryException($"Parameter 'aasIdentifier' must not be empty", new ArgumentNullException(nameof(aasIdentifier)));
            }

            if (_cache == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_cache' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_cache' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"GetAssetAdministrationShellDescriptorById called with id '{aasIdentifier}'");

            try
            {
                return await _cache.Get<AssetAdministrationShellDescriptor>($"aas_{aasIdentifier}");

            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while reading descriptor for id '{aasIdentifier}' from the cache: {ex.Message}");

                throw new AASRegistryException($"Exception while reading descriptor for id '{aasIdentifier}' from the cache", ex);
            }
        }

        public async Task<SubmodelDescriptor> GetSubmodelDescriptorById(string submodelIdentifier)
        {
            if (string.IsNullOrEmpty(submodelIdentifier))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'submodelIdentifier' must not be empty");

                throw new AASRegistryException($"Parameter 'submodelIdentifier' must not be empty", new ArgumentNullException(nameof(submodelIdentifier)));
            }

            if (_cache == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_cache' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_cache' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"GetSubmodelDescriptorById called with id '{submodelIdentifier}'");

            try
            {
                return await _cache.Get<SubmodelDescriptor>($"sm_{submodelIdentifier}");

            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while reading submodel descriptor for id '{submodelIdentifier}' from the cache: {ex.Message}");

                throw new AASRegistryException($"Exception while reading submodel descriptor for id '{submodelIdentifier}' from the cache", ex);
            }
        }

        public async Task<AssetAdministrationShellDescriptor> UpdateAssetAdministrationShellDescriptorById(AssetAdministrationShellDescriptor aasDesc)
        {
            if (aasDesc == null)
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasDesc' must not be null");

                throw new AASRegistryException($"Parameter 'aasDesc' must not be null", new ArgumentNullException(nameof(aasDesc)));
            }

            if (string.IsNullOrEmpty(aasDesc.Identification))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasIdentifier' must not be empty");

                throw new AASRegistryException($"Parameter 'aasIdentifier' must not be empty", new ArgumentNullException(nameof(aasDesc.Identification)));
            }

            if (_cache == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_cache' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_cache' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"UpdateAssetAdministrationShellDescriptorById called for AAS Descriptor with id '{aasDesc.Identification}'");

            try
            {
                await _cache.Set<AssetAdministrationShellDescriptor>(aasDesc.Identification, aasDesc, new DistributedCacheEntryOptions());
                return aasDesc;
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while updating '{aasDesc.Identification}' from the cache: {ex.Message}");

                throw new AASRegistryException("Exception while updating a value in the cache", ex);
            }
        }

        public async Task UpdateSubmodelDescriptorById(string submodelIdentifier, SubmodelDescriptor submodelDescriptor)
        {
            if (submodelDescriptor == null)
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'submodelDescriptor' must not be null");

                throw new AASRegistryException($"Parameter 'submodelDescriptor' must not be null", new ArgumentNullException(nameof(submodelDescriptor)));
            }

            if (string.IsNullOrEmpty(submodelIdentifier))
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'submodelIdentifier' must not be empty");

                throw new AASRegistryException($"Parameter 'submodelIdentifier' must not be empty", new ArgumentNullException(nameof(submodelIdentifier)));
            }

            if (_cache == null)
            {
                if (_logger != null)
                    _logger.LogError("Wrong DI configuration. No '_cache' configured");

                throw new AASRegistryException("Wrong DI configuration. No '_cache' configured");
            }

            if (_logger != null)
                _logger.LogTrace($"UpdateSubmodelDescriptorById called for Submodel Descriptor with id '{submodelIdentifier}'");

            try
            {
                await _cache.Set<SubmodelDescriptor>(submodelIdentifier, submodelDescriptor, new DistributedCacheEntryOptions());

            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while updating '{submodelIdentifier}' from the cache: {ex.Message}");

                throw new AASRegistryException("Exception while updating a value in the cache", ex);
            }
        }
    }
}
