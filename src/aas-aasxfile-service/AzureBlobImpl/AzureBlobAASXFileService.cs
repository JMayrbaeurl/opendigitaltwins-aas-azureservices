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

        private const string PACKAGE_METADATA_KEY = "package";

        private const string AASIDENTIFIERSBLOBNAME = "aasIdentifiers";

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

        public async Task<PackageDescription> StoreAASXPackage(List<string> aasIds, byte[] file, string fileName)
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
                await UploadPackage(blobClient, file, aasIds);

                // Second write the aasIdentifiers file
                blobClient = blobContainer.GetBlobClient($"{containerpath}/AASIDENTIFIERSBLOBNAME");
                await UploadAASIdentifers(blobClient, aasIds, false);                
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
                    hasDeleted |= response.Status == (int)HttpStatusCode.Accepted;
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
            if (containerClient == null)
                throw new AASXFileServiceException("Invalid setup. No Blob container client configured");

            if (packageId == null || packageId.Length == 0)
                throw new AASXFileServiceException("Parameter 'fileName' must not be empty");

            _logger.LogDebug($"Retrieving AASX package with id '{packageId}'");

            byte[] result = null;

            try
            {
                BlobContainerClient blobContainer = await GetContainerClient();

                BlobItem blob = await FindPackageBlob(blobContainer, packageId);
                if (blob != null)
                {
                    BlobDownloadResult downloadResult = await blobContainer.GetBlobClient(blob.Name).DownloadContentAsync();
                    result = downloadResult.Content.ToArray();
                }
            }
            catch (RequestFailedException exc)
            {
                _logger.LogError($"*** Error retrieving aasx package '{packageId}':{exc.Status}/{exc.Message}");

                throw new AASXFileServiceException($"*** Error retrieving aasx package '{packageId}':{exc.Status}/{exc.Message}");
            }

            return result;
        }

        public async Task<List<PackageDescription>> GetAllAASXPackageIds(string aasId)
        {
            throw new NotImplementedException();
        }

        public async Task<PackageDescription> UpdateAASXPackage(string packageId, List<string> aasIds, byte[] file, string fileName)
        {
            if (containerClient == null)
                throw new AASXFileServiceException("Invalid setup. No Blob container client configured");

            if (packageId == null || packageId.Length == 0)
                throw new AASXFileServiceException("Parameter 'packageId' must not be empty");

            PackageDescription result = null;

            try
            {
                BlobContainerClient blobContainer = await GetContainerClient();
                string containerpath = WebUtility.UrlEncode(packageId);
                BlobClient blobClient;

                if (fileName != null || file != null)
                {
                    BlobItem existingBlob = await FindPackageBlob(blobContainer, packageId);

                    if (fileName != null && file != null)
                    {
                        // First delete old package blob
                        if (existingBlob != null)
                        {
                            await blobContainer.DeleteBlobAsync(existingBlob.Name);
                        }

                        // Then create the new blob
                        blobClient = blobContainer.GetBlobClient($"{containerpath}/{fileName}");
                        await UploadPackage(blobClient, file, aasIds);
                    } else
                    {
                        if (fileName != null)
                        {
                            // See https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-copy?tabs=dotnet
                            //blobClient.StartCopyFromUriAsync();
                        }
                        else
                        {
                            // Just replace contents
                            if (existingBlob != null)
                            {
                                blobClient = blobContainer.GetBlobClient(existingBlob.Name);
                                await UploadPackage(blobClient, file, aasIds, true);
                            }
                        }
                    }
                }

                if (aasIds != null && aasIds.Count > 0)
                {
                    blobClient = blobContainer.GetBlobClient($"{containerpath}/AASIDENTIFIERSBLOBNAME");
                    await UploadAASIdentifers(blobClient, aasIds, true);
                }
            }
            catch (RequestFailedException exc)
            {
                _logger.LogError($"*** Error updating aasx package '{packageId}':{exc.Status}/{exc.Message}");

                throw new AASXFileServiceException($"*** Error updating aasx package '{packageId}':{exc.Status}/{exc.Message}");
            }

            return result;
        }

        private async Task<BlobItem> FindPackageBlob(BlobContainerClient blobContainer, string packageId)
        {
            BlobItem result = null;

            string containerpath = WebUtility.UrlEncode(packageId);

            await foreach (BlobItem item in blobContainer.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, $"{containerpath}/"))
            {
                if (item.Metadata.ContainsKey(PACKAGE_METADATA_KEY) && item.Metadata[PACKAGE_METADATA_KEY] == "true")
                {
                    result = item;
                    break;
                }
            }

            return result;
        }

        private async Task UploadPackage(BlobClient blobClient, byte[] file, List<string> aasIds, bool overwrite = false)
        {
            await blobClient.UploadAsync(new BinaryData(file), overwrite);

            IDictionary<string, string> metadata = new Dictionary<string, string>() { { PACKAGE_METADATA_KEY, "true" } };
            if (aasIds != null)
            {
                for (int i = 0; i < aasIds.Count; i++)
                    metadata.Add($"aasId_{i + 1}", aasIds[i]);
            }

            await blobClient.SetMetadataAsync(metadata);
        }

        private async Task<Response<BlobContentInfo>> UploadAASIdentifers(BlobClient blobClient, List<string> aasIds, bool overwrite)
        {
            string aasIdsQuotedSep = String.Join(",", aasIds.Select(s => $"\"{s}\"").ToArray());
            Response<BlobContentInfo> result = await blobClient.UploadAsync(new BinaryData(Encoding.UTF8.GetBytes(aasIdsQuotedSep)), overwrite);

            if (!overwrite) 
            { 
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

            return result;
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
