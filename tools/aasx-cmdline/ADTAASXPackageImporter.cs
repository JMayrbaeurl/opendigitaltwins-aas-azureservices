using AAS.AASX.Support;
using AdminShellNS;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.ADT
{
    public class ADTAASXPackageImporter : AASXImporter
    {
        private DigitalTwinsClient dtClient;

        private readonly ILogger _logger;

        public ADTAASXPackageImporter(DigitalTwinsClient adtClient, ILogger<ADTAASXPackageImporter> logger)
        {
            this.dtClient = adtClient;
            this._logger = logger;
        }

        public async Task<ImportResult> ImportFromPackageFile(string packageFilePath)
        {
            ImportResult result = new ImportResult();

            using var package = new AdminShellPackageEnv(packageFilePath);

            if (package.AasEnv != null)
            {
                if (package.AasEnv.ConceptDescriptions != null && package.AasEnv.ConceptDescriptions.Count > 0)
                {
                    foreach (var desc in package.AasEnv.ConceptDescriptions)
                    {
                        try
                        {
                            await ImportConceptDescription(desc, package, result);
                        } catch (RequestFailedException ex)
                        {
                            _logger.LogError($"Exception create twin for Concept description '{desc.idShort}': {ex.Message}", ex);
                        }
                    }
                }
            }

            return result;
        }

        public async Task ImportConceptDescription(ConceptDescription conceptDescription, AdminShellPackageEnv package, ImportResult result)
        {
            _logger.LogInformation($"Now importing Concept description '{conceptDescription.idShort}' into ADT instance");

            if (await ConceptDescriptionExists(conceptDescription.idShort))
            {
                _logger.LogInformation($"Skipping creation of twin for Concept description '{conceptDescription.idShort}' because it already exists");
                return;
            }

            // Start by creating a twin for the Concept description
            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = "dtmi:digitaltwins:aas:ConceptDescription;1";
            twinData.Id = $"ConceptDescription_{conceptDescription.idShort}";

            AddIdentifiableAttributes(twinData, conceptDescription,
                (conceptDescription.IEC61360Content != null && conceptDescription.IEC61360Content.shortName != null) ?
                conceptDescription.IEC61360Content.shortName.GetDefaultStr() : null);

            // TODO: IsCaseOf references
            if (conceptDescription.IsCaseOf != null)
            {
                await AddReferences(twinData, conceptDescription.IsCaseOf, "isCaseOf", result);
            }

            await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
            result.DTInstances.Add(new Tuple<string, string>(twinData.Id, twinData.Metadata.ModelId));

            // Create twins and relationships for embedded data specifications
            if (conceptDescription.embeddedDataSpecification != null)
            {
                AddHasDataSpecification(twinData, conceptDescription.embeddedDataSpecification, result);
            }
        }

        private async void AddHasDataSpecification(BasicDigitalTwin twinData, HasDataSpecification embeddedDataSpecification, ImportResult result)
        {
            if (embeddedDataSpecification.IEC61360Content != null)
            {
                DataSpecificationIEC61360 content = embeddedDataSpecification.IEC61360Content;

                // Create the DataSpecificationIEC61360 twin
                var dsTwinData = new BasicDigitalTwin();
                dsTwinData.Metadata.ModelId = "dtmi:digitaltwins:aas:DataSpecificationIEC61360;1";
                dsTwinData.Id = $"DataSpecIEC61360_{StripInvalidTwinIdCharacters(content.shortName.GetDefaultStr())}";

                if (content.preferredName != null)
                {
                    dsTwinData.Contents.Add("preferredName", LangStringSetIEC61360ToString(
                        content.preferredName));
                }
                if (content.shortName != null)
                {
                    dsTwinData.Contents.Add("shortName", LangStringSetIEC61360ToString(
                        content.shortName));
                }
                if (!string.IsNullOrEmpty(content.unit))
                    dsTwinData.Contents.Add("unit", content.unit);
                // TODO: unitId
                if (!string.IsNullOrEmpty(content.sourceOfDefinition))
                    dsTwinData.Contents.Add("sourceOfDefinition", content.sourceOfDefinition);
                if (!string.IsNullOrEmpty(content.symbol))
                    dsTwinData.Contents.Add("symbol", content.symbol);
                if (!string.IsNullOrEmpty(content.dataType))
                    dsTwinData.Contents.Add("dataType", content.dataType);
                if (content.definition != null)
                    dsTwinData.Contents.Add("definition", LangStringSetIEC61360ToString(content.definition));
                if (!string.IsNullOrEmpty(content.valueFormat))
                    dsTwinData.Contents.Add("valueFormat", content.valueFormat);
                // TODO: valueList
                // TODO: value
                // TODO: levelType

                await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(dsTwinData.Id, dsTwinData);
                result.DTInstances.Add(new Tuple<string, string>(dsTwinData.Id, dsTwinData.Metadata.ModelId));

                // Create relationship to 
                var relationship = new BasicRelationship
                {
                    TargetId = dsTwinData.Id,
                    Name = "dataSpecification"
                };

                string relId = $"{twinData.Id}-dataSpecification->{dsTwinData.Id}";
                await dtClient.CreateOrReplaceRelationshipAsync(twinData.Id, relId, relationship);
            }
        }

        private async Task AddReferences(BasicDigitalTwin twinData, List<Reference> references, string relationshipName, ImportResult result)
        {
            foreach(var reference in references)
            {
                // Create Reference twin
                var refTwinData = new BasicDigitalTwin();
                refTwinData.Metadata.ModelId = "dtmi:digitaltwins:aas:Reference;1";
                refTwinData.Id = $"{twinData.Id}_Ref_{Guid.NewGuid().ToString()}";
                await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(refTwinData.Id, refTwinData);
                result.DTInstances.Add(new Tuple<string, string>(refTwinData.Id, refTwinData.Metadata.ModelId));

                // Create relationship between source twin and Reference twin
                var relationship = new BasicRelationship
                {
                    TargetId = refTwinData.Id,
                    Name = relationshipName
                };

                string relId = $"{twinData.Id}-{relationshipName}->{refTwinData.Id}";
                await dtClient.CreateOrReplaceRelationshipAsync(twinData.Id, relId, relationship);

                // Create key
                var keyTwinData = new BasicDigitalTwin();
                keyTwinData.Metadata.ModelId = "dtmi:digitaltwins:aas:Key;1";
                keyTwinData.Id = $"{twinData.Id}_Key_{Guid.NewGuid().ToString()}";
                keyTwinData.Contents.Add("key", reference.First.type);
                keyTwinData.Contents.Add("value", reference.First.value);
                keyTwinData.Contents.Add("idType", URITOIRI(reference.First.idType));
                await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(keyTwinData.Id, keyTwinData);
                result.DTInstances.Add(new Tuple<string, string>(keyTwinData.Id, keyTwinData.Metadata.ModelId));

                var keyRel = new BasicRelationship
                {
                    TargetId = keyTwinData.Id,
                    Name = "key"
                };
                relId = $"{refTwinData.Id}-key->{keyTwinData.Id}";
                await dtClient.CreateOrReplaceRelationshipAsync(refTwinData.Id, relId, keyRel);
            }
        }

        private void AddReferableAttributes(BasicDigitalTwin twinData, Referable referable, string displayname = null)
        {
            if (!string.IsNullOrEmpty(referable.idShort))
                twinData.Contents.Add("idShort", referable.idShort);
            if (!string.IsNullOrEmpty(displayname))
                twinData.Contents.Add("displayName", displayname);
            if (!string.IsNullOrEmpty(referable.category))
                twinData.Contents.Add("category", referable.category);
            if (referable.description != null)
                twinData.Contents.Add("description", DescToString(referable.description));
        }

        private void AddIdentifiableAttributes(BasicDigitalTwin twinData, Identifiable identifiable, string displayname = null)
        {
            // Referable attributes
            AddReferableAttributes(twinData, identifiable, displayname);

            // Identifiable attributes
            BasicDigitalTwinComponent identifier = new BasicDigitalTwinComponent();
            if (identifiable.identification != null)
            {
                identifier.Contents.Add("id", identifiable.identification.id);
                identifier.Contents.Add("idType", identifiable.identification.idType);
            }
            twinData.Contents.Add("identification", identifier);

            BasicDigitalTwinComponent admin = new BasicDigitalTwinComponent();
            if (identifiable.administration != null &&
                (identifiable.administration.version != null || identifiable.administration.revision != null))
            {
                if (!string.IsNullOrEmpty(identifiable.administration.version))
                    admin.Contents.Add("version", identifiable.administration.version);
                if (!string.IsNullOrEmpty(identifiable.administration.revision))
                    admin.Contents.Add("revision", identifiable.administration.revision);
            }
            twinData.Contents.Add("administration", admin);
        }

        public static string DescToString(Description desc)
        {
            if (desc == null)
                return default(string);
            else
            {
                string result = "";
                foreach(var entry in desc.langString)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string LangStringSetIEC61360ToString(LangStringSetIEC61360 langStrs)
        {
            if (langStrs == null)
                return default(string);
            else
            {
                string result = "";
                foreach (var entry in langStrs)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string StripInvalidTwinIdCharacters(string dtIdProposal)
        {
            string result = dtIdProposal.Trim();

            result = result.Replace(" ", "");
            result = result.Replace("/", "");

            return result;
        }

        public static string URITOIRI(string idType)
        {
            if ("URI".Equals(idType))
                return "IRI";
            else
                return idType;
        }

        public async Task<bool> ConceptDescriptionExists(string shortName)
        {
            bool result = false;

            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL(dt, 'dtmi:digitaltwins:aas:ConceptDescription;1') AND dt.$dtId = 'ConceptDescription_{shortName}'";
            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            result = await queryResult.GetAsyncEnumerator().MoveNextAsync();

            return result;
        }
    }
}
