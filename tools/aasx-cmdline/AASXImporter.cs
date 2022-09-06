using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.CmdLine.Import
{
    public interface IAASXImporter
    {
        public Task<ImportResult> ImportFromPackageFile(string packageFilePath, ImportContext processInfo);

        public Task<string> ImportRelationshipElement(RelationshipElement relElement, ImportContext processInfo = null);
        public Task<string> ImportAnnotatedRelationshipElement(AnnotatedRelationshipElement relElement, ImportContext processInfo = null);
        public Task ImportConceptDescription(ConceptDescription conceptDescription, ImportContext processInfo = null);

        public Task<List<string>> CreateLinkedReferences(ISet<string> referenceTwinIds, ImportContext processInfo = null);

        public Task<List<string>> CreateLinkedReferenceElements(ISet<string> referenceTwinIds, ImportContext processInfo = null);
    }

    public class TwinRef<T>
    {
        public string DtId { get; set; }
        public T AASOject { get; set; }
    }

    public class ImportResult
    {
        private List<Tuple<string,string>> instances;

        private Dictionary<string, Asset> assets;

        public ImportResult()
        {
            instances = new List<Tuple<string, string>>();
            assets = new Dictionary<string,Asset>();
        }

        public List<Tuple<string,string>> DTInstances
        {
            get { return instances; }
            set { instances = value; }
        }

        public Dictionary<string,Asset> AASAssets
        {
            get { return assets; }
            set { assets = value; }
        }

        public ISet<string> DTInstancesOfModel(string modelId)
        {
            return new HashSet<string>(DTInstances.Where(item => item.Item2 == modelId).Select(item => item.Item1));
        }
    }

    public class ImportConfiguration
    {
        public bool DryRun { get; set; }
        public bool DeleteShellBeforeImport { get; set; }
        public bool IgnoreConceptDescriptions { get; set; }
        public bool IgnoreShells { get; set; }
        public bool AutomaticRelationshipCreationForReferences { get; set; }

        public ImportConfiguration()
        {
            IgnoreConceptDescriptions = false;
            IgnoreShells = false;
            DryRun = false;
            DeleteShellBeforeImport = false;
            AutomaticRelationshipCreationForReferences = true;
        }
    }

    public class ImportContext
    {
        public ImportConfiguration Configuration { get; set; }
        public ImportResult Result { get; set; }

        public string CurrentShellDtId { get; set; }
        public string CurrentSubmodelDtId { get; set; }

        public ImportContext()
        {
            Configuration = new ImportConfiguration();
            Result = new ImportResult();
        }
    }

    public class ImportException : Exception
    {
        public ImportException()
        {
        }

        public ImportException(string message) : base(message)
        {
        }

        public ImportException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
