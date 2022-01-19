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

        public async Task DeleteAssetAdministrationShellDescriptorById(string aasIdentifier)
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
        }

        public async Task<List<AssetAdministrationShellDescriptor>> GetAllAssetAdministrationShellDescriptors()
        {
            throw new NotImplementedException();
        }

        public async Task<AssetAdministrationShellDescriptor> GetAssetAdministrationShellDescriptorById(string aasIdentifier)
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

        public async Task UpdateAssetAdministrationShellDescriptorById(AssetAdministrationShellDescriptor aasDesc, string aasIdentifier)
        {
            if (aasDesc == null)
            {
                if (_logger != null)
                    _logger.LogError($"Parameter 'aasDesc' must not be null");

                throw new AASRegistryException($"Parameter 'aasDesc' must not be null", new ArgumentNullException(nameof(aasDesc)));
            }

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
                _logger.LogTrace($"UpdateAssetAdministrationShellDescriptorById called for AAS Descriptor with id '{aasIdentifier}'");

            try
            {
                await _cache.Set<AssetAdministrationShellDescriptor>(aasIdentifier, aasDesc, new DistributedCacheEntryOptions());

            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError($"Exception while updating '{aasIdentifier}' from the cache: {ex.Message}");

                throw new AASRegistryException("Exception while updating a value in the cache", ex);
            }
        }
    }
}
