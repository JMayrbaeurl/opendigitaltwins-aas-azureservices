using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.AASX.Support
{
    public interface IAASXInspector
    {
        public string ListAllAASXPackageEntries(string packageFilePath);
    }
}
