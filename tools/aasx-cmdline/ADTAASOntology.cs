using System.Collections.Generic;

namespace AAS.AASX.CmdLine.ADT
{
    public class ADTAASOntology
    {
        public static readonly string MODEL_CONCEPTDESCRIPTION = "dtmi:digitaltwins:aas:ConceptDescription;1";
        public static readonly string MODEL_DATASPECIEC61360 = "dtmi:digitaltwins:aas:DataSpecificationIEC61360;1";
        public static readonly string MODEL_REFERENCE = "dtmi:digitaltwins:aas:Reference;1";
        public static readonly string MODEL_KEY = "dtmi:digitaltwins:aas:Key;1";
        public static readonly string MODEL_ASSET = "dtmi:digitaltwins:aas:Asset;1";
        public static readonly string MODEL_SHELL = "dtmi:digitaltwins:aas:AssetAdministrationShell;1";
        public static readonly string MODEL_ASSETINFORMATION = "dtmi:digitaltwins:aas:AssetInformation;1";
        public static readonly string MODEL_SUBMODEL = "dtmi:digitaltwins:aas:Submodel;1";
        public static readonly string MODEL_PROPERTY = "dtmi:digitaltwins:aas:Property;1";
        public static readonly string MODEL_SUBMODELELEMENTCOLLECTION = "dtmi:digitaltwins:aas:SubmodelElementCollection;1";
        public static readonly string MODEL_FILE = "dtmi:digitaltwins:aas:File;1";
        public static readonly string MODEL_BLOB = "dtmi:digitaltwins:aas:Blob;1";
        public static readonly string MODEL_RANGE = "dtmi:digitaltwins:aas:Range;1";
        public static readonly string MODEL_MULTILANGUAGEPROPERTY = "dtmi:digitaltwins:aas:MultiLanguageProperty;1";
        public static readonly string MODEL_REFERENCEELEMENT = "dtmi:digitaltwins:aas:ReferenceElement;1";
        public static readonly string MODEL_QUALIFIER = "dtmi:digitaltwins:aas:Qualifier;1";
        public static readonly string MODEL_CAPABILITY = "dtmi:digitaltwins:aas:Capability;1";
        public static readonly string MODEL_RELATIONSHIPELEMENT = "dtmi:digitaltwins:aas:RelationshipElement;1";
        public static readonly string MODEL_ANNOTATEDRELATIONSHIPELEMENT = "dtmi:digitaltwins:aas:AnnotatedRelationshipElement;1";
        public static readonly string MODEL_REFERABLE = "dtmi:digitaltwins:aas:Referable;1";
        public static readonly string MODEL_IDENTIFIABLE = "dtmi:digitaltwins:aas:Identifiable;1";

        public static readonly Dictionary<string, Dictionary<string, string>> DTIDMap = new Dictionary<string, Dictionary<string, string>>()
        {
            { $"{MODEL_CONCEPTDESCRIPTION}", new Dictionary<string, string>() { { "dtId", "ConceptDescription_" } } },
            { $"{MODEL_DATASPECIEC61360}", new Dictionary<string, string>() { { "dtId", "DataSpecIEC61360_" } } },
            { $"{MODEL_REFERENCE}", new Dictionary<string, string>() { { "dtId", "Reference_" } } },
            { $"{MODEL_KEY}", new Dictionary<string, string>() { { "dtId", "Key_" } } },
            { $"{MODEL_ASSET}", new Dictionary<string, string>() { { "dtId", "Asset_" } } },
            { $"{MODEL_SHELL}", new Dictionary<string, string>() { { "dtId", "Shell_" } } },
            { $"{MODEL_ASSETINFORMATION}", new Dictionary<string, string>() { { "dtId", "AssetInfo_" } } },
            { $"{MODEL_SUBMODEL}", new Dictionary<string, string>() { { "dtId", "Submodel_" } } },
            { $"{MODEL_PROPERTY}", new Dictionary<string, string>() { { "dtId", "Property_" } } },
            { $"{MODEL_SUBMODELELEMENTCOLLECTION}", new Dictionary<string, string>() { { "dtId", "SubmodelElements_" } } },
            { $"{MODEL_FILE}", new Dictionary<string, string>() { { "dtId", "File_" } } },
            { $"{MODEL_BLOB}", new Dictionary<string, string>() { { "dtId", "Blob_" } } },
            { $"{MODEL_RANGE}", new Dictionary<string, string>() { { "dtId", "Range_" } } },
            { $"{MODEL_MULTILANGUAGEPROPERTY}", new Dictionary<string, string>() { { "dtId", "MultiLangProp_" } } },
            { $"{MODEL_REFERENCEELEMENT}", new Dictionary<string, string>() { { "dtId", "RefElement_" } } },
            { $"{MODEL_QUALIFIER}", new Dictionary<string, string>() { { "dtId", "Qualifier_" } } },
            { $"{MODEL_CAPABILITY}", new Dictionary<string, string>() { { "dtId", "Capability_" } } },
            { $"{MODEL_RELATIONSHIPELEMENT}", new Dictionary<string, string>() { { "dtId", "RelShipElement_" } } },
            { $"{MODEL_ANNOTATEDRELATIONSHIPELEMENT}", new Dictionary<string, string>() { { "dtId", "AnnRelShipElement_" } } }
        };

        public static Dictionary<string, string> KEYS = new Dictionary<string, string>() 
        {
            { "Submodel", MODEL_SUBMODEL }
        };
    }
}
