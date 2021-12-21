using AAS.API.Models;
using AAS.API.Models.Interfaces;
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

        private const string PACKAGE_FILENAME_KEY = "filename";

        private const string PACKAGE_BLOBNAME = "Package.aasx";

        private bool createExtraAASIdentifierBlob;

        public bool CreateExtraAASIdentifierBlob
        {
            get { return createExtraAASIdentifierBlob; }
            set { createExtraAASIdentifierBlob = value; }
        }


        private const string AASIDENTIFIERSBLOBNAME = "aasIdentifiers";

        public bool AutoCreateContainer { get => autoCreateContainer; set => autoCreateContainer = value; }

        public AzureBlobAASXFileService(BlobServiceClient serviceClient, IConfiguration config, ILogger<AzureBlobAASXFileService> logger)
        {
            blobServiceClient = serviceClient;
            _logger = logger;

            createExtraAASIdentifierBlob = false;

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
                string containerpath = WebUtility.UrlEncode(result.PackageId);

                // First write the Package file
                BlobClient blobClient = blobContainer.GetBlobClient($"{containerpath}/{PACKAGE_BLOBNAME}");
                await UploadPackage(blobClient, fileName, file, aasIds);

                // Second write the aasIdentifiers file
                if (CreateExtraAASIdentifierBlob)
                { 
                    blobClient = blobContainer.GetBlobClient($"{containerpath}/{AASIDENTIFIERSBLOBNAME}");
                    await UploadAASIdentifers(blobClient, aasIds, false);
                }
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

        public async Task<PackageFile> GetAASXByPackageId(string packageId)
        {
            if (containerClient == null)
                throw new AASXFileServiceException("Invalid setup. No Blob container client configured");

            if (packageId == null || packageId.Length == 0)
                throw new AASXFileServiceException("Parameter 'fileName' must not be empty");

            _logger.LogDebug($"Retrieving AASX package with id '{packageId}'");

            PackageFile result = null;

            try
            {
                BlobContainerClient blobContainer = await GetContainerClient();
                string containerpath = WebUtility.UrlEncode(packageId);

                BlobClient blobClient = blobContainer.GetBlobClient($"{containerpath}/{PACKAGE_BLOBNAME}");
                if (await blobClient.ExistsAsync())
                {
                    BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                    string filename = (await blobClient.GetPropertiesAsync()).Value.Metadata[PACKAGE_FILENAME_KEY];
                    result = new PackageFile() { Filename = filename, Contents = downloadResult.Content.ToArray() };
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
            List<PackageDescription> result = new List<PackageDescription>();
            BlobContainerClient blobContainer = await GetContainerClient();

            await foreach (BlobItem item in blobContainer.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None))
            {
                if (item.Metadata.ContainsKey(PACKAGE_METADATA_KEY) && item.Name.Contains('/'))
                {
                    List<string> aasIds = item.Metadata.Where(key => key.Key.StartsWith("aasId_")).Select(key => key.Value).ToList();

                    if (aasIds != null )
                    {
                        if (aasId == null || aasIds.Contains(aasId))
                        {
                            string packId = WebUtility.UrlDecode(item.Name.Split('/')[0]);
                            result.Add(new PackageDescription() { PackageId = packId, AasIds = aasIds });
                        }
                    }
                }
            }

            return result;
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

                BlobClient blobClient = blobContainer.GetBlobClient($"{containerpath}/{PACKAGE_BLOBNAME}");
                if (await blobClient.ExistsAsync())
                {
                    IDictionary<string, string> blobMetadata = (await blobClient.GetPropertiesAsync()).Value.Metadata;

                    if (file != null)
                    {
                        //BlobHttpHeaders headers = (await blobClient.GetPropertiesAsync()).Value.
                        await blobClient.UploadAsync(new BinaryData(file), true);
                        await blobClient.SetMetadataAsync(blobMetadata);
                    }

                    bool needsMetadataUpdate = false;

                    if (fileName != null)
                    {
                        blobMetadata[PACKAGE_FILENAME_KEY] = fileName;
                        needsMetadataUpdate = true;
                    }

                    result = new PackageDescription() { PackageId = packageId};
                    List<string> aasIdKeys = blobMetadata.Where(key => key.Key.StartsWith("aasId_")).Select(key => key.Key).ToList();
                    result.AasIds = aasIdKeys.Select(key => blobMetadata[key]).ToList();

                    if (aasIds != null && aasIds.Count > 0)
                    {
                        foreach(string key in aasIdKeys)
                        {
                            blobMetadata.Remove(key);
                        }
                        for (int i = 0; i < aasIds.Count; i++)
                            blobMetadata.Add($"aasId_{i + 1}", aasIds[i]);

                        needsMetadataUpdate = true;

                        if (CreateExtraAASIdentifierBlob)
                        {
                            blobClient = blobContainer.GetBlobClient($"{containerpath}/AASIDENTIFIERSBLOBNAME");
                            await UploadAASIdentifers(blobClient, aasIds, true);
                        }

                        result.AasIds = aasIds;
                    }

                    if (needsMetadataUpdate)
                        await blobClient.SetMetadataAsync(blobMetadata);
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

        private async Task UploadPackage(BlobClient blobClient, string fileName, byte[] file, List<string> aasIds, bool overwrite = false)
        {
            await blobClient.UploadAsync(new BinaryData(file), overwrite);

            IDictionary<string, string> metadata = new Dictionary<string, string>() { 
                { PACKAGE_METADATA_KEY, "true" }, { PACKAGE_FILENAME_KEY, fileName } };

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

        private string BuildPackageIdFrom(List<string> aasIds, string fileName)
        {
            // Make sure we are using a valid blob name. See https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata
            string result = fileName.Trim(); // Remove whitespaces at the end and at the beginning
            if (result.Contains('.'))
                result = Path.GetFileNameWithoutExtension(fileName);

            result = result.Replace("/", "");

            if (result.EndsWith('.'))
                result = result.Substring(0, result.Length - 1);

            return result;
        }

        private async Task<BlobContainerClient> GetContainerClient()
        {
            if (AutoCreateContainer)
            {
                await containerClient.CreateIfNotExistsAsync();
            }

            return containerClient;
        }

    }
}
