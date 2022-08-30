using System.Collections.Generic;

namespace AAS.AASX.CmdLine.ADT
{
    public class ADTAASOntology
    {
        public static readonly string MODEL_CONCEPTDESCRIPTION = "dtmi:digitaltwins:aas:ConceptDescription;1";
        public static readonly string MODEL_DATASPECIEC61360 = "dtmi:digitaltwins:aas:DataSpecificationIEC61360;1";
        public static readonly string MODEL_REFERENCE = "dtmi:digitaltwins:aas:Reference;1";
        public static readonly string MODEL_KEY = "dtmi:digitaltwins:aas:Key;1";
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
        public static readonly string MODEL_DATAELEMENT = "dtmi:digitaltwins:aas:DataElement;1";
        public static readonly string MODEL_OPERATION = "dtmi:digitaltwins:aas:Operation;1";
        public static readonly string MODEL_OPERATIONVARIABLE = "dtmi:digitaltwins:aas:OperationVariable;1";
        public static readonly string MODEL_BASICEVENTELEMENT = "dtmi:digitaltwins:aas:BasicEventElement;1";
        public static readonly string MODEL_ENTITY = "dtmi:digitaltwins:aas:Entity;1";
        public static readonly string MODEL_SUBMODELELEMENT = "dtmi:digitaltwins:aas:SubmodelElement;1";
        public static readonly string MODEL_VIEW = "dtmi:digitaltwins:aas:View;1";

        public static readonly Dictionary<string, Dictionary<string, string>> DTIDMap = new()
        {
            { $"{MODEL_CONCEPTDESCRIPTION}", new Dictionary<string, string>() { { "dtId", "ConceptDescription_" } } },
            { $"{MODEL_DATASPECIEC61360}", new Dictionary<string, string>() { { "dtId", "DataSpecIEC61360_" } } },
            { $"{MODEL_REFERENCE}", new Dictionary<string, string>() { { "dtId", "Reference_" } } },
            { $"{MODEL_KEY}", new Dictionary<string, string>() { { "dtId", "Key_" } } },
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
            { $"{MODEL_ANNOTATEDRELATIONSHIPELEMENT}", new Dictionary<string, string>() { { "dtId", "AnnRelShipElement_" } } },
            { $"{MODEL_OPERATION}", new Dictionary<string, string>() { { "dtId", "Operation_" } } },
            { $"{MODEL_OPERATIONVARIABLE}", new Dictionary<string, string>() { { "dtId", "OpVariable_" } } },
            { $"{MODEL_BASICEVENTELEMENT}", new Dictionary<string, string>() { { "dtId", "BasicEventElement_" } } },
            { $"{MODEL_ENTITY}", new Dictionary<string, string>() { { "dtId", "Entity_" } } },
            { $"{MODEL_SUBMODELELEMENT}", new Dictionary<string, string>() { { "dtId", "SmElement_" } } },
            { $"{MODEL_VIEW}", new Dictionary<string, string>() { { "dtId", "View_" } } }
        };

        public static Dictionary<string, string> KEYS = new() 
        {
            { "AssetAdministrationShell", MODEL_SHELL },
            { "ConceptDescription", MODEL_CONCEPTDESCRIPTION },
            { "Blob", MODEL_BLOB },
            { "Submodel", MODEL_SUBMODEL },
            { "DataElement", MODEL_DATAELEMENT },
            { "File", MODEL_FILE },
            { "Operation", MODEL_OPERATION },
            { "OperationVariable", MODEL_OPERATIONVARIABLE },
            { "BasicEvent", MODEL_BASICEVENTELEMENT },
            { "Entity", MODEL_ENTITY },
            { "Property", MODEL_PROPERTY },
            { "MultiLanguageProperty", MODEL_MULTILANGUAGEPROPERTY },
            { "Range", MODEL_RANGE },
            { "ReferenceElement", MODEL_REFERENCEELEMENT },
            { "RelationshipElement", MODEL_RELATIONSHIPELEMENT },
            { "AnnotatedRelationshipElement", MODEL_ANNOTATEDRELATIONSHIPELEMENT },
            { "Capability", MODEL_CAPABILITY },
            { "SubmodelElement", MODEL_SUBMODELELEMENT },
            { "SubmodelElementCollection", MODEL_SUBMODELELEMENTCOLLECTION },
            { "View", MODEL_VIEW }
        };
    }
}
