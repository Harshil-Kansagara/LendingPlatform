using LendingPlatform.Repository.CustomException;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LendingPlatform.Web.Client
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration).Enrich.FromLogContext().Filter
                         .ByExcluding(x => (x.Exception is InvalidResourceAccessException
                         || x.Exception is DataNotFoundException
                         || x.Exception is InvalidParameterException
                         || x.Exception is ValidationException)));


    }
}
