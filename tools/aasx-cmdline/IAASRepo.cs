using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.CmdLine
{
    public interface IAASRepo
    {
        public Task<string> KeyExists(Key key);
        public Task<string> FindTwinForReference(Reference reference);
    }
}
