using AAS.API.Models;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.AASXFile
{
    public class AzureBlobAASXFileService : AASAASXFile
    {
        private BlobServiceClient blobServiceClient; // See https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet#upload-a-blob-to-a-container

        private readonly ILogger _logger;

        private string aasxfilesContainername = "aasxfiles";

        private BlobContainerClient containerClient;

        private bool autoCreateContainer = true;

        public AzureBlobAASXFileService(BlobServiceClient serviceClient, IConfiguration config, ILogger<AzureBlobAASXFileService> logger)
        {
            blobServiceClient = serviceClient;
            _logger = logger;

            if (config != null && config["AASX_FILESERVICE_CONTAINERNAME"] != null)
            {
                aasxfilesContainername = config["AASX_FILESERVICE_CONTAINERNAME"];
            }

            containerClient = serviceClient.GetBlobContainerClient(aasxfilesContainername);
        }

        public async Task<PackageDescription> CreateAASXPackage(List<string> aasIds, byte[] file, string fileName)
        {
            if (containerClient == null)
                throw new AASXFileServiceException("Invalid setup. No Blob container client configured");

            if (fileName == null || fileName.Length == 0)
                throw new AASXFileServiceException("Parameter 'fileName' must not be empty");

            if (file == null || file.Length == 0)
                throw new AASXFileServiceException("Parameter 'file' must not be empty");

            PackageDescription result = CreatePackageDescriptionFor(aasIds, fileName);

            try
            {
                BlobContainerClient blobContainer = await GetContainerClient();
                string containerpath = WebUtility.UrlEncode(Path.GetFileNameWithoutExtension(fileName));

                // First write the Package file
                BlobClient blobClient = blobContainer.GetBlobClient(BuildBlobnameFor(fileName, aasIds));
                await blobClient.UploadAsync(new BinaryData(file));

                IDictionary<string, string> metadata = new Dictionary<string, string>();
                for (int i = 0; i < aasIds.Count; i++)
                    metadata.Add($"aasId_{i}", aasIds[i]);
                await blobClient.SetMetadataAsync(metadata);

                // Second write the aasIdentifiers file
                blobClient = blobContainer.GetBlobClient($"{containerpath}/aasIdentifiers");
                string aasIdsQuotedSep = String.Join(",", aasIds.Select(s => $"\"{s}\"").ToArray());
                await blobClient.UploadAsync(new BinaryData(Encoding.UTF8.GetBytes(aasIdsQuotedSep)));
                BlobProperties properties = await blobClient.GetPropertiesAsync();
                BlobHttpHeaders headers = new BlobHttpHeaders
                {
                    // Set the MIME ContentType every time the properties 
                    // are updated or the field will be cleared
                    ContentType = "text/plain",
                    ContentLanguage = "en-us",

                    // Populate remaining headers with 
                    // the pre-existing properties
                    CacheControl = properties.CacheControl,
                    ContentDisposition = properties.ContentDisposition,
                    ContentEncoding = Encoding.UTF8.EncodingName,
                    ContentHash = properties.ContentHash
                };

                // Set the blob's properties.
                await blobClient.SetHttpHeadersAsync(headers);

            }
            catch(RequestFailedException exc)
            {
                _logger.LogError($"*** Error in accessing Blob:{exc.Status}/{exc.Message}");

                throw new AASXFileServiceException($"*** Error in accessing Blob:{exc.Status}/{exc.Message}");
            }

            return result;
        }

        public async Task DeleteAASXByPackageId(string packageId)
        {
            if (containerClient == null)
                throw new AASXFileServiceException("Invalid setup. No Blob container client configured");

            if (packageId == null || packageId.Length == 0)
                throw new AASXFileServiceException("Parameter 'fileName' must not be empty");

            _logger.LogDebug($"Deleting AASX package with id '{packageId}'");

            try
            {
                BlobContainerClient blobContainer = await GetContainerClient();
                string containerpath = WebUtility.UrlEncode(packageId);

                bool hasDeleted = false;
                await foreach (BlobItem item in blobContainer.GetBlobsAsync(BlobTraits.None, BlobStates.None, $"{containerpath}/"))
                {
                    Response response = await blobContainer.DeleteBlobAsync(item.Name);
                    _logger.LogDebug($"Deleted blob '{item.Name}' from AASX package '{packageId}'");
                    hasDeleted = response.Status == (int)HttpStatusCode.OK;
                }

                if (!hasDeleted)
                    throw new AASXFileServiceException($"No blobs for package with id '{packageId}' found");
            }
            catch (RequestFailedException exc)
            {
                _logger.LogError($"*** Error deleting aasx package '{packageId}':{exc.Status}/{exc.Message}");

                throw new AASXFileServiceException($"*** Error deleting aasx package '{packageId}':{exc.Status}/{exc.Message}");
            }
        }

        public async Task<byte[]> GetAASXByPackageId(string packageId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<PackageDescription>> GetAllAASXPackageIds(string aasId)
        {
            throw new NotImplementedException();
        }

        public async Task<PackageDescription> UpdateAASXPackage(string packageId, List<string> aasIds, byte[] file, string fileName)
        {
            throw new NotImplementedException();
        }

        private PackageDescription CreatePackageDescriptionFor(List<string> aasIds, string fileName)
        {
            PackageDescription result = new PackageDescription() { AasIds = aasIds, PackageId = BuildPackageIdFrom(aasIds, fileName) };

            return result;
        }
        private string BuildBlobnameFor(string fileName, List<string> aasIds = null)
        {
            string containerpath = WebUtility.UrlEncode(Path.GetFileNameWithoutExtension(fileName));
            return $"{containerpath}/{fileName}";
        }

        private string BuildPackageIdFrom(List<string> aasIds, string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        private async Task<BlobContainerClient> GetContainerClient()
        {
            if (autoCreateContainer)
            {
                await containerClient.CreateIfNotExistsAsync();
            }

            return containerClient;
        }

    }
}
