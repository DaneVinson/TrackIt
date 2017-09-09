using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TrackIt.Data.EFCore;
using TrackIt.Domain.Logic.Managers;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TrackIt.Data.Dapper;
using TrackIt.Data.DocumentDB;

namespace TrackIt.Service.CoreWebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                                    .SetBasePath(env.ContentRootPath)
                                    .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                                    .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true)
                                    .AddEnvironmentVariables()
                                    .Build();

            // Initialize readonly fields.
            AadClientId = Configuration["aad:clientId"] ?? String.Empty;
            AadPolicyId = Configuration["aad:policyId"] ?? String.Empty;
            AadTenant = Configuration["aad:tenant"] ?? String.Empty;
            DocumentDbAccountKey = Configuration["documentDB:AccountKey"] ?? String.Empty;
            DocumentDbAccountUri = Configuration["documentDB:AccountUri"] ?? String.Empty;
            DocumentDbDatabaseName = Configuration["documentDB:DatabaseName"] ?? String.Empty;
            RepositoryType = Configuration["appSettings:repository"] ?? String.Empty;
            TrackItSqlConnectionAlpha = Configuration["ConnectionStrings:TrackItAlpha"] ?? String.Empty;
            TrackItSqlConnectionBravo = Configuration["ConnectionStrings:TrackItBravo"] ?? String.Empty;

            // AAD policy Ids to lower-case to avoid casing issues.
            AadPolicyId = AadPolicyId.ToLower();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        // It gets run after ConfigureServices.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseCors("CorsPolicy");
#if !DEBUG
            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                Audience = AadClientId,
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                MetadataAddress = $"https://login.microsoftonline.com/{AadTenant}/v2.0/.well-known/openid-configuration?p={AadPolicyId}",

                Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = (ctx) =>
                    {
                        ctx.SkipToNextMiddleware();
                        return Task.FromResult(0);
                    }
                }
            });
#endif
            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container
        // It gets run before Configure.
        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: re-evaluate CORS later
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins("*")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddMvc();

            // TrackIt data managers.
            services.AddScoped<ICategoryManager, CategoryManager>();
            services.AddScoped<IDataPointManager, DataPointManager>();

            // Set repository dependencies by configuration
            switch (RepositoryType.ToLower())
            {
                case "dapper":
                    services.AddTransient<IRepository<Category>, TrackIt.Data.Dapper.Repository<Category>>();
                    services.AddTransient<IRepository<DataPoint>, TrackIt.Data.Dapper.Repository<DataPoint>>();
                    services.AddSingleton<IDapperConfiguration>(new DapperConfiguration(TrackItSqlConnectionBravo));
                    break;
                case "documentdb":
                    services.AddTransient<IRepository<Category>, TrackIt.Data.DocumentDB.Repository<Category>>();
                    services.AddTransient<IRepository<DataPoint>, TrackIt.Data.DocumentDB.Repository<DataPoint>>();
                    services.AddSingleton<IDocumentDBConfiguration>(new DocumentDBConfiguration(DocumentDbAccountKey, DocumentDbAccountUri, DocumentDbDatabaseName));
                    break;
                case "efcore":
                    services.AddTransient<IRepository<Category>, TrackIt.Data.EFCore.Repository<Category>>();
                    services.AddTransient<IRepository<DataPoint>, TrackIt.Data.EFCore.Repository<DataPoint>>();
                    services.AddSingleton<IDbConfiguration>(new DbConfiguration(TrackItSqlConnectionAlpha));
                    break;
                default:
                    throw new InvalidOperationException($"The repository {RepositoryType} is not known.");
            }
        }

#region Readonly Plumbing

        private readonly string AadClientId;
        private readonly string AadPolicyId;
        private readonly string AadTenant;
        private readonly IConfigurationRoot Configuration;
        private readonly string DocumentDbAccountKey;
        private readonly string DocumentDbAccountUri;
        private readonly string DocumentDbDatabaseName;
        private readonly string RepositoryType;
        private readonly string TrackItSqlConnectionAlpha;
        private readonly string TrackItSqlConnectionBravo;

#endregion
    }
}
