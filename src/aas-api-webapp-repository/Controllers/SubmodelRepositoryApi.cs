using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AAS.API.Models;
using AAS.API.Registry.Attributes;
using AAS.API.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace AAS.API.Registry.Controllers
{
    [Route("api/v1/submodels")]
    [ApiController]
    public class SubmodelRepositoryApi : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISubmodelRepository _repository;

        public SubmodelRepositoryApi(IConfiguration config, ISubmodelRepository repository)
        {
            _configuration = config ??
                             throw new ArgumentNullException(nameof(config));

            _repository = repository ??
                          throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Returns all Submodels
        /// </summary>
        /// <param name="semanticId">The value of the semantic id reference (BASE64-URL-encoded)</param>
        /// <param name="idShort">The Submodel’s idShort</param>
        /// <response code="200">Requested Submodels</response>
        [HttpGet]
        [ValidateModelState]
        [SwaggerOperation("GetAllSubmodels")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submodel>), description: "Requested Submodels")]
        public async Task<ActionResult<List<Submodel>>>GetAllSubmodels([FromQuery] string semanticId, [FromQuery] string idShort)
        {
            return Ok(await _repository.GetAllSubmodels());
        }
    }
}
