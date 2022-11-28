using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.Repository.AutoMapper;
using LendingPlatform.Repository.Repository.Application;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Repository.Repository.Seed;
using LendingPlatform.Utils.Utils;
using LendingPlatform.Utils.Utils.OCR;
using LendingPlatform.Utils.Utils.Transunion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace LendingPlatform.Repository.Test
{
    public class Bootstrap
    {
        #region public properties
        public readonly IServiceProvider ServiceProvider;
        #endregion

        #region Constructor
        public Bootstrap()
        {
            var services = new ServiceCollection();

            #region Dependecy-Injection

            services.AddScoped<IGlobalRepository, GlobalRepository>();
            services.AddScoped<IEntityRepository, EntityRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IEntityFinanceRepository, EntityFinanceRepository>();
            services.AddScoped<IEntityTaxReturnRepository, EntityTaxReturnRepository>();
            #endregion

            #region Mocks

            //DataRepository
            var dataRepositoryMock = new Mock<IDataRepository>();
            services.AddSingleton(x => dataRepositoryMock);
            services.AddSingleton(x => dataRepositoryMock.Object);

            //QuickbooksUtility
            var quickbooksUtilityMock = new Mock<IQuickbooksUtility>();
            services.AddSingleton(x => quickbooksUtilityMock);
            services.AddSingleton(x => quickbooksUtilityMock.Object);

            //GlobalRepository
            var globalRepositoryMock = new Mock<IGlobalRepository>();
            services.AddSingleton(x => globalRepositoryMock);
            services.AddSingleton(x => globalRepositoryMock.Object);

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

            //SmartyStreetsUtility
            var smartyStreetsUtilityMock = new Mock<ISmartyStreetsUtility>();
            services.AddSingleton(x => smartyStreetsUtilityMock);
            services.AddSingleton(x => smartyStreetsUtilityMock.Object);

            //XeroUtility
            var xeroUtilityMock = new Mock<IXeroUtility>();
            services.AddSingleton(x => xeroUtilityMock);
            services.AddSingleton(x => xeroUtilityMock.Object);

            //SimpleEmailServiceUtility
            var simpleEmailServiceUtilityMock = new Mock<ISimpleEmailServiceUtility>();
            services.AddSingleton(x => simpleEmailServiceUtilityMock);
            services.AddSingleton(x => simpleEmailServiceUtilityMock.Object);

            //ExperianUtility
            var experianUtilityMock = new Mock<IExperianUtility>();
            services.AddSingleton(x => experianUtilityMock);
            services.AddSingleton(x => experianUtilityMock.Object);

            //EquifaxUtility
            var equifaxUtilityMock = new Mock<IEquifaxUtility>();
            services.AddSingleton(x => equifaxUtilityMock);
            services.AddSingleton(x => equifaxUtilityMock.Object);

            //YodleeUtility
            var yodleeUtilityMock = new Mock<IYodleeUtility>();
            services.AddSingleton(x => yodleeUtilityMock);
            services.AddSingleton(x => yodleeUtilityMock.Object);

            //PayPalUtility
            var payPalUtilityMock = new Mock<IPayPalUtility>();
            services.AddSingleton(x => payPalUtilityMock);
            services.AddSingleton(x => payPalUtilityMock.Object);

            //PlaidUtility
            var plaidUtilityMock = new Mock<IPlaidUtility>();
            services.AddSingleton(x => plaidUtilityMock);
            services.AddSingleton(x => plaidUtilityMock.Object);

            // RulesUtility
            var rulesUtilityMock = new Mock<IRulesUtility>();
            services.AddSingleton(x => rulesUtilityMock);
            services.AddSingleton(x => rulesUtilityMock.Object);

            //SquareUtility
            var squareUtilityMock = new Mock<ISquareUtility>();
            services.AddSingleton(x => squareUtilityMock);
            services.AddSingleton(x => squareUtilityMock.Object);

            //StripeUtility
            var stripeUtilityMock = new Mock<IStripeUtility>();
            services.AddSingleton(x => stripeUtilityMock);
            services.AddSingleton(x => stripeUtilityMock.Object);

            //TransunionUtility
            var transunionUtilityMock = new Mock<ITransunionUtility>();
            services.AddSingleton(x => transunionUtilityMock);
            services.AddSingleton(x => transunionUtilityMock.Object);

            //OCRUtility
            var ocrUtilityMock = new Mock<IOCRUtility>();
            services.AddSingleton(x => ocrUtilityMock);
            services.AddSingleton(x => ocrUtilityMock.Object);

            #endregion

            // IMapper
            // Register mapper
            services.AddSingleton(provider => MapperConfigurer.ConfigureMapping(provider));
            services.AddAutoMapper(typeof(MapperActions));

            // Register seeddatabase class
            services.AddSingleton<SeedDatabase>();

            ServiceProvider = services.BuildServiceProvider();
        }
        #endregion
    }
}