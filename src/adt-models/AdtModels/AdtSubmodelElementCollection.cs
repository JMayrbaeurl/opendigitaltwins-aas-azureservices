using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtModels.AdtModels
{
    public class AdtSubmodelElementCollection : AdtSubmodelElement
    {
        public List<AdtSubmodelElement> submodelElements = new List<AdtSubmodelElement>();
    }
}
