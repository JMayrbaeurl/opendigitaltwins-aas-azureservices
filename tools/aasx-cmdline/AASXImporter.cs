using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.AASX.Support
{
    public interface AASXImporter
    {
        public Task<ImportResult> ImportFromPackageFile(string packageFilePath);
    }

    public class ImportResult
    {
        private List<Tuple<string,string>> instances;

        public ImportResult()
        {
            instances = new List<Tuple<string, string>>();
        }

        public List<Tuple<string,string>> DTInstances
        {
            get { return instances; }
            set { instances = value; }
        }

    }
}
