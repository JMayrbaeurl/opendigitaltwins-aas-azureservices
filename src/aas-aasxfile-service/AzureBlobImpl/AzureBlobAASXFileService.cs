using AAS.API.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.AASXFile
{
    public class AzureBlobAASXFileService : AASAASXFile
    {
        private BlobServiceClient blobServiceClient; // See https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet#upload-a-blob-to-a-container

        private readonly ILogger _logger;

        public AzureBlobAASXFileService(BlobServiceClient serviceClient, IConfiguration config, ILogger<AzureBlobAASXFileService> logger)
        {
            blobServiceClient = serviceClient;
            _logger = logger;
        }

        public async Task<PackageDescription> CreateAASXPackage(List<string> aasIds, byte[] file, string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAASXByPackageId(string packageId)
        {
            throw new NotImplementedException();
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
    }
}
