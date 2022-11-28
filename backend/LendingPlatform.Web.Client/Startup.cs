using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.Repository.AutoMapper;
using LendingPlatform.Repository.Repository.Application;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Repository.Repository.Seed;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using LendingPlatform.Utils.Utils.OCR;
using LendingPlatform.Utils.Utils.Transunion;
using LendingPlatform.Web.Client.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using System;
using System.Linq;
using Xero.NetStandard.OAuth2.Api;

namespace LendingPlatform.Web.Client
{
    public class Startup
    {

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;

        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LendingPlatformContext>(options =>
               options.UseNpgsql(
                   Configuration.GetConnectionString(StringConstant.LendingPlatformConnection),
                   b => b.MigrationsAssembly("LendingPlatform.DomainModel")));



            services.AddScoped<UserTokenValidator>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = Configuration.GetValue<string>("IdentityServer:Authority");
                options.RequireHttpsMetadata = Configuration.GetValue<string>("Environment") != StringConstant.Local;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = Configuration.GetSection("IdentityServer").GetSection("Audiences").GetChildren().Select(x => x.Value).ToList()
                };
                options.EventsType = typeof(UserTokenValidator);

            });


            services.AddHealthChecks();

            // Register services

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped(typeof(IQuickbooksUtility), typeof(QuickbooksUtility));
            services.AddScoped(typeof(IAccountingApi), typeof(AccountingApi));
            services.AddScoped(typeof(IXeroUtility), typeof(XeroUtility));
            services.AddScoped(typeof(IFileOperationsUtility), typeof(FileOperationsUtility));
            services.AddScoped(typeof(IAmazonServicesUtility), typeof(AmazonServicesUtility));
            services.AddScoped(typeof(ISmartyStreetsUtility), typeof(SmartyStreetsUtility));
            services.AddScoped(typeof(IExperianUtility), typeof(ExperianUtility));
            services.AddScoped(typeof(ISimpleEmailServiceUtility), typeof(SimpleEmailServiceUtility));
            services.AddScoped(typeof(IEquifaxUtility), typeof(EquifaxUtility));
            services.AddScoped(typeof(IYodleeUtility), typeof(YodleeUtility));
            services.AddScoped(typeof(IPlaidUtility), typeof(PlaidUtility));
            services.AddScoped(typeof(IPayPalUtility), typeof(PayPalUtility));
            services.AddScoped(typeof(ISquareUtility), typeof(SquareUtility));
            services.AddScoped(typeof(IStripeUtility), typeof(StripeUtility));
            services.AddScoped(typeof(ITransunionUtility), typeof(TransunionUtility));
            services.AddScoped(typeof(IRulesUtility), typeof(RulesUtility));
            services.AddScoped(typeof(IDataRepository), typeof(DataRepository));
            services.AddScoped<SeedDatabase>();
            services.AddScoped(typeof(IEntityFinanceRepository), typeof(EntityFinanceRepository));
            services.AddScoped(typeof(IApplicationRepository), typeof(ApplicationRepository));
            services.AddScoped(typeof(IEntityRepository), typeof(EntityRepository));
            services.AddScoped(typeof(IGlobalRepository), typeof(GlobalRepository));
            services.AddScoped(typeof(IProductRepository), typeof(ProductRepository));
            services.AddScoped(typeof(IEntityTaxReturnRepository), typeof(EntityTaxReturnRepository));
            services.AddScoped(typeof(IOCRUtility), typeof(OCRUtility));



            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpClient();

            // Register the Swagger services
            services.AddSwaggerDocument(config =>
            {
                config.AddSecurity("bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Description = "Login",
                    Flow = OpenApiOAuth2Flow.Implicit,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            TokenUrl = Configuration.GetValue<string>("IdentityServer:Authority") + "/connect/token",
                            AuthorizationUrl = Configuration.GetValue<string>("IdentityServer:Authority") + "/protocol/openid-connect/auth"
                        }
                    }
                });

                config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
                config.PostProcess = document =>
                {
                    document.Info.Version = Configuration.GetValue<String>("SwaggerDocument:Version");
                    document.Info.Title = Configuration.GetValue<String>("SwaggerDocument:Title");
                    document.Info.Description = Configuration.GetValue<String>("SwaggerDocument:Description");
                };

            });

            if (Configuration.GetValue<bool>("MiniProfiler:IsEnabled"))
            {
                services.AddMiniProfiler(options =>
                {
                    (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                    // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                    options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                    // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                    // (defaults to true, and connection opening/closing is tracked)
                    options.TrackConnectionOpenClose = true;

                    // (Optional) Use something other than the "light" color scheme.
                    // (defaults to "light")
                    options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                    // The below are newer options, available in .NET Core 3.0 and above:

                    // (Optional) You can disable MVC filter profiling
                    // (defaults to true, and filters are profiled)
                    options.EnableMvcFilterProfiling = true;

                    // (Optional) You can disable MVC view profiling
                    // (defaults to true, and views are profiled)
                    options.EnableMvcViewProfiling = true;
                }).AddEntityFramework();
            }

            // To persist protection API keys that is saved in disc when app scales 
            if (Configuration.GetValue<string>("Environment") != StringConstant.Local)
            {
                services.AddDataProtection()
                    .PersistKeysToAWSSystemsManager(Configuration.GetValue<string>("AwsParameterStorePath"));
            }
            else
            {
                services.AddDataProtection();
            }

            // Register mapper
            services.AddSingleton(provider => MapperConfigurer.ConfigureMapping(provider));
            services.AddAutoMapper(typeof(MapperActions));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            LendingPlatformContext lendingPlatformContext, SeedDatabase seedData)
        {

            // Pipeline to catch custom exceptions globally
            if (Configuration.GetValue<string>("Environment") == StringConstant.Local)
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseSwaggerUi3();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            GlobalExceptionHandler.HandleCustomException(app);
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(builder =>
                builder.WithOrigins(Configuration.GetSection("Origins").GetSection("Frontend").GetChildren().Select(x => x.Value).ToArray())
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("x-miniprofiler-ids"));

            app.UseAuthentication();
            app.UseAuthorization();
            if (Configuration.GetValue<bool>("MiniProfiler:IsEnabled"))
            {
                app.UseMiniProfiler();
            }
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health-check");
            });

            app.UseOpenApi();
            app.UseReDoc(options =>
            {
                options.Path = Configuration.GetValue<String>("Redoc:Path");
                options.DocumentPath = Configuration.GetValue<String>("Redoc:DocumentPath");
            });

            // Migrate and seed the Database.
            using (lendingPlatformContext)
            {
                lendingPlatformContext.Database.Migrate();
                seedData.SeedAsync().GetAwaiter().GetResult();
            }


        }
    }
}