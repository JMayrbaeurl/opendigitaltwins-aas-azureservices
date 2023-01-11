using AasCore.Aas3_0_RC02;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Aas.Api.Repository.Mapper
{
    public static class AasAttributeExtensionsWithJObject
    {
        
        public static void AddSubmodelElementValues(this ISubmodelElement submodelElement, JObject obj)
        {
            submodelElement.AddReferableValues(obj);
            submodelElement.AddDataSpecificationValues(obj);
            submodelElement.AddSemanticValues(obj);
            submodelElement.AddKind(obj);
            submodelElement.AddQualifiers(obj);
        }

        public static void AddReferableValues(this IReferable referable, JObject obj)
        {
            referable.Category = obj.GetValue("category").ToString();
            referable.IdShort = obj.GetValue("idShort").ToString();
            referable.DisplayName = obj.GetValue("displayName").ToObject<List<LangString>>();
            referable.Description = obj.GetValue("description").ToObject<List<LangString>>();
            referable.Checksum = obj.GetValue("checksum").ToString();
            referable.AddHasExtensions(obj);
        }

        public static void AddHasExtensions(this IHasExtensions hasExtensions, JObject obj)
        {
            hasExtensions.Extensions = obj.GetValue("extensions").ToObject<List<Extension>>();
        }

        public static void AddDataSpecificationValues(this IHasDataSpecification hasDataSpecification, JObject obj)
        {
            if (obj.GetValue("embeddedDataSpecifications") != null)
            {
                hasDataSpecification.EmbeddedDataSpecifications = new List<EmbeddedDataSpecification>();
                var tmpEmbeddedDataSpecifications = obj.GetValue("embeddedDataSpecifications").ToObject<List<dynamic>>();
                foreach (var tmpEmbeddedDataSpecification in tmpEmbeddedDataSpecifications)
                {
                    if (tmpEmbeddedDataSpecification.dataSpecificationContent.modelType ==
                        "DataSpecificationPhysicalUnit")
                        hasDataSpecification.EmbeddedDataSpecifications.Add(new EmbeddedDataSpecification(
                            tmpEmbeddedDataSpecification.dataSpecification.ToObject<Reference>(),
                            tmpEmbeddedDataSpecification.dataSpecificationContent
                                .ToObject<DataSpecificationPhysicalUnit>()));
                    if (tmpEmbeddedDataSpecification.dataSpecificationContent.modelType == "DataSpecificationIec61360")
                        hasDataSpecification.EmbeddedDataSpecifications.Add(new EmbeddedDataSpecification(
                            tmpEmbeddedDataSpecification.dataSpecification.ToObject<Reference>(),
                            tmpEmbeddedDataSpecification.dataSpecificationContent
                                .ToObject<DataSpecificationIec61360>()));
                }
            }
        }

        public static void AddSemanticValues(this IHasSemantics hasSemantics, JObject obj)
        {
            hasSemantics.SemanticId = obj.GetValue("semanticId").ToObject<Reference>();
            hasSemantics.SupplementalSemanticIds =
                obj.GetValue("supplementalSemanticIds").ToObject<List<Reference>>();
        }

        public static void AddKind(this IHasKind hasKind, JObject obj)
        {
            hasKind.Kind = obj.GetValue("kind").ToObject<ModelingKind>();
        }

        public static void AddQualifiers(this IQualifiable qualifiable, JObject obj)
        {
            qualifiable.Qualifiers = obj.GetValue("qualifiers").ToObject<List<Qualifier>>();
        }

        
    }
}
