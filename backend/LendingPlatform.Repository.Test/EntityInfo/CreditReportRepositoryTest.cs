using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.ApplicationClass.EntityInfo;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace LendingPlatform.Repository.Test.EntityInfo
{
    [Collection("Register Dependency")]
    public class CreditReportRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly ICreditReportRepository _creditReportRepository;
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        #endregion

        #region Constructor
        public CreditReportRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _creditReportRepository = bootstrap.ServiceProvider.GetService<ICreditReportRepository>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _dataRepositoryMock.Reset();
        }
        #endregion

        #region Private Method


        #endregion

        #region Public Methods

        /// <summary>
        /// Check if method returns null if credit report does not exists
        /// </summary>
        [Fact]
        public async Task GetCreditReportDetailsAsync_CreditReportNotFound_AssertNull()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            EntityCreditReportJsonAC expected = null;
            List<CreditReport> creditReportList = new List<CreditReport>();

            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _creditReportRepository.GetCreditReportDetailsAsync(loanApplicationId, entityId);

            //Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Check if mehtods successfully returns credit report
        /// </summary>
        [Fact]
        public async Task GetCreditReportDetailsAsync_CreditReportExist_AssertCreditReportResponse()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            List<CreditReport> creditReportList = new List<CreditReport>
            {
                new CreditReport
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    CommercialScore = 50,
                    FsrScore = 50,
                    LoanApplicationId = Guid.NewGuid(),
                    Response = "{'fsrScore':50, 'commercialScore':31}"
                }
            };
            EntityCreditReportJsonAC expected = new EntityCreditReportJsonAC
            {
                CreditReportJson = JObject.Parse(creditReportList.Single().Response),
                EntityId = entityId
            };

            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _creditReportRepository.GetCreditReportDetailsAsync(loanApplicationId, entityId);

            //Assert
            Assert.Equal(expected.CreditReportJson, actual.CreditReportJson);
            Assert.Equal(expected.EntityId, actual.EntityId);
        }

        /// <summary>
        /// Check if mehtods successfully returns credit report for transunion
        /// </summary>
        [Fact]
        public async Task GetCreditReportDetailsAsync_CreditReportTransunionExist_AssertCreditReportResponse()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            List<CreditReport> creditReportList = new List<CreditReport>
            {
                new CreditReport
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    CommercialScore = 50,
                    FsrScore = 50,
                    LoanApplicationId = Guid.NewGuid(),
                    Response = "<xyz>hi</xyz>",
                    ResponseSource = StringConstant.TransunionAPI
                }
            };
            JObject json = JObject.Parse("{'xyz':'hi'}");
            EntityCreditReportJsonAC expected = new EntityCreditReportJsonAC
            {
                CreditReportJson = json,
                EntityId = entityId
            };

            _dataRepositoryMock.Setup(x => x.Fetch<CreditReport>(It.IsAny<Expression<Func<CreditReport, bool>>>()))
                .Returns(creditReportList.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.ConvertXmlToJson(It.IsAny<string>())).Returns(json);

            //Act
            var actual = await _creditReportRepository.GetCreditReportDetailsAsync(loanApplicationId, entityId);

            //Assert
            Assert.Equal(expected.CreditReportJson, actual.CreditReportJson);
            Assert.Equal(expected.EntityId, actual.EntityId);
        }
        #endregion
    }
}
