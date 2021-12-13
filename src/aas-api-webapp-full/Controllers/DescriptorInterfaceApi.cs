/*
 * DotAAS Part 2 | HTTP/REST | Entire Interface Collection
 *
 * The entire interface collection as part of Details of the Asset Administration Shell Part 2
 *
 * OpenAPI spec version: Final-Draft
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using AAS.API.Attributes;

using Microsoft.AspNetCore.Authorization;
using AAS.API.Models;

namespace AAS.API.WebApp.Controllers
{ 
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class DescriptorInterfaceApiController : ControllerBase
    { 
        /// <summary>
        /// Returns the self-describing information of a network resource (Descriptor)
        /// </summary>
        /// <response code="200">Requested Descriptor</response>
        [HttpGet]
        [Route("/descriptor")]
        [ValidateModelState]
        [SwaggerOperation("GetDescriptor")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Descriptor>), description: "Requested Descriptor")]
        public virtual IActionResult GetDescriptor()
        { 
            //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
            // return StatusCode(200, default(List<Descriptor>));
            string exampleJson = null;
            /*
            exampleJson = "[ \"{  \\"endpoints\\": [{ \\"protocolInformation\\": { \\"endpointAddress\\": \\"https://localhost:1234\\", \\"endpointProtocolVersion: \\"1.1\\"  }, \\"interface\\": \\"AAS-1.0\\"  }, { \\"protocolInformation\\": {  \\"endpointAddress\\": \\"opc.tcp://localhost:4840\\" },    \\"interface\\": \\"AAS-1.0\\" }, {  \\"protocolInformation\\": { \\"endpointAddress\\": \\"https://localhost:5678\\",  \\"endpointProtocolVersion: \\"1.1\\", \\"subprotocol\\": \\"OPC UA Basic SOAP\\", \\"subprotocolBody\\": \\"ns=2;s=MyAAS\\", \\"subprotocolBodyEncoding\\": \\"plain\\"  }, \\"interface\\": \\"AAS-1.0\\"  }] }\", \"{  \\"endpoints\\": [{ \\"protocolInformation\\": { \\"endpointAddress\\": \\"https://localhost:1234\\", \\"endpointProtocolVersion: \\"1.1\\"  }, \\"interface\\": \\"AAS-1.0\\"  }, { \\"protocolInformation\\": {  \\"endpointAddress\\": \\"opc.tcp://localhost:4840\\" },    \\"interface\\": \\"AAS-1.0\\" }, {  \\"protocolInformation\\": { \\"endpointAddress\\": \\"https://localhost:5678\\",  \\"endpointProtocolVersion: \\"1.1\\", \\"subprotocol\\": \\"OPC UA Basic SOAP\\", \\"subprotocolBody\\": \\"ns=2;s=MyAAS\\", \\"subprotocolBodyEncoding\\": \\"plain\\"  }, \\"interface\\": \\"AAS-1.0\\"  }] }\" ]";
            */
                        var example = exampleJson != null
                        ? JsonConvert.DeserializeObject<List<Descriptor>>(exampleJson)
                        : default(List<Descriptor>);            //TODO: Change the data returned
            return new ObjectResult(example);
        }
    }
}
