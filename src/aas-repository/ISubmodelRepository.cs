using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAS.API.Models;

namespace AAS.API.Repository
{
    public interface ISubmodelRepository
    {
        public Task<List<Submodel>> GetAllSubmodels();
        public Task<Submodel> GetSubmodelWithId(string submodelId);
    }

    
}
