using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.Models.Interfaces
{
    public interface AASXFileServer
    {
        /// <summary>
        /// Returns a specific AASX package from the server
        /// </summary>
        /// <param name="packageId">The package Id (BASE64-URL-encoded)</param>
        public Task<PackageFile> GetAASXByPackageId(string packageId);

        /// <summary>
        /// Returns a list of available AASX packages at the server
        /// </summary>
        /// <param name="aasId">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        public Task<List<PackageDescription>> GetAllAASXPackageIds(string aasId);

        /// <summary>
        /// Deletes a specific AASX package from the server
        /// </summary>
        /// <param name="packageId">The Package Id (BASE64-URL-encoded)</param>
        public Task DeleteAASXByPackageId(string packageId);

        /// <summary>
        /// Stores the AASX package at the server
        /// </summary>
        public Task<PackageDescription> StoreAASXPackage(List<string> aasIds, byte[] file, string fileName);

        /// <summary>
        /// Updates the AASX package at the server
        /// </summary>
        public Task<PackageDescription> UpdateAASXPackage(string packageId, List<string> aasIds, byte[] file, string fileName);
    }

    public class PackageFile
    {
        private string filename;

        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }
        private byte[] contents;

        public byte[] Contents
        {
            get { return contents; }
            set { contents = value; }
        }
    }
}
