using AdminShellNS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AAS.AASX.CmdLine.Inspect
{
    public class StdAASXInspector : IAASXInspector
    {
        public string ListAllAASXPackageEntries(string packageFilePath)
        {
            if (packageFilePath == null)
                throw new ArgumentNullException(nameof(packageFilePath));

            if (!System.IO.File.Exists(packageFilePath))
            {
                throw new ArgumentException($"AASX package file at '{packageFilePath}' doesn't exist");
            }

            using var package = new AdminShellPackageEnv(packageFilePath);
            InspectionResult result = new InspectionResult();

            if (package.AasEnv != null)
            {
                foreach (var aas in package.AasEnv.AdministrationShells)
                {
                    result.Shells.Add(new EntryDesc(aas.identification.idType, aas.identification.id, aas.idShort));
                }

                foreach (var submodel in package.AasEnv.Submodels)
                {
                    result.Submodels.Add(new EntryDesc(submodel.identification.idType, submodel.identification.id, submodel.idShort));
                }

                foreach (var asset in package.AasEnv.Assets)
                {
                    result.Assets.Add(new EntryDesc(asset.identification.idType, asset.identification.id, asset.idShort));
                }
            }

            return result.ToString();
        }
    }

    public class EntryDesc
    {
        public EntryDesc(string idType, string id, string idShort)
        {
            IdType = idType;
            Id = id;
            IdShort = idShort;
        }

        public string IdType { get; set; }
        public string Id { get; set; }
        public string IdShort { get; set; }
    }

    public class InspectionResult
    {
        public List<EntryDesc> Shells { get; set; }
        public List<EntryDesc> Submodels { get; set; }
        public List<EntryDesc> Assets { get; set; }

        public InspectionResult()
        {
            Shells = new List<EntryDesc>();
            Submodels = new List<EntryDesc>();
            Assets = new List<EntryDesc>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
