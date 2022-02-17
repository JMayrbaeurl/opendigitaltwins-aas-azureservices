using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.CmdLine
{
    public interface IAASRepo
    {
        public Task<string> FindTwinForReference(Reference reference);

        public Task<List<string>> FindLinkedReferences();

        public Task<List<string>> FindReferenceElements();
    }
}
