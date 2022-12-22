using Azure.DigitalTwins.Core;
using System.Collections.Generic;

namespace AAS.API.Services.ADT
{
    public class ADTOperationsResult
    {
        private Dictionary<string,BasicDigitalTwin> createdReplacedTwins;

        public Dictionary<string,BasicDigitalTwin> CreatedReplacedTwins
        {
            get { return createdReplacedTwins; }
            set { createdReplacedTwins = value; }
        }

        private Dictionary<string, BasicRelationship> createdReplacedRelationships;

        public Dictionary<string, BasicRelationship> CreatedReplacedRelationships
        {
            get { return createdReplacedRelationships; }
            set { createdReplacedRelationships = value; }
        }

        public ADTOperationsResult()
        {
            createdReplacedTwins = new Dictionary<string, BasicDigitalTwin>();
            createdReplacedRelationships = new Dictionary<string, BasicRelationship>();
        }

        public void AddCreatedReplacedTwin(BasicDigitalTwin twin)
        {
            if (twin != null)
                CreatedReplacedTwins.Add(twin.Id, twin);
        }

        public void AddCreatedReplacedRelationship(BasicRelationship rel)
        {
            if (rel != null)
                CreatedReplacedRelationships.Add(rel.Id, rel);
        }
    }
}
