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
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using AAS.API.Attributes;

using AAS.API.Models;
using AAS.API.Discovery;
using System.Web;
using Microsoft.Extensions.Logging;

namespace AAS.API.WebApp.Controllers
{ 
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class AssetAdministrationShellBasicDiscoveryApiController : ControllerBase
    {
        private readonly ILogger _logger;

        private AASDiscovery discoveryService;


        /// <summary>
        /// 
        /// </summary>
        public AssetAdministrationShellBasicDiscoveryApiController(ILogger<AssetAdministrationShellBasicDiscoveryApiController> logger, AASDiscovery service) : base()
        {
            _logger = logger;

            discoveryService = service;
        }

        /// <summary>
        /// Deletes all Asset identifier key-value-pair linked to an Asset Administration Shell to edit discoverable content
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <response code="204">Asset identifier key-value-pairs deleted successfully</response>
        [HttpDelete]
        [Route("/lookup/shells/{aasIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("DeleteAllAssetLinksById")]
        public virtual IActionResult DeleteAllAssetLinksById([FromRoute][Required]string aasIdentifier)
        {
            _logger.LogInformation($"DeleteAllAssetLinksById called for for Asset identifier '{aasIdentifier}'");

            if (discoveryService == null)
            {
                _logger.LogError("Invalid setup. No Discovery service configured. Check DI setup");
                throw new AASDiscoveryException("Invalid setup. No Discovery service configured. Check DI setup");
            }

            return new ObjectResult(discoveryService.DeleteAllAssetLinksById(HttpUtility.UrlDecode(aasIdentifier)).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Returns a list of Asset Administration Shell ids based on Asset identifier key-value-pairs
        /// </summary>
        /// <param name="assetIds">The key-value-pair of an Asset identifier</param>
        /// <response code="200">Requested Asset Administration Shell ids</response>
        [HttpGet]
        [Route("/lookup/shells")]
        [ValidateModelState]
        [SwaggerOperation("GetAllAssetAdministrationShellIdsByAssetLink")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<string>), description: "Requested Asset Administration Shell ids")]
        public virtual IActionResult GetAllAssetAdministrationShellIdsByAssetLink([FromQuery]List<IdentifierKeyValuePair> assetIds)
        {
            _logger.LogInformation($"GetAllAssetAdministrationShellIdsByAssetLink called for Asset links '{assetIds}'");

            if (discoveryService == null)
            {
                _logger.LogError("Invalid setup. No Discovery service configured. Check DI setup");
                throw new AASDiscoveryException("Invalid setup. No Discovery service configured. Check DI setup");
            }

            return new ObjectResult(discoveryService.GetAllAssetAdministrationShellIdsByAssetLink(assetIds).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Returns a list of Asset identifier key-value-pairs based on an Asset Administration Shell id to edit discoverable content
        /// </summary>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <response code="200">Requested Asset identifier key-value-pairs</response>
        [HttpGet]
        [Route("/lookup/shells/{aasIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("GetAllAssetLinksById")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<IdentifierKeyValuePair>), description: "Requested Asset identifier key-value-pairs")]
        public virtual IActionResult GetAllAssetLinksById([FromRoute][Required] string aasIdentifier)
        {
            _logger.LogInformation($"GetAllAssetLinksById called for Asset identifier '{aasIdentifier}'");

            if (discoveryService == null)
            {
                _logger.LogError("Invalid setup. No Discovery service configured. Check DI setup");
                throw new AASDiscoveryException("Invalid setup. No Discovery service configured. Check DI setup");
            }

            return new ObjectResult(discoveryService.GetAllAssetLinksById(HttpUtility.UrlDecode(aasIdentifier)).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Creates all Asset identifier key-value-pair linked to an Asset Administration Shell to edit discoverable content
        /// </summary>
        /// <param name="body">Asset identifier key-value-pairs</param>
        /// <param name="aasIdentifier">The Asset Administration Shell’s unique id (BASE64-URL-encoded)</param>
        /// <response code="201">Asset identifier key-value-pairs created successfully</response>
        [HttpPost]
        [Route("/lookup/shells/{aasIdentifier}")]
        [ValidateModelState]
        [SwaggerOperation("PostAllAssetLinksById")]
        [SwaggerResponse(statusCode: 201, type: typeof(List<IdentifierKeyValuePair>), description: "Asset identifier key-value-pairs created successfully")]
        public virtual IActionResult PostAllAssetLinksById([FromBody]List<IdentifierKeyValuePair> body, [FromRoute][Required]string aasIdentifier)
        {
            _logger.LogInformation($"PostAllAssetLinksById called for Asset identifier '{aasIdentifier}' and asset links '{body}'");

            if (discoveryService == null)
            {
                _logger.LogError("Invalid setup. No Discovery service configured. Check DI setup");
                throw new AASDiscoveryException("Invalid setup. No Discovery service configured. Check DI setup");
            }

            return new ObjectResult(discoveryService.CreateAllAssetLinksById(HttpUtility.UrlDecode(aasIdentifier), body).GetAwaiter().GetResult());
        }
    }
}
