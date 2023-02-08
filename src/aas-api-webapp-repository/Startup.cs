/*
 * DotAAS Part 2 | HTTP/REST | Asset Administration Shell Repository
 *
 * An exemplary interface combination for the use case of an Asset Administration Shell Repository
 *
 * OpenAPI spec version: Final-Draft
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.IO;
using AAS.ADT;
using AAS.ADT.Models;
using AAS.API.Repository;
using AAS.API.Repository.Adt;
using Aas.Api.Repository.Filters;
using Aas.Api.Repository.Models;
using AAS.API.Services.ADT;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Aas.Api.Repository
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnv;

        private IConfiguration Configuration { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _hostingEnv = env;
            Configuration = configuration;

        }

        
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
                cfg.DisableConstructorMapping();
                cfg.AllowNullCollections = true;
            });
            IMapper mapper = configuration.CreateMapper();
            services.AddLogging();
            services.AddSingleton(mapper);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));

            // Add framework services.
            services
                .AddMvc(options =>
                {
                    options.Filters.Add(new ProducesAttribute("application/json"));
                    options.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
                    options.ReturnHttpNotAcceptable = true;
                    options.ModelBinderProviders.Insert(0, new IdentifierKeyValuePairModelBinderProvider());
                })
                .AddNewtonsoftJson(opts =>
                {
                    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    opts.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
                    opts.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .AddXmlSerializerFormatters();

            services.AddHttpClient<IAzureDigitalTwinsHttpClient, AzureDigitalTwinsHttpClient>(client =>
                client.BaseAddress = new Uri(Configuration["ADT_SERVICE_URL"]));
            
            services.AddSingleton<DigitalTwinsClientFactory, StdDigitalTwinsClientFactory>();
            services.AddScoped<ISubmodelRepository, AdtSubmodelRepository>();
            services.AddTransient<IAdtAasConnector, AdtAasConnector>();
            services.AddTransient<IAdtSubmodelConnector, AdtSubmodelConnector>();
            services.AddTransient<IAdtDefinitionsAndSemanticsModelFactory, AdtDefinitionsAndSemanticsModelFactory>();
            services.AddTransient<IAdtSubmodelModelFactory, AdtSubmodelModelFactory>();
            services.AddTransient<IAdtSubmodelElementFactory,AdtSubmodelElementFactory>();
            services.AddTransient<IAASRepositoryFactory, AASRepositoryFactory>();

            services.AddTransient<IAasWriteSubmodelElements, AasWriteSubmodelElements>();
            services.AddTransient<IAasWriteSubmodel, AasWriteSubmodel>();
            services.AddTransient<IAasWriteAssetAdministrationShell, AasWriteAssetAdministrationShell>();
            services.AddTransient<IAasWriteConnector, AasWriteConnectorForAdtCommunication>();
            services.AddTransient<IAasWriteBase, AasWriteBase>();
            services.AddTransient<IAdtTwinFactory, AdtTwinFactory>();
            services.AddTransient<IAasDeleteAdt, AasDeleteAdt>();





            services
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("Final-Draft", new OpenApiInfo
                    {
                        Version = "Final-Draft",
                        Title = "DotAAS Part 2 | HTTP/REST | Asset Administration Shell Repository",
                        Description = "DotAAS Part 2 | HTTP/REST | Asset Administration Shell Repository (ASP.NET Core 3.1)",
                        Contact = new OpenApiContact()
                        {
                           Name = "Michael Hoffmeister, Torben Miny, Andreas Orzelski, Manuel Sauer, Constantin Ziesche",
                           Url = new Uri("https://github.com/swagger-api/swagger-codegen"),
                           Email = ""
                        },
                        TermsOfService = new Uri("https://github.com/admin-shell-io/aas-specs")
                    });
                    c.CustomSchemaIds(type => type.FullName);
                    c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{_hostingEnv.ApplicationName}.xml");

                    // Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
                    // Use [ValidateModelState] on Actions to actually validate it in C# as well!
                    c.OperationFilter<GeneratePathParamsValidationFilter>();

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMapper mapper)
        {
            
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            app.UseRouting();
            //app.UseCors(builder => builder
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader());
            //TODO: Uncomment this if you need wwwroot folder
            // app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger(options =>
            {
                options.SerializeAsV2 = Configuration.GetValue<bool>("OPENAPI_JSON_VERSION_2");
            });
            app.UseSwaggerUI(c =>
            {
                //TODO: Either use the SwaggerGen generated Swagger contract (generated from C# classes)
                c.SwaggerEndpoint("/swagger/Final-Draft/swagger.json", "DotAAS Part 2 | HTTP/REST | Asset Administration Shell Repository");

                //TODO: Or alternatively use the original Swagger contract that's included in the static files
                // c.SwaggerEndpoint("/swagger-original.json", "DotAAS Part 2 | HTTP/REST | Asset Administration Shell Repository Original");
            });

            //TODO: Use Https Redirection
            // app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                if (env.IsDevelopment())
                    endpoints.MapControllers().WithMetadata(new AllowAnonymousAttribute());
                else
                    endpoints.MapControllers();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //TODO: Enable production exception handling (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
                app.UseExceptionHandler("/Error");

                app.UseHsts();
            }
        }
    }
}
