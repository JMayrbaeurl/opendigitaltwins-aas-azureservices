using AAS.API.Models;
using Azure;
using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.Repository
{
    public class ADTAASRepository : AASRepository
    {
        private DigitalTwinsClient dtClient;

        public ADTAASRepository(DigitalTwinsClient client)
        {
            dtClient = client;
        }

        public async Task<List<AssetAdministrationShell>> GetAllAdministrationShells()
        {
            List<AssetAdministrationShell> result = new List<AssetAdministrationShell>();

            string query = $"SELECT * FROM digitaltwins WHERE IS_OF_MODEL('{ADTConstants.AAS_MODEL_NAME}')";

            try
            {
                AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(query);
                await foreach (BasicDigitalTwin twin in twins)
                {
                    result.Add(this.CreateAASFromBasicDigitalTwin(twin));
                }
            } catch (RequestFailedException exc)
            {
                //log.LogError($"*** Error in retrieving parent:{exc.Status}/{exc.Message}");
                throw new AASRepositoryException($"*** Error in retrieving parent:{exc.Status}/{exc.Message}");
            }

            return result;
        }

        public AssetAdministrationShell CreateAASFromBasicDigitalTwin(BasicDigitalTwin twin)
        {
            AssetAdministrationShell aShell = new AssetAdministrationShell();
            if (twin.Contents.ContainsKey("idShort"))
            {
                aShell.IdShort = twin.Contents["idShort"].ToString();
            }
            else
            {
                aShell.IdShort = $"AAS with ADT id '{twin.Id}' has no entry for idShort";
            }

            return aShell;
        }
    }
}
