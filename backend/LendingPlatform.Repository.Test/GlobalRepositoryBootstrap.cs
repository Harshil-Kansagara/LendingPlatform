using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.Repository.AutoMapper;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace LendingPlatform.Repository.Test
{
    public class GlobalRepositoryBootstrap
    {
        #region public properties
        public readonly IServiceProvider ServiceProvider;
        #endregion

        #region Constructor
        public GlobalRepositoryBootstrap()
        {
            var services = new ServiceCollection();

            #region Dependecy-Injection
            services.AddScoped<IGlobalRepository, GlobalRepository>();
            #endregion

            #region Mocks

            //DataRepository
            var dataRepositoryMock = new Mock<IDataRepository>();
            services.AddSingleton(x => dataRepositoryMock);
            services.AddSingleton(x => dataRepositoryMock.Object);

            //IConfiguration
            var configurationMock = new Mock<IConfiguration>();
            services.AddSingleton(x => configurationMock);
            services.AddSingleton(x => configurationMock.Object);

            //IFileOperationsUtility
            var fileOperationsUtilityMock = new Mock<IFileOperationsUtility>();
            services.AddSingleton(x => fileOperationsUtilityMock);
            services.AddSingleton(x => fileOperationsUtilityMock.Object);

            //AmazonS3Utility
            var amazonS3UtilityMock = new Mock<IAmazonServicesUtility>();
            services.AddSingleton(x => amazonS3UtilityMock);
            services.AddSingleton(x => amazonS3UtilityMock.Object);

            // IMapper
            // Register mapper
            services.AddSingleton(provider => MapperConfigurer.ConfigureMapping(provider));
            services.AddAutoMapper(typeof(MapperActions));


            #endregion

            ServiceProvider = services.BuildServiceProvider();
        }
        #endregion
    }
}