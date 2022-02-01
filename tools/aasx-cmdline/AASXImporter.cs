using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.Support
{
    public interface AASXImporter
    {
        public Task<ImportResult> ImportFromPackageFile(string packageFilePath, bool ignConceptDescs);
    }

    public class TwinRef<T>
    {
        public string DtId { get; set; }
        public T AASOject { get; set; }
    }

    public class ImportResult
    {
        private List<Tuple<string,string>> instances;

        private Dictionary<string, TwinRef<Asset>> assets;

        public ImportResult()
        {
            instances = new List<Tuple<string, string>>();
            assets = new Dictionary<string,TwinRef<Asset>>();
        }

        public List<Tuple<string,string>> DTInstances
        {
            get { return instances; }
            set { instances = value; }
        }

        public Dictionary<string,TwinRef<Asset>> AASAssets
        {
            get { return assets; }
            set { assets = value; }
        }
    }
}
