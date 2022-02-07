using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AAS.AASX.CmdLine.Test
{
    public abstract class AbstractTestSupport
    {
        protected IConfiguration configuration;

        protected AbstractTestSupport()
        {
            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.tests.json").Build();
        }

        protected static void ConfigureBasicServices(IServiceCollection services, string adtInstanceUrl)
        {
            services.Configure<DigitalTwinsClientOptions>(options => options.ADTEndpoint = new Uri(adtInstanceUrl));

            services.AddAzureClients(builder =>
            {
                builder.AddClient<DigitalTwinsClient, DigitalTwinsClientOptions>((options, provider) =>
                {
                    var appOptions = provider.GetService<IOptions<DigitalTwinsClientOptions>>();

                    var credentials = new DefaultAzureCredential();
                    DigitalTwinsClient client = new DigitalTwinsClient(appOptions.Value.ADTEndpoint,
                                credentials, new Azure.DigitalTwins.Core.DigitalTwinsClientOptions { Transport = new HttpClientTransport(new HttpClient()) });
                    return client;
                });

                // First use DefaultAzureCredentials and second EnvironmentCredential to enable local docker execution
                builder.UseCredential(new ChainedTokenCredential(new DefaultAzureCredential(), new EnvironmentCredential()));
            });
        }
    }

    public class DigitalTwinsClientOptions
    {
        public Uri ADTEndpoint { get; set; }
    }
}
