using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AAS.API.Services.ADT
{
    public abstract class AbstractADTAASService
    {
        protected DigitalTwinsClient dtClient;

        public AbstractADTAASService(DigitalTwinsClient client)
        {
            dtClient = client;
        }
    }
}
