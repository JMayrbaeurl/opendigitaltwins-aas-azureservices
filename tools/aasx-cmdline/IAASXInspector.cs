using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.AASX.CmdLine.Inspect
{
    public interface IAASXInspector
    {
        public string ListAllAASXPackageEntries(string packageFilePath);
    }
}
