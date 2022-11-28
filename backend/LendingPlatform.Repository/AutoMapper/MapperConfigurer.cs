using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LendingPlatform.Repository.AutoMapper
{
    public static class MapperConfigurer
    {
        /// <summary>
        /// Method that configures mapping and instantiate dependencies for profiler helpers (Type converters, value resolvers etc)
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static MapperConfiguration ConfigureMapping(IServiceProvider provider)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
                cfg.ConstructServicesUsing(type => ActivatorUtilities.CreateInstance(provider, type));
            });
            config.AssertConfigurationIsValid();
            return config;
        }
    }
}
