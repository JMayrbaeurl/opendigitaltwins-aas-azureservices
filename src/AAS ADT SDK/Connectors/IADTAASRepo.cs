using System.Collections.Generic;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT
{
    public interface IAASRepo
    {
        public Task<string> FindTwinForReference(Reference reference);

        public Task<List<string>> FindLinkedReferences();

        public Task<List<string>> FindReferenceElements();
    }
}
