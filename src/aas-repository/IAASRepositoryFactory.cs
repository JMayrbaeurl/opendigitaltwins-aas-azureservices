using AAS_Services_Support.ADT_Support;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.Repository
{
    public interface IAASRepositoryFactory
    {
        public AASRepository CreateAASRepositoryForADT(string adtInstanceURL);
    }
}
