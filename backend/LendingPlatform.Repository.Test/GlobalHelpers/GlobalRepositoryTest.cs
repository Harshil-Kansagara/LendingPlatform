using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
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

namespace LendingPlatform.Repository.Test.GlobalHelpers
{
    [Collection("GlobalRepository Register Dependency")]
    public class GlobalRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly IGlobalRepository _globalRepository;
        private readonly Mock<IConfiguration> _configuration;
        #endregion

        #region Constructor
        public GlobalRepositoryTest(GlobalRepositoryBootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _globalRepository = bootstrap.ServiceProvider.GetService<IGlobalRepository>();
            _configuration = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            bootstrap.ServiceProvider.GetService<IMapper>();
            _dataRepositoryMock.Reset();
            _configuration.Reset();
        }
        #endregion

        #region Private method(s)

        /// <summary>
        /// Get list of phone number
        /// </summary>
        /// <returns>Returns list of phone number</returns>
        private List<string> GetPhoneNumberList()
        {
            return new List<string>()
            {
                "9090909090",
                "9999999999"
            };
        }

        /// <summary>
        /// Get object of Company
        /// </summary>
        /// <returns>Returns object of Company class</returns>
        private Company GetCompany()
        {
            return new Company
            {
                Id = Guid.NewGuid(),
                CIN = "369258178",
                BusinessAgeId = Guid.NewGuid(),
                CompanySizeId = Guid.NewGuid(),
                CompanyStructureId = Guid.NewGuid(),
                NAICSIndustryTypeId = Guid.NewGuid(),
                Name = "amazon",
            };
        }
        /// <summary>
        /// Get EntityRelationshipMapping list.
        /// </summary>
        /// <returns></returns>
        private List<EntityRelationshipMapping> GetEntityRelationshipMappingList()
        {
            return new List<EntityRelationshipMapping>() {
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid()
                },
                new EntityRelationshipMapping
                {
                    Id = Guid.NewGuid(),
                    PrimaryEntityId = Guid.NewGuid(),
                    RelativeEntityId = Guid.NewGuid()
                }
            };
        }

        /// <summary>
        /// Get GetEntityFinanceYearlyMappingACs list.
        /// </summary>
        /// <returns></returns>
        //private List<EntityFinanceYearlyMappingAC> GetEntityFinanceYearlyMappingACList()
        //{
        //    Guid financialAccountTypeId = Guid.NewGuid();
        //    return new List<EntityFinanceYearlyMappingAC>
        //        {
        //            new EntityFinanceYearlyMappingAC
        //            {
        //                Period = "Jan - Dec 2018",
        //                EntityFinanceStandardAccounts = new List<EntityFinanceStandardAccountsAC>
        //                {
        //                    new EntityFinanceStandardAccountsAC
        //                    {
        //                        FinancialAccountTypeId = financialAccountTypeId,
        //                        Amount = 100
        //                    },
        //                    new EntityFinanceStandardAccountsAC
        //                    {
        //                        FinancialAccountTypeId = financialAccountTypeId,
        //                        Amount = 100
        //                    }
        //                }
        //            }
        //        };
        //}

        /// <summary>
        /// Get the audit log entity object json.
        /// </summary>
        /// <returns>string auditLogs.</returns>
        private string GetAuditLogJson(string tableName, string action = StringConstant.AuditLogUpdateActionName)
        {
            if (tableName.Equals(nameof(User)))
            {
                return "{\"Table\":\"User\",\"Action\":\"Update\",\"PrimaryKey\":{\"Id\":\"ae8d926f-793d-4b73-af39-f5bd2865d6bc\"},\"Changes\":[{\"ColumnName\":\"DOB\",\"OriginalValue\":\"1990-02-09T13:30:00\",\"NewValue\":\"1987-01-06T18:30:00Z\"},{\"ColumnName\":\"Email\",\"OriginalValue\":\"mohammedsabir@promactinfo.com\",\"NewValue\":\"mohammedsabir@promactinfo.com\"},{\"ColumnName\":\"FirstName\",\"OriginalValue\":\"sabir\",\"NewValue\":\"ROLAND\"},{\"ColumnName\":\"HasAnyJudgementsSelfDeclared\",\"OriginalValue\":false,\"NewValue\":false},{\"ColumnName\":\"HasBankruptcySelfDeclared\",\"OriginalValue\":false,\"NewValue\":false},{\"ColumnName\":\"LastName\",\"OriginalValue\":\"shaikh\",\"NewValue\":\"SERMANUKIAN\"},{\"ColumnName\":\"MiddleName\"},{\"ColumnName\":\"Phone\",\"OriginalValue\":\"+19106657800\",\"NewValue\":\"+19106657800\"},{\"ColumnName\":\"ResidencyStatus\",\"OriginalValue\":1,\"NewValue\":1},{\"ColumnName\":\"SSN\",\"OriginalValue\":\"696969410\",\"NewValue\":\"666386948\"},{\"ColumnName\":\"SelfDeclaredCreditScore\",\"OriginalValue\":\"651-700\",\"NewValue\":\"651-700\"}],\"ColumnValues\":{\"DOB\":\"1987-01-06T18:30:00Z\",\"Email\":\"mohammedsabir@promactinfo.com\",\"FirstName\":\"ROLAND\",\"HasAnyJudgementsSelfDeclared\":false,\"HasBankruptcySelfDeclared\":false,\"LastName\":\"SERMANUKIAN\",\"MiddleName\":null,\"Phone\":\"+19106657800\",\"ResidencyStatus\":1,\"SSN\":\"666386948\",\"SelfDeclaredCreditScore\":\"651-700\"},\"Valid\":true}";
            }
            else if (tableName.Equals(nameof(Address)) && action.Equals(StringConstant.AuditLogInsertActionName))
            {
                return "{\"Table\":\"Address\",\"Action\":\"Insert\",\"PrimaryKey\":{\"Id\":\"524e9ba3-822a-4807-89e1-b84234b81c49\"},\"ColumnValues\":{\"City\":\"Centennial\",\"PrimaryNumber\":\"2001\",\"SecondaryDesignator\":null,\"SecondaryNumber\":null,\"StateAbbreviation\":\"CO\",\"StreetLine\":\"Phillips\",\"StreetSuffix\":\"Cir\",\"ZipCode\":\"80122\"},\"Valid\":true}";
            }
            else if (tableName.Equals(nameof(Address)) && action.Equals(StringConstant.AuditLogUpdateActionName))
            {
                return "{\"Table\":\"Address\",\"Action\":\"Update\",\"PrimaryKey\":{\"Id\":\"fd089048-74ab-4b6b-8d0e-babb9164b3c0\"},\"Changes\":[{\"ColumnName\":\"City\",\"OriginalValue\":\"Gilbert\",\"NewValue\":\"Milton\"},{\"ColumnName\":\"PrimaryNumber\",\"OriginalValue\":\"3301\",\"NewValue\":\"5168\"},{\"ColumnName\":\"SecondaryDesignator\"},{\"ColumnName\":\"SecondaryNumber\"},{\"ColumnName\":\"StateAbbreviation\",\"OriginalValue\":\"AZ\",\"NewValue\":\"FL\"},{\"ColumnName\":\"StreetLine\",\"OriginalValue\":\"Greenfield\",\"NewValue\":\"Hawks Nest\"},{\"ColumnName\":\"StreetSuffix\",\"OriginalValue\":\"Rd\",\"NewValue\":\"Dr\"},{\"ColumnName\":\"ZipCode\",\"OriginalValue\":\"85297\",\"NewValue\":\"32570\"}],\"ColumnValues\":{\"City\":\"Milton\",\"PrimaryNumber\":\"5168\",\"SecondaryDesignator\":null,\"SecondaryNumber\":null,\"StateAbbreviation\":\"FL\",\"StreetLine\":\"Hawks Nest\",\"StreetSuffix\":\"Dr\",\"ZipCode\":\"32570\"},\"Valid\":true}";
            }
            else if (tableName.Equals(nameof(EntityLoanApplicationConsent)))
            {
                return "{\"Table\":\"EntityLoanApplicationConsent\",\"Action\":\"Insert\",\"PrimaryKey\":{\"Id\":\"0798b186-fa12-48c8-9099-90a85c8b8080\"},\"ColumnValues\":{\"ConsentId\":\"6720d807-7952-4e17-9695-e919954e4817\"},\"Valid\":true}";
            }
            else if (tableName.Equals(nameof(Consent)))
            {
                return "{\"Table\":\"Consent\",\"Action\":\"Insert\",\"PrimaryKey\":{\"Id\":\"6720d807-7952-4e17-9695-e919954e4817\"},\"Entity\":{\"Id\":\"6720d807-7952-4e17-9695-e919954e4817\",\"ConsentText\":\"Credit Agency and References\",\"IsEnabled\":true},\"ColumnValues\":{\"ConsentText\":\"Credit Agency and References\",\"IsEnabled\":true},\"Valid\":true}";
            }
            else if (tableName.Equals(nameof(EntityLoanApplicationMapping)))
            {
                return "{\"Table\":\"EntityLoanApplicationMapping\",\"Action\":\"Insert\",\"PrimaryKey\":{\"Id\":\"891be916-4495-4dc8-9140-03e4f8912742\"},\"ColumnValues\":{\"EntityId\":\"0d40e968-9cb8-4cba-becd-a782cb66a186\"},\"Valid\":true}";
            }
            else if (tableName.Equals(nameof(Company)))
            {
                return "{\"Table\":\"Company\",\"Action\":\"Insert\",\"PrimaryKey\":{\"Id\":\"0d40e968-9cb8-4cba-becd-a782cb66a186\"},\"ColumnValues\":{\"BusinessAgeId\":\"717c2862-a347-4b11-9979-c303031204d0\",\"CIN\":\"696969410\",\"CompanyFiscalYearStartMonth\":null,\"CompanyRegisteredState\":null,\"CompanySizeId\":\"92f3c649-fe32-49c5-8efe-fd6c7a1a5fb6\",\"CompanyStructureId\":\"4e25bc9d-660f-45b8-85da-26a3e469be62\",\"IndustryExperienceId\":\"9e420aff-ea63-441b-8372-a85c89f0255e\",\"NAICSIndustryTypeId\":\"00d3e8fd-df5b-40d5-972f-a3f426e235e4\",\"Name\":\"Modware premium\"},\"Valid\":true}";
            }
            else if (tableName.Equals(nameof(CompanyStructure)))
            {
                return "{\"Table\":\"CompanyStructure\",\"Action\":\"Insert\",\"PrimaryKey\":{\"Id\":\"4e25bc9d-660f-45b8-85da-26a3e469be62\"},\"Entity\":{\"Id\":\"4e25bc9d-660f-45b8-85da-26a3e469be62\",\"Structure\":\"Proprietorship\",\"Order\":1,\"IsEnabled\":false},\"ColumnValues\":{\"Id\":\"4e25bc9d-660f-45b8-85da-26a3e469be62\",\"IsEnabled\":false,\"Order\":1,\"Structure\":\"Proprietorship\"},\"Valid\":true}";
            }
            else if (tableName.Equals(nameof(LoanApplication)) && action.Equals(StringConstant.AuditLogInsertActionName))
            {
                return "{\"Table\":\"LoanApplication\",\"Action\":\"Insert\",\"PrimaryKey\":{\"Id\":\"5e80abf4-4ea3-48bc-b87c-6caab4a0aaee\"},\"Entity\":{\"Id\":\"5e80abf4-4ea3-48bc-b87c-6caab4a0aaee\",\"Status\":0,\"SectionId\":\"402c41bb-b108-485a-a01b-84fccdfe6a15\",\"Section\":{\"Id\":\"402c41bb-b108-485a-a01b-84fccdfe6a15\",\"Name\":\"Company Info\",\"Order\":2,\"IsEnabled\":true},\"LoanAmount\":53000.0,\"LoanPurposeId\":\"30a2f343-7dd0-4c6f-93ad-d4a2d7dfd60a\",\"LoanPeriod\":3.0,\"LoanApplicationNumber\":\"LP3112202010023553\",\"CreatedOn\":\"2020-12-31T10:02:35.5518591Z\",\"CreatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"CreatedByUser\":{\"Id\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"FirstName\":\"sabir\",\"LastName\":\"shaikh\",\"Email\":\"mohammedsabir@promactinfo.com\",\"ResidencyStatus\":0,\"SelfDeclaredCreditScore\":\">750\",\"HasBankruptcySelfDeclared\":false,\"HasAnyJudgementsSelfDeclared\":false,\"IsRegistered\":true,\"CreatedOn\":\"2020-12-31T10:02:14.697779\",\"UpdatedOn\":\"2020-12-31T10:02:23.429301\",\"CreatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\"}},\"ColumnValues\":{\"Id\":\"5e80abf4-4ea3-48bc-b87c-6caab4a0aaee\",\"CreatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"CreatedOn\":\"2020-12-31T10:02:35.5518591Z\",\"EMIDeducteeBankId\":null,\"EvaluationComments\":null,\"LoanAmount\":53000.0,\"LoanAmountDepositeeBankId\":null,\"LoanApplicationNumber\":\"LP3112202010023553\",\"LoanPeriod\":3.0,\"LoanPurposeId\":\"30a2f343-7dd0-4c6f-93ad-d4a2d7dfd60a\",\"ProductId\":null,\"SectionId\":\"402c41bb-b108-485a-a01b-84fccdfe6a15\",\"Status\":0,\"StatusUpdatedByBankUserId\":null,\"UpdatedByUserId\":null,\"UpdatedOn\":null},\"Valid\":true}";
            }
            else
            {
                return "{\"Table\":\"LoanApplication\",\"Action\":\"Update\",\"PrimaryKey\":{\"Id\":\"5e80abf4-4ea3-48bc-b87c-6caab4a0aaee\"},\"Entity\":{\"Id\":\"5e80abf4-4ea3-48bc-b87c-6caab4a0aaee\",\"Status\":0,\"SectionId\":\"10693d87-54f3-45d9-8aa0-5a803434f306\",\"Section\":{\"Id\":\"10693d87-54f3-45d9-8aa0-5a803434f306\",\"Name\":\"Finances\",\"Order\":3,\"IsEnabled\":true},\"LoanAmount\":53000.0,\"LoanPurposeId\":\"30a2f343-7dd0-4c6f-93ad-d4a2d7dfd60a\",\"LoanPeriod\":3.0,\"LoanApplicationNumber\":\"LP3112202010023553\",\"CreatedOn\":\"2020-12-31T10:02:35.551859\",\"UpdatedOn\":\"2020-12-31T10:48:01.2316509Z\",\"CreatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"CreatedByUser\":{\"Id\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"FirstName\":\"sabir\",\"LastName\":\"shaikh\",\"Email\":\"mohammedsabir@promactinfo.com\",\"SSN\":\"123123133\",\"Phone\":\"+919909272159\",\"ResidencyStatus\":0,\"SelfDeclaredCreditScore\":\">750\",\"HasBankruptcySelfDeclared\":false,\"HasAnyJudgementsSelfDeclared\":false,\"IsRegistered\":true,\"CreatedOn\":\"2020-12-31T10:02:14.697779\",\"UpdatedOn\":\"2020-12-31T10:48:01.069748\",\"CreatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"UpdatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\"},\"UpdatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"UpdatedByUser\":{\"Id\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"FirstName\":\"sabir\",\"LastName\":\"shaikh\",\"Email\":\"mohammedsabir@promactinfo.com\",\"SSN\":\"123123133\",\"Phone\":\"+919909272159\",\"ResidencyStatus\":0,\"SelfDeclaredCreditScore\":\">750\",\"HasBankruptcySelfDeclared\":false,\"HasAnyJudgementsSelfDeclared\":false,\"IsRegistered\":true,\"CreatedOn\":\"2020-12-31T10:02:14.697779\",\"UpdatedOn\":\"2020-12-31T10:48:01.069748\",\"CreatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"UpdatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\"}},\"Changes\":[{\"ColumnName\":\"CreatedByUserId\",\"OriginalValue\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"NewValue\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\"},{\"ColumnName\":\"CreatedOn\",\"OriginalValue\":\"2020-12-31T10:02:35.551859\",\"NewValue\":\"2020-12-31T10:02:35.551859\"},{\"ColumnName\":\"EMIDeducteeBankId\"},{\"ColumnName\":\"EvaluationComments\"},{\"ColumnName\":\"LoanAmount\",\"OriginalValue\":53000.0,\"NewValue\":53000.0},{\"ColumnName\":\"LoanAmountDepositeeBankId\"},{\"ColumnName\":\"LoanApplicationNumber\",\"OriginalValue\":\"LP3112202010023553\",\"NewValue\":\"LP3112202010023553\"},{\"ColumnName\":\"LoanPeriod\",\"OriginalValue\":3.0,\"NewValue\":3.0},{\"ColumnName\":\"LoanPurposeId\",\"OriginalValue\":\"30a2f343-7dd0-4c6f-93ad-d4a2d7dfd60a\",\"NewValue\":\"30a2f343-7dd0-4c6f-93ad-d4a2d7dfd60a\"},{\"ColumnName\":\"ProductId\"},{\"ColumnName\":\"SectionId\",\"OriginalValue\":\"402c41bb-b108-485a-a01b-84fccdfe6a15\",\"NewValue\":\"10693d87-54f3-45d9-8aa0-5a803434f306\"},{\"ColumnName\":\"Status\",\"OriginalValue\":0,\"NewValue\":0},{\"ColumnName\":\"StatusUpdatedByBankUserId\"},{\"ColumnName\":\"UpdatedByUserId\",\"NewValue\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\"},{\"ColumnName\":\"UpdatedOn\",\"NewValue\":\"2020-12-31T10:48:01.2316509Z\"}],\"ColumnValues\":{\"Id\":\"5e80abf4-4ea3-48bc-b87c-6caab4a0aaee\",\"CreatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"CreatedOn\":\"2020-12-31T10:02:35.551859\",\"EMIDeducteeBankId\":null,\"EvaluationComments\":null,\"LoanAmount\":53000.0,\"LoanAmountDepositeeBankId\":null,\"LoanApplicationNumber\":\"LP3112202010023553\",\"LoanPeriod\":3.0,\"LoanPurposeId\":\"30a2f343-7dd0-4c6f-93ad-d4a2d7dfd60a\",\"ProductId\":null,\"SectionId\":\"10693d87-54f3-45d9-8aa0-5a803434f306\",\"Status\":0,\"StatusUpdatedByBankUserId\":null,\"UpdatedByUserId\":\"66579ad2-2471-4cc3-be2b-9c44ce3fdfe6\",\"UpdatedOn\":\"2020-12-31T10:48:01.2316509Z\"},\"Valid\":true}";
            }
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Check if phone number list don't have unique phone number then throws ValidationException
        /// </summary>
        [Fact]
        public async Task IsUniquePhoneNumberAsync_NotUniquePhoneNumber_VerifyThrowsValidationException()
        {
            //Arrange
            List<string> phoneNumberList = GetPhoneNumberList();

            List<User> checkUserList = new List<User>()
            {
                new User
                {
                    Email = "arjun@promactinfo.com",
                    Id = Guid.NewGuid(),
                    FirstName = "Arjunsinh",
                    LastName = "Jadeja",
                    Phone = "9090909090",
                    SSN = "123456789"
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch<User>(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(checkUserList.AsQueryable().BuildMock().Object);

            //Act


            //Assert

            await Assert.ThrowsAsync<ValidationException>(async () => await _globalRepository.IsUniquePhoneNumberAsync(phoneNumberList));
            _dataRepositoryMock.Verify(x => x.Fetch<User>(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        }


        /// <summary>
        /// Check if  phone number list has all unique numbers then verify only one fetch call and method don't throw any exception
        /// </summary>
        [Fact]
        public async Task IsUniquePhoneNumberAsync_UniquePhoneNumbers_VerifyOneFetchCall()
        {
            //Arrange
            List<string> phoneNumberList = GetPhoneNumberList();

            List<User> checkUserList = new List<User>();

            _dataRepositoryMock.Setup(x => x.Fetch<User>(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(checkUserList.AsQueryable().BuildMock().Object);

            //Act
            await _globalRepository.IsUniquePhoneNumberAsync(phoneNumberList);

            //Assert

            _dataRepositoryMock.Verify(x => x.Fetch<User>(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        }

        #region IsValidCIN Method
        /// <summary>
        /// Check valid CIN.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void IsValidCIN_PerformCheck_AssertValidateToValidCIN()
        {
            //Arrange
            string cin = "112233554";

            //Act
            bool isValid = _globalRepository.IsValidCIN(cin);

            //Assert
            Assert.True(isValid);
        }
        /// <summary>
        /// Check In valid CIN.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void IsValidCIN_PerformCheck_AssertValidateToInValidCIN()
        {
            //Arrange
            string cin = "1122xd33";

            //Act
            bool isValid = _globalRepository.IsValidCIN(cin);

            //Assert
            Assert.False(isValid);
        }
        #endregion

        #region IsUniqueEINAsync Method
        /// <summary>
        /// Check company unique EIN.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsUniqueEINAsync_PerformCheck_AssertValidateToUniqueEINAsync()
        {
            //Arrange
            Company company = null;
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Company, bool>>>())).Returns(Task.FromResult(company));
            string cin = "1122xd33";

            //Act
            bool isUniqueEin = await _globalRepository.IsUniqueEINAsync(cin);

            //Assert
            Assert.True(isUniqueEin);
        }
        /// <summary>
        /// Check company already exist EIN.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsUniqueEINAsync_PerformCheck_AssertValidateToExistEINAsync()
        {
            //Arrange
            Company company = GetCompany();
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync<Company>(It.IsAny<Expression<Func<Company, bool>>>()))
                .ReturnsAsync(company);

            //Act
            bool isUniqueEin = await _globalRepository.IsUniqueEINAsync(company.CIN);

            //Assert
            Assert.False(isUniqueEin);
        }
        #endregion

        #region CheckEntityRelationshipMappingAsync Method

        /// <summary>
        /// Check valid user mapping to company.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckEntityRelationshipMappingAsync_PerformCheck_AssertValidateToCheckMappingWithUserAndCompanyAsync()
        {
            //Arrange
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappingList();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            Guid entityId = entityRelationshipMappings[0].PrimaryEntityId;
            Guid userId = entityRelationshipMappings[0].RelativeEntityId;

            //Act
            bool isValidUser = await _globalRepository.CheckEntityRelationshipMappingAsync(entityId, userId);

            //Assert
            Assert.True(isValidUser);
        }
        /// <summary>
        /// Check Invalid user mapping to company.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckEntityRelationshipMappingAsync_PerformCheck_AssertValidateToCheckInvalidMappingWithUserAndCompanyAsync()
        {
            //Arrange
            List<EntityRelationshipMapping> entityRelationshipMappings = GetEntityRelationshipMappingList();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            Guid entityId = entityRelationshipMappings[0].PrimaryEntityId;
            Guid userId = entityRelationshipMappings[1].RelativeEntityId;

            //Act
            bool isValidUser = await _globalRepository.CheckEntityRelationshipMappingAsync(entityId, userId);

            //Assert
            Assert.False(isValidUser);
        }
        #endregion

        #region GetFinancialStatementFromNameAsync Method
        /// <summary>
        /// Check the valid financial statement.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetFinancialStatementFromNameAsync_PerformCheck_AssertValidateToValidFinacialStatementAsync()
        {
            //Arrange
            FinancialStatement financialStatement = new FinancialStatement()
            {
                Name = StringConstant.IncomeStatement
            };
            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).Returns(Task.FromResult(financialStatement));

            //Act
            financialStatement = await _globalRepository.GetFinancialStatementFromNameAsync(financialStatement.Name);

            //Assert
            Assert.Equal(StringConstant.IncomeStatement, financialStatement.Name);
        }
        /// <summary>
        ///  Raise a DataNotFoundException while FinancialStatement doesn't exist.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetFinancialStatementFromNameAsync_PerformCheck_AssertValidateToInValidFinacialStatementAsync()
        {
            //Arrange
            FinancialStatement financialStatement = null;

            _dataRepositoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).Returns(Task.FromResult(financialStatement));

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.GetFinancialStatementFromNameAsync(StringConstant.IncomeStatement));
        }
        #endregion

        #region GetListOfLastNFinancialYears Method
        /// <summary>
        /// Check the count of list of years with the financialyears.
        /// </summary>
        [Fact]
        public void GetListOfLastNFinancialYears_PerformCheck_AssertValidateToCountListOfYears()
        {
            int totalYears = 3;
            //Arrange
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns(totalYears.ToString());
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _configuration.Setup(x => x.GetSection("FinancialYear:EndMonth").Value).Returns("December");

            //Act
            var listOfYears = _globalRepository.GetListOfLastNFinancialYears(StringConstant.January, StringConstant.December);

            //Assert
            Assert.Equal(totalYears, listOfYears.Count);
        }
        /// <summary>
        /// Check the count of list of years including current year with the financialyears.
        /// </summary>
        [Fact]
        public void GetListOfLastNFinancialYears_PerformCheck_AssertValidateToCountListOfYearsIncludeCurrentYear()
        {
            int totalYears = 3;
            //Arrange
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns(totalYears.ToString());
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _configuration.Setup(x => x.GetSection("FinancialYear:EndMonth").Value).Returns("December");

            //Act
            var listOfYears = _globalRepository.GetListOfLastNFinancialYears(StringConstant.January, StringConstant.December, true);

            //Assert
            Assert.Equal(totalYears + 1, listOfYears.Count);
        }

        #endregion

        #region AddEntityFinanceYearlyMappingAndStandardAccountAsync Method
        /// <summary>
        /// Check to how many times AddRangeAsync call in the EntityFinanceYearlyMapping, EntityFinanceStandardAccount.
        /// Check to how many times Remove, AddAsync call in the UploadedFinancialDocument.
        /// </summary>
        /// <returns></returns>
        //[Fact]
        //public async Task AddEntityFinanceYearlyMappingAndStandardAccountAsync_PerformAdd_AssertCallToNotAddUploadedFinancialDocumentDatabasesAsync()
        //{
        //    // Arrange
        //    var entityFinanceYearlyMappingACList = GetEntityFinanceYearlyMappingACList();
        //    EntityFinance entityFinance = new EntityFinance();

        //    // Act
        //    await _globalRepository.AddEntityFinanceYearlyMappingAndStandardAccountAsync(entityFinanceYearlyMappingACList, entityFinance, false);

        //    // Assert
        //    _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityFinanceYearlyMapping>(It.IsAny<List<EntityFinanceYearlyMapping>>()), Times.Exactly(1));
        //    _dataRepositoryMock.Verify(x => x.Remove<UploadedFinancialDocument>(It.IsAny<UploadedFinancialDocument>()), Times.Exactly(0));
        //    _dataRepositoryMock.Verify(x => x.AddAsync<UploadedFinancialDocument>(It.IsAny<UploadedFinancialDocument>()), Times.Exactly(0));
        //}

        /// <summary>
        /// Check to how many times AddRangeAsync call in the EntityFinanceYearlyMapping, EntityFinanceStandardAccount.
        /// Check to how many times Remove, AddAsync call in the UploadedFinancialDocument.
        /// Add uploaded document.
        /// </summary>
        /// <returns></returns>
        //[Fact]
        //public async Task AddEntityFinanceYearlyMappingAndStandardAccountAsync_PerformAdd_AssertCallToAddUploadedFinancialDocumentDatabasesAsync()
        //{
        //    // Arrange
        //    var entityFinanceYearlyMappingACList = GetEntityFinanceYearlyMappingACList();
        //    entityFinanceYearlyMappingACList[0].DocumentName = "incomeyear2019";
        //    entityFinanceYearlyMappingACList[0].DocumentPathToDownload = "incomeyear2019";

        //    EntityFinance entityFinance = new EntityFinance();

        //    // Act
        //    await _globalRepository.AddEntityFinanceYearlyMappingAndStandardAccountAsync(entityFinanceYearlyMappingACList, entityFinance, true);

        //    // Assert
        //    _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityFinanceYearlyMapping>(It.IsAny<List<EntityFinanceYearlyMapping>>()), Times.Exactly(1));
        //    _dataRepositoryMock.Verify(x => x.Remove<UploadedFinancialDocument>(It.IsAny<UploadedFinancialDocument>()), Times.Exactly(0));
        //    _dataRepositoryMock.Verify(x => x.AddAsync<UploadedFinancialDocument>(It.IsAny<UploadedFinancialDocument>()), Times.Exactly(1));
        //}

        /// <summary>
        /// Check to how many times AddRangeAsync call in the EntityFinanceYearlyMapping, EntityFinanceStandardAccount.
        /// Check to how many times Remove, AddAsync call in the UploadedFinancialDocument.
        /// Add and Remove uploaded document.
        /// </summary>
        /// <returns></returns>
        //[Fact]
        //public async Task AddEntityFinanceYearlyMappingAndStandardAccountAsync_PerformAdd_AssertCallToAddRemoveUploadedFinancialDocumentDatabasesAsync()
        //{
        //    // Arrange
        //    var entityFinanceYearlyMappingACList = GetEntityFinanceYearlyMappingACList();
        //    entityFinanceYearlyMappingACList[0].UploadedDocumentId = Guid.NewGuid();
        //    entityFinanceYearlyMappingACList[0].DocumentName = "incomeyear2019";
        //    entityFinanceYearlyMappingACList[0].DocumentPathToDownload = "incomeyear2019";
        //    EntityFinance entityFinance = new EntityFinance();

        //    // Act
        //    await _globalRepository.AddEntityFinanceYearlyMappingAndStandardAccountAsync(entityFinanceYearlyMappingACList, entityFinance, true);

        //    // Assert
        //    _dataRepositoryMock.Verify(x => x.AddRangeAsync<EntityFinanceYearlyMapping>(It.IsAny<List<EntityFinanceYearlyMapping>>()), Times.Exactly(1));
        //    _dataRepositoryMock.Verify(x => x.Remove<UploadedFinancialDocument>(It.IsAny<UploadedFinancialDocument>()), Times.Exactly(1));
        //    _dataRepositoryMock.Verify(x => x.AddAsync<UploadedFinancialDocument>(It.IsAny<UploadedFinancialDocument>()), Times.Exactly(1));
        //}

        #endregion



        #region UpdateSectionNameAsync Method
        /// <summary>
        /// Method to check if it throws an exception when loan application not found.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateSectionNameAsync_NoApplicationExist_ThrowsException()
        {
            // Arrange
            Guid loanApplicationId = Guid.NewGuid();
            CurrentUserAC currentUser = new CurrentUserAC();
            List<LoanApplication> loanApplications = new List<LoanApplication>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                  .Returns(loanApplications.AsQueryable().BuildMock().Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.UpdateSectionNameAsync(loanApplicationId, "Loan Needs", currentUser));
        }

        /// <summary>
        /// Method to check if it throws an exception when no any section found.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateSectionNameAsync_NoSectionsExist_ThrowsException()
        {
            // Arrange
            Guid loanApplicationId = Guid.NewGuid();
            CurrentUserAC currentUser = new CurrentUserAC();
            List<LoanApplication> loanApplications = new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = Guid.NewGuid(),
                    CreatedByUserId = currentUser.Id,
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = currentUser.Id,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Id = Guid.NewGuid(), Name = "Loan Needs", Order = 1, IsEnabled = true}
                            }
                        },
                    Status = LoanApplicationStatusType.Draft
                }
            };
            List<Section> sections = new List<Section>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                  .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<Section>(It.IsAny<Expression<Func<Section, bool>>>())).Returns(sections.AsQueryable().BuildMock().Object);

            // Act

            // Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.UpdateSectionNameAsync(loanApplicationId, loanApplications.First().UserLoanSectionMappings.First().Section.Name, currentUser));
        }

        /// <summary>
        /// Update the section id if it is not having last section as a current section.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateSectionNameAsync_CurrentSectionIsNotLastSection_UpdateSectionOfLoanApplication()
        {
            // Arrange
            CurrentUserAC currentUser = new CurrentUserAC { Id = Guid.NewGuid() };
            List<Guid> sectionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            List<LoanApplication> loanApplications = new List<LoanApplication>()
            {
                new LoanApplication
                {
                    Id = Guid.NewGuid(),
                    CreatedByUserId = currentUser.Id,
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = currentUser.Id,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Id = sectionIds.First(), Name = "Loan Needs", Order = 1, IsEnabled = true}
                            }
                        },
                    Status = LoanApplicationStatusType.Draft
                }
            };
            List<Section> sections = new List<Section>
            {
                new Section { Id =  sectionIds.First(), Name = "Loan Needs", Order = 1, IsEnabled = true},
                new Section { Id =  sectionIds.ElementAt(1), Name = "CompanyInfo", Order = 2, IsEnabled = true},
                new Section { Id =  sectionIds.Last(), Name = "Finances", Order = 3, IsEnabled = true}
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                  .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<Section>(It.IsAny<Expression<Func<Section, bool>>>())).Returns(sections.AsQueryable().BuildMock().Object);

            // Act
            await _globalRepository.UpdateSectionNameAsync(loanApplications.First().Id, loanApplications.First().UserLoanSectionMappings.First().Section.Name, currentUser);

            // Assert
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<UserLoanSectionMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// If the current user is shareholder of company linked with loan and current section is personal finance then update it to loan consent..
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateSectionNameAsync_PersonalFinanceSectionShareholder_UpdateSectionOfLoanApplication()
        {
            Guid commonId = Guid.NewGuid();
            // Arrange
            CurrentUserAC currentUser = new CurrentUserAC { Id = Guid.NewGuid() };
            List<Guid> sectionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            List<LoanApplication> loanApplications = new List<LoanApplication>()
            {
                new LoanApplication
                {
                    Id = commonId,
                    CreatedByUserId = Guid.NewGuid(),
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = commonId,
                                UserId = currentUser.Id,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Id = sectionIds.First(), Name = StringConstant.PersonalFinancesSection, Order = 1, IsEnabled = true}
                            }
                        },
                    Status = LoanApplicationStatusType.Draft
                }
            };
            List<Section> sections = new List<Section>
            {
                new Section { Id =  sectionIds.First(), Name = "Loan Needs", Order = 1, IsEnabled = true},
                new Section { Id =  sectionIds.ElementAt(1), Name = StringConstant.PersonalFinancesSection, Order = 2, IsEnabled = true},
                new Section { Id =  sectionIds.Last(), Name = StringConstant.LoanConsentSection, Order = 3, IsEnabled = true}
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                  .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<Section>(It.IsAny<Expression<Func<Section, bool>>>())).Returns(sections.AsQueryable().BuildMock().Object);

            // Act
            var actual = await _globalRepository.UpdateSectionNameAsync(loanApplications.First().Id, loanApplications.First().UserLoanSectionMappings.First().Section.Name, currentUser);

            // Assert
            Assert.Equal(StringConstant.LoanConsentSection, actual);
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<UserLoanSectionMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// If next section is greater then loan consent then update it for all shareholders.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateSectionNameAsync_UpdateSectionOfShareholder_UpdateSectionOfLoanApplication()
        {
            Guid commonId = Guid.NewGuid();
            // Arrange
            CurrentUserAC currentUser = new CurrentUserAC { Id = Guid.NewGuid() };
            List<Guid> sectionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            List<LoanApplication> loanApplications = new List<LoanApplication>()
            {
                new LoanApplication
                {
                    Id = commonId,
                    CreatedByUserId = Guid.NewGuid(),
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = commonId,
                                UserId = currentUser.Id,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Id = sectionIds.First(), Name = StringConstant.LoanConsentSection, Order = 1, IsEnabled = true}
                            }
                        },
                    Status = LoanApplicationStatusType.Draft
                }
            };
            List<Section> sections = new List<Section>
            {
                new Section { Id =  sectionIds.First(), Name = "Loan Needs", Order = 8, IsEnabled = true},
                new Section { Id =  sectionIds.ElementAt(1), Name = StringConstant.LoanConsentSection, Order = 9, IsEnabled = true},
                new Section { Id =  sectionIds.Last(), Name = StringConstant.LoanStatusSection, Order = 10, IsEnabled = true}
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                  .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<Section>(It.IsAny<Expression<Func<Section, bool>>>())).Returns(sections.AsQueryable().BuildMock().Object);

            // Act
            var actual = await _globalRepository.UpdateSectionNameAsync(loanApplications.First().Id, loanApplications.First().UserLoanSectionMappings.First().Section.Name, currentUser);

            // Assert
            Assert.Equal(StringConstant.LoanStatusSection, actual);
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<UserLoanSectionMapping>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        #endregion

        #region GetPathForKeyNameBucket Method
        /// <summary>
        /// Check actual image bucket path for income statement.
        /// </summary>
        //[Fact]
        //public void GetPathForKeyNameBucket_PerformCheck_AssertValidateToIncomeStatementPath()
        //{
        //    //Arrange
        //    EntityFinance entityFinance = new EntityFinance()
        //    {
        //        EntityId = Guid.NewGuid(),
        //        FinancialStatement = new FinancialStatement()
        //        {
        //            Name = StringConstant.IncomeStatement
        //        }
        //    };
        //    EntityFinanceYearlyMappingAC entityFinanceYearMappingObject = new EntityFinanceYearlyMappingAC()
        //    {
        //        Period = "Jan - Dec 2019",
        //        DocumentName = "abc.csv"
        //    };
        //    string expectedPath = $"{entityFinance.EntityId}/{StringConstant.IncomeStatement}/{entityFinanceYearMappingObject.Period}/{entityFinanceYearMappingObject.DocumentName}";

        //    //Act
        //    var actualPath = _globalRepository.GetPathForKeyNameBucket(entityFinance, entityFinanceYearMappingObject);

        //    //Assert
        //    Assert.Equal(expectedPath, actualPath);
        //}

        /// <summary>
        /// Check actual image bucket path for BalanceSheet.
        /// </summary>
        //[Fact]
        //public void GetPathForKeyNameBucket_PerformCheck_AssertValidateToBalanceSheetPath()
        //{
        //    //Arrange
        //    EntityFinance entityFinance = new EntityFinance()
        //    {
        //        EntityId = Guid.NewGuid(),
        //        FinancialStatement = new FinancialStatement()
        //        {
        //            Name = StringConstant.BalanceSheet
        //        }
        //    };
        //    EntityFinanceYearlyMappingAC entityFinanceYearMappingObject = new EntityFinanceYearlyMappingAC()
        //    {
        //        Period = "Jan - Dec 2019",
        //        DocumentName = "abc.csv"
        //    };
        //    string expectedPath = $"{entityFinance.EntityId}/{StringConstant.BalanceSheet}/{entityFinanceYearMappingObject.Period}/{entityFinanceYearMappingObject.DocumentName}";

        //    //Act
        //    var actualPath = _globalRepository.GetPathForKeyNameBucket(entityFinance, entityFinanceYearMappingObject);

        //    //Assert
        //    Assert.Equal(expectedPath, actualPath);
        //}

        /// <summary>
        /// Check actual image bucket path for TaxReturns.
        /// </summary>
        //[Fact]
        //public void GetPathForKeyNameBucket_PerformCheck_AssertValidateToTaxReturnsPath()
        //{
        //    //Arrange
        //    EntityFinance entityFinance = new EntityFinance()
        //    {
        //        EntityId = Guid.NewGuid(),
        //        FinancialStatement = new FinancialStatement()
        //        {
        //            Name = StringConstant.TaxReturns
        //        }
        //    };
        //    EntityFinanceYearlyMappingAC entityFinanceYearMappingObject = new EntityFinanceYearlyMappingAC()
        //    {
        //        Period = "Jan - Dec 2019",
        //        DocumentName = "abc.csv"
        //    };
        //    string expectedPath = $"{entityFinance.EntityId}/{StringConstant.TaxReturns}/{entityFinanceYearMappingObject.Period}/{entityFinanceYearMappingObject.DocumentName}";
        //    //Act
        //    var actualPath = _globalRepository.GetPathForKeyNameBucket(entityFinance, entityFinanceYearMappingObject);

        //    //Assert
        //    Assert.Equal(expectedPath, actualPath);
        //}
        #endregion

        #region IsAddOrUpdateAllowedAsync method

        /// <summary>
        /// Method to check it throws an exception if a loan application does not exist.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsAddOrUpdateAllowedAsync_ApplicationNotExist_ThrowsException()
        {
            //Arrange
            Guid loanApplicationId = Guid.NewGuid();
            List<LoanApplication> loanApplications = new List<LoanApplication>();
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.IsAddOrUpdateAllowedAsync(loanApplicationId));
        }

        /// <summary>
        /// Method to check if it returns false for application with status locked.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsAddOrUpdateAllowedAsync_ApplicationExistsAndStatusIsNotDraft_ReturnsFalse()
        {
            Guid currentUserId = Guid.NewGuid();
            //Arrange
            List<LoanApplication> loanApplications = new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = Guid.NewGuid(),
                    Status = LoanApplicationStatusType.Locked,
                    CreatedByUserId = currentUserId,
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = currentUserId,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Name = StringConstant.LoanStatusSection }
                            }
                        }
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object);

            //Act
            bool isAddOrUpdateAllow = await _globalRepository.IsAddOrUpdateAllowedAsync(loanApplications.First().Id);

            //Assert
            Assert.False(isAddOrUpdateAllow);
        }

        /// <summary>
        /// Method to check if it returns false if initiator has given consent for the loan application.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsAddOrUpdateAllowedAsync_InitiatorHasGivenConsent_ReturnsFalse()
        {
            Guid currentUserId = Guid.NewGuid();
            //Arrange
            List<LoanApplication> loanApplications = new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = Guid.NewGuid(),
                    CreatedByUserId = currentUserId,
                    Status = LoanApplicationStatusType.Locked,
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = currentUserId,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Name = StringConstant.LoanStatusSection }
                            }
                        }
                }
            };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent{
                    LoanApplicationId = loanApplications.First().Id,
                    IsConsentGiven= true,
                    ConsenteeId = loanApplications.First().CreatedByUserId
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(consents.AsQueryable().BuildMock().Object);

            //Act
            bool isAddOrUpdateAllow = await _globalRepository.IsAddOrUpdateAllowedAsync(loanApplications.First().Id);

            //Assert
            Assert.False(isAddOrUpdateAllow);
        }

        /// <summary>
        /// Method to check if it returns true for existing loan application with status draft and all the consents are pending.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsAddOrUpdateAllowedAsync_PerformCheck_AssertValidateToIsReadOnlyAsync()
        {
            Guid currentUserId = Guid.NewGuid();
            //Arrange
            List<LoanApplication> loanApplications = new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = Guid.NewGuid(),
                    CreatedByUserId = currentUserId,
                    Status = LoanApplicationStatusType.Draft,
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = currentUserId,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Name = StringConstant.FinancesSection }
                            }
                        }
                }
            };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent{
                    LoanApplicationId = loanApplications.First().Id,
                    IsConsentGiven= false,
                    ConsenteeId = loanApplications.First().CreatedByUserId
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                 .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(consents.AsQueryable().BuildMock().Object);

            //Act
            bool isAddOrUpdateAllow = await _globalRepository.IsAddOrUpdateAllowedAsync(loanApplications.First().Id);

            //Assert
            Assert.True(isAddOrUpdateAllow);
        }

        /// <summary>
        /// Method to check if loan initiator has given the consent then also the update operation in consent should be allowed.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsAddOrUpdateAllowedAsync_InitiatorGivenConsent_AllowUpdateInLoanConsent()
        {
            Guid currentUserId = Guid.NewGuid();
            //Arrange
            List<LoanApplication> loanApplications = new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = Guid.NewGuid(),
                    CreatedByUserId = currentUserId,
                    Status = LoanApplicationStatusType.Draft,
                    UserLoanSectionMappings = new List<UserLoanSectionMapping>()
                        {
                            new UserLoanSectionMapping
                            {
                                Id= Guid.NewGuid(),
                                LoanApplicationId = Guid.NewGuid(),
                                UserId = currentUserId,
                                SectionId = Guid.NewGuid(),
                                Section = new Section { Name = StringConstant.LoanConsentSection }
                            }
                        }
                }
            };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent
                {
                    LoanApplicationId = loanApplications.First().Id,
                    IsConsentGiven= true,
                    ConsenteeId = loanApplications.First().CreatedByUserId
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                 .Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                .Returns(consents.AsQueryable().BuildMock().Object);

            //Act
            bool isAddOrUpdateAllow = await _globalRepository.IsAddOrUpdateAllowedAsync(loanApplications.First().Id);

            //Assert
            Assert.True(isAddOrUpdateAllow);
        }
        #endregion

        #region UpdateStatusOfLoanApplicationAsync
        /// <summary>
        /// Check to update loan application status on valid loan detail.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateStatusOfLoanApplicationAsync_LoanApplicationExists_PerformsUpdateOperation()
        {
            // Arrange
            CurrentUserAC currentUser = new CurrentUserAC { Id = Guid.NewGuid() };
            LoanApplication loanApplication = new LoanApplication { Id = Guid.NewGuid() };
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
              .ReturnsAsync(loanApplication);

            // Act
            await _globalRepository.UpdateStatusOfLoanApplicationAsync(loanApplication.Id, LoanApplicationStatusType.Approved, currentUser);

            // Assert
            _dataRepositoryMock.Verify(x => x.Update<LoanApplication>(It.IsAny<LoanApplication>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Method to check if it throws an exception for the non-existing loan application.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateStatusOfLoanApplicationAsync_LoanApplicationNotExist_ThrowsException()
        {
            // Arrange
            LoanApplication loanApplication = null;
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
              .ReturnsAsync(loanApplication);

            // Assert        
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.UpdateStatusOfLoanApplicationAsync(Guid.NewGuid(), LoanApplicationStatusType.Approved, new CurrentUserAC()));
        }
        #endregion

        #region IsLoanReadOnly Method

        /// <summary>
        /// Method to check if it returns true for the loan application with status approved.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsLoanReadOnlyAsync_StatusIsApproved_ReturnsTrue()
        {
            //Arrange
            LoanApplication loanApplication = new LoanApplication() { Status = LoanApplicationStatusType.Approved };

            //Act
            bool isReadOnly = await _globalRepository.IsLoanReadOnlyAsync(loanApplication);

            //Assert
            Assert.True(isReadOnly);
        }

        /// <summary>
        /// Method to check if it returns true for the loan application with only initiator given consent.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsLoanReadOnlyAsync_InitiatorHasGivenConsent_ReturnsTrue()
        {
            //Arrange
            LoanApplication loanApplication = new LoanApplication() { Id = Guid.NewGuid(), CreatedByUserId = Guid.NewGuid(), Status = LoanApplicationStatusType.Draft };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent{
                    LoanApplicationId = loanApplication.Id,
                    IsConsentGiven= true,
                    ConsenteeId = loanApplication.CreatedByUserId
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
              .Returns(consents.AsQueryable().BuildMock().Object);

            //Act
            bool isReadOnly = await _globalRepository.IsLoanReadOnlyAsync(loanApplication);

            //Assert
            Assert.True(isReadOnly);
        }

        /// <summary>
        /// Check return true when loan is submitted.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IsLoanReadOnlyAsync_StatusIsDraftAndInitiatorConsentPending_ReturnsFalse()
        {
            //Arrange
            LoanApplication loanApplication = new LoanApplication() { Id = Guid.NewGuid(), CreatedByUserId = Guid.NewGuid(), Status = LoanApplicationStatusType.Draft };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent{
                    LoanApplicationId = loanApplication.Id,
                    IsConsentGiven= false,
                    ConsenteeId = loanApplication.CreatedByUserId
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
              .Returns(consents.AsQueryable().BuildMock().Object);

            //Act
            bool isReadOnly = await _globalRepository.IsLoanReadOnlyAsync(loanApplication);

            //Assert
            Assert.False(isReadOnly);
        }
        #endregion

        #region CheckUserLoanAccessAsync Method

        /// <summary>
        /// Check is not client access. 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckUserLoanAccessAsync_PerformCheck_AssertValidateToIsNotClientAccessAsync()
        {
            //Arrange
            LoanApplication loanApplication = new LoanApplication()
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid()
            };
            CurrentUserAC currentUser = new CurrentUserAC()
            {
                Id = loanApplication.CreatedByUserId
            };
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));

            //Act
            bool isAccess = await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplication.Id, false);

            //Assert
            Assert.True(isAccess);
        }
        /// <summary>
        /// Check Own user created a loan access. 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckUserLoanAccessAsync_PerformCheck_AssertValidateToOwnUserCreatedLoanAccessAsync()
        {
            //Arrange
            LoanApplication loanApplication = new LoanApplication()
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid()
            };
            CurrentUserAC currentUser = new CurrentUserAC()
            {
                Id = loanApplication.CreatedByUserId
            };
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));

            //Act
            bool isAccess = await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplication.Id);

            //Assert
            Assert.True(isAccess);
        }
        /// <summary>
        /// Check if the user has part to loan.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckUserLoanAccessAsync_PerformCheck_AssertValidateToUserPartToLoanAsync()
        {
            //Arrange
            LoanApplication loanApplication = new LoanApplication();
            CurrentUserAC currentUser = new CurrentUserAC()
            {
                Id = Guid.NewGuid()
            };
            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    EntityId = currentUser.Id
                }
            };
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                               .Returns((Expression<Func<EntityLoanApplicationMapping, bool>> expression) => entityLoanApplicationMappings.AsQueryable().Where(expression).BuildMock().Object);

            //Act
            bool isAccess = await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplication.Id);

            //Assert
            Assert.True(isAccess);
        }
        /// <summary>
        /// Check only if loan consent is given by the loan initiator.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckUserLoanAccessAsync_PerformCheck_LoanInitiatorConcentCheckAsync()
        {
            //Arrange
            LoanApplication loanApplication = new LoanApplication()
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid()
            };
            CurrentUserAC currentUser = new CurrentUserAC()
            {
                Id = Guid.NewGuid()
            };
            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    EntityId = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent{
                    LoanApplicationId = loanApplication.Id,
                    LoanApplication =loanApplication,
                    IsConsentGiven= true,
                    ConsenteeId = Guid.NewGuid()
                }
            };
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                               .Returns((Expression<Func<EntityLoanApplicationMapping, bool>> expression) => entityLoanApplicationMappings.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityLoanApplicationConsent>(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                               .Returns(consents.AsQueryable().BuildMock().Object);

            //Act
            bool isAccess = await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplication.Id);

            //Assert
            Assert.False(isAccess);
        }
        /// <summary>
        /// Check if any of the borrower has relationship with the logged in user
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckUserLoanAccessAsync_PerformCheck_CheckBorrowerRelationShipMappingAsync()
        {
            //Arrange
            LoanApplication loanApplication = new LoanApplication()
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid()
            };
            CurrentUserAC currentUser = new CurrentUserAC()
            {
                Id = Guid.NewGuid()
            };
            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = loanApplication.Id
                }
            };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent
                {
                    LoanApplicationId = loanApplication.Id,
                    LoanApplication =loanApplication,
                    IsConsentGiven= true,
                    ConsenteeId = loanApplication.CreatedByUserId
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    PrimaryEntityId = currentUser.Id,
                    RelativeEntityId = entityLoanApplicationMappings[0].EntityId
                }
            };
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                               .Returns((Expression<Func<EntityLoanApplicationMapping, bool>> expression) => entityLoanApplicationMappings.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                              .Returns((Expression<Func<EntityLoanApplicationConsent, bool>> expression) => consents.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                              .Returns((Expression<Func<EntityRelationshipMapping, bool>> expression) => entityRelationshipMappings.AsQueryable().Where(expression).BuildMock().Object);

            //Act
            bool isAccess = await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplication.Id);

            //Assert
            Assert.True(isAccess);
        }

        /// <summary>
        /// Check if any of the borrower has not relationship with the logged in user.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckUserLoanAccessAsync_PerformCheck_CheckNoBorrowerRelationShipMappingAsync()
        {
            //Arrange
            LoanApplication loanApplication = new LoanApplication()
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid()
            };
            CurrentUserAC currentUser = new CurrentUserAC()
            {
                Id = Guid.NewGuid()
            };
            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = loanApplication.Id
                }
            };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent
                {
                    LoanApplicationId = loanApplication.Id,
                    LoanApplication =loanApplication,
                    IsConsentGiven= true,
                    ConsenteeId = loanApplication.CreatedByUserId
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    PrimaryEntityId = currentUser.Id,
                    RelativeEntityId = Guid.NewGuid()
                }
            };
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(Task.FromResult(loanApplication));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                               .Returns((Expression<Func<EntityLoanApplicationMapping, bool>> expression) => entityLoanApplicationMappings.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                              .Returns((Expression<Func<EntityLoanApplicationConsent, bool>> expression) => consents.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                              .Returns((Expression<Func<EntityRelationshipMapping, bool>> expression) => entityRelationshipMappings.AsQueryable().Where(expression).BuildMock().Object);

            //Act
            bool isAccess = await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplication.Id);

            //Assert
            Assert.False(isAccess);
        }

        #region Check valid loan application

        /// <summary>
        /// Check number of valid loan of the user.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckUserLoanAccessAsync_PerformCheck_CheckCountNoOfValidLoanAsync()
        {
            //Arrange
            CurrentUserAC currentUser = new CurrentUserAC()
            {
                Id = Guid.NewGuid()
            };
            List<ApplicationBasicDetailAC> loanApplicationBasicDetailACs = new List<ApplicationBasicDetailAC>()
            {
                new ApplicationBasicDetailAC(){ Id = Guid.NewGuid() },
                new ApplicationBasicDetailAC(){ Id = Guid.NewGuid() },
                new ApplicationBasicDetailAC(){ Id = Guid.NewGuid() },
                new ApplicationBasicDetailAC(){ Id = Guid.NewGuid() }
            };
            List<LoanApplication> loanApplications = new List<LoanApplication>()
            {
                new LoanApplication()
                {
                    Id = loanApplicationBasicDetailACs[0].Id.Value,
                    CreatedByUserId = currentUser.Id
                },
                new LoanApplication()
                {
                    Id = loanApplicationBasicDetailACs[1].Id.Value,
                    CreatedByUserId = Guid.NewGuid()
                },
                new LoanApplication()
                {
                    Id = loanApplicationBasicDetailACs[2].Id.Value,
                    CreatedByUserId = Guid.NewGuid()
                },
                new LoanApplication()
                {
                    Id = loanApplicationBasicDetailACs[3].Id.Value,
                    CreatedByUserId = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    EntityId = currentUser.Id,
                    LoanApplicationId = loanApplicationBasicDetailACs[1].Id.Value
                },
                new EntityLoanApplicationMapping()
                {
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = loanApplicationBasicDetailACs[3].Id.Value
                }
            };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent
                {
                    LoanApplicationId = loanApplicationBasicDetailACs[2].Id.Value,
                    IsConsentGiven= true,
                    ConsenteeId = Guid.NewGuid()
                },
                new EntityLoanApplicationConsent
                {
                    LoanApplicationId = loanApplicationBasicDetailACs[3].Id.Value,
                    IsConsentGiven= true,
                    ConsenteeId = Guid.NewGuid()
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    PrimaryEntityId = currentUser.Id,
                    RelativeEntityId = entityLoanApplicationMappings[1].EntityId
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                               .Returns((Expression<Func<LoanApplication, bool>> expression) => loanApplications.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                               .Returns((Expression<Func<EntityLoanApplicationMapping, bool>> expression) => entityLoanApplicationMappings.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                              .Returns((Expression<Func<EntityLoanApplicationConsent, bool>> expression) => consents.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                              .Returns((Expression<Func<EntityRelationshipMapping, bool>> expression) => entityRelationshipMappings.AsQueryable().Where(expression).BuildMock().Object);

            //Act
            var actualloanApplicationBasicDetailACs = await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplicationBasicDetailACs);

            //Assert
            Assert.Equal(2, actualloanApplicationBasicDetailACs.Count);
        }
        /// <summary>
        /// Check number of valid loan of the user.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckUserLoanAccessAsync_PerformCheck_CheckCountOfIsNotClientUserAsync()
        {
            //Arrange
            CurrentUserAC currentUser = new CurrentUserAC()
            {
                Id = Guid.NewGuid()
            };
            List<ApplicationBasicDetailAC> loanApplicationBasicDetailACs = new List<ApplicationBasicDetailAC>()
            {
                new ApplicationBasicDetailAC(){ Id = Guid.NewGuid() },
                new ApplicationBasicDetailAC(){ Id = Guid.NewGuid() },
                new ApplicationBasicDetailAC(){ Id = Guid.NewGuid() },
                new ApplicationBasicDetailAC(){ Id = Guid.NewGuid() }
            };
            List<LoanApplication> loanApplications = new List<LoanApplication>()
            {
                new LoanApplication()
                {
                    Id = loanApplicationBasicDetailACs[0].Id.Value,
                    CreatedByUserId = currentUser.Id
                },
                new LoanApplication()
                {
                    Id = loanApplicationBasicDetailACs[1].Id.Value,
                    CreatedByUserId = Guid.NewGuid()
                },
                new LoanApplication()
                {
                    Id = loanApplicationBasicDetailACs[2].Id.Value,
                    CreatedByUserId = Guid.NewGuid()
                },
                new LoanApplication()
                {
                    Id = loanApplicationBasicDetailACs[3].Id.Value,
                    CreatedByUserId = Guid.NewGuid()
                }
            };
            List<EntityLoanApplicationMapping> entityLoanApplicationMappings = new List<EntityLoanApplicationMapping>()
            {
                new EntityLoanApplicationMapping()
                {
                    EntityId = currentUser.Id,
                    LoanApplicationId = loanApplicationBasicDetailACs[1].Id.Value
                },
                new EntityLoanApplicationMapping()
                {
                    EntityId = Guid.NewGuid(),
                    LoanApplicationId = loanApplicationBasicDetailACs[3].Id.Value
                }
            };
            List<EntityLoanApplicationConsent> consents = new List<EntityLoanApplicationConsent>()
            {
                new EntityLoanApplicationConsent
                {
                    LoanApplicationId = loanApplicationBasicDetailACs[2].Id.Value,
                    IsConsentGiven= true,
                    ConsenteeId = loanApplications[0].CreatedByUserId
                },
                new EntityLoanApplicationConsent
                {
                    LoanApplicationId = loanApplicationBasicDetailACs[3].Id.Value,
                    IsConsentGiven= true,
                    ConsenteeId = loanApplications[3].CreatedByUserId
                }
            };
            List<EntityRelationshipMapping> entityRelationshipMappings = new List<EntityRelationshipMapping>()
            {
                new EntityRelationshipMapping()
                {
                    PrimaryEntityId = currentUser.Id,
                    RelativeEntityId = entityLoanApplicationMappings[1].EntityId
                }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<LoanApplication, bool>>>()))
                               .Returns((Expression<Func<LoanApplication, bool>> expression) => loanApplications.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationMapping, bool>>>()))
                               .Returns((Expression<Func<EntityLoanApplicationMapping, bool>> expression) => entityLoanApplicationMappings.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityLoanApplicationConsent, bool>>>()))
                              .Returns((Expression<Func<EntityLoanApplicationConsent, bool>> expression) => consents.AsQueryable().Where(expression).BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                              .Returns((Expression<Func<EntityRelationshipMapping, bool>> expression) => entityRelationshipMappings.AsQueryable().Where(expression).BuildMock().Object);

            //Act
            var actualloanApplicationBasicDetailACs = await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplicationBasicDetailACs, false);

            //Assert
            Assert.Equal(loanApplicationBasicDetailACs.Count, actualloanApplicationBasicDetailACs.Count);
        }
        #endregion
        #endregion

        #region ConvertUtcDateToLocalDate Method
        /// <summary>
        /// Check if utc date is properly converted to local date
        /// </summary>
        [Fact]
        public void ConvertUtcDateToLocalDate_PerformCheck_DateConversion()
        {
            //Arrange

            //Act
            var date = _globalRepository.ConvertUtcDateToLocalDate(DateTime.UtcNow);

            //Assert
            Assert.Equal(date.ToShortDateString(), DateTime.Now.ToShortDateString());
            Assert.Equal(date.ToShortTimeString(), DateTime.Now.ToShortTimeString());
        }

        #endregion

        /// <summary>
        /// Check if  method correctly converts xml to json
        /// </summary>
        [Fact]
        public void ConvertXmlToJson_ValidXml_ReturnsValidJson()
        {
            //Arrange
            string xml = "<xyz>hi</xyz>";
            JObject expected = JObject.Parse("{'xyz':'hi'}");

            //Act
            JObject actual = _globalRepository.ConvertXmlToJson(xml);

            //Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Method to verify if consents don't exist in database then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetConsentStatementsAsync_ConsentsNotExistInDatabase_ThrowsDataNotFoundException()
        {
            //Arrange
            List<Consent> consents = new List<Consent>();
            _dataRepositoryMock.Setup(x => x.GetAll<Consent>()).Returns(consents.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.GetConsentStatementsAsync());
        }

        /// <summary>
        /// Method to verify if consents exist in database then it returns a list of all the consents.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetConsentStatementsAsync_ConsentsExistInDatabase_ReturnsConsentList()
        {
            //Arrange
            List<Consent> consents = new List<Consent>
            {
                new Consent { Id = Guid.NewGuid(), ConsentText = "Consent1" },
                new Consent { Id = Guid.NewGuid(), ConsentText = "Consent2" }
            };
            _dataRepositoryMock.Setup(x => x.GetAll<Consent>()).Returns(consents.AsQueryable().BuildMock().Object);

            //Act
            List<ConsentStatementAC> actualList = await _globalRepository.GetConsentStatementsAsync();

            //Assert
            Assert.Equal(consents.Count, actualList.Count);
        }

        /// <summary>
        /// Method to verify if banks don't exist in database then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAllBanksAsync_BanksNotExistInDatabase_ThrowsDataNotFoundException()
        {
            //Arrange
            List<Bank> banks = new List<Bank>();
            _dataRepositoryMock.Setup(x => x.GetAll<Bank>()).Returns(banks.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.GetAllBanksAsync());
        }

        /// <summary>
        /// Method to verify if consents exist in database then it returns a list of all the consents.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAllBanksAsync_BanksExistInDatabase_ReturnsBankList()
        {
            //Arrange
            List<Bank> banks = new List<Bank>
            {
                new Bank { Id = Guid.NewGuid(), Name = "Bank1", SWIFTCode = "BNK0001251" },
                new Bank { Id = Guid.NewGuid(), Name = "Bank2", SWIFTCode = "BNK0001252" }
            };
            _dataRepositoryMock.Setup(x => x.GetAll<Bank>()).Returns(banks.AsQueryable().BuildMock().Object);

            //Act
            List<ApplicationClass.Others.BankAC> actualList = await _globalRepository.GetAllBanksAsync();

            //Assert
            Assert.Equal(banks.Count, actualList.Count);
        }

        /// <summary>
        /// Method to verify if loan purposes don't exist in database then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetLoanPurposeListAsync_LoanPurposesNotExistInDatabase_ThrowsDataNotFoundException()
        {
            //Arrange
            List<LoanPurpose> loanPurposes = new List<LoanPurpose>();
            _dataRepositoryMock.Setup(x => x.GetAll<LoanPurpose>()).Returns(loanPurposes.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.GetLoanPurposeListAsync());
        }

        /// <summary>
        /// Method to verify if loan purposes exist in database then it returns a list of all the purposes.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetLoanPurposeListAsync_LoanPurposesExistInDatabase_ReturnsPurposeList()
        {
            //Arrange
            List<Guid> purposeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            List<LoanPurpose> loanPurposes = new List<LoanPurpose>
            {
                new LoanPurpose { Id = purposeIds.First(), Name = "Purpose1", Order = 1, IsEnabled = true,
                    LoanPurposeRangeTypeMappings = new List<LoanPurposeRangeTypeMapping>
                    {
                        new LoanPurposeRangeTypeMapping
                        {
                            LoanPurposeId = purposeIds.First(),
                            Minimum = 5000,
                            Maximum = 100000,
                            StepperAmount = 1000,
                            LoanRangeType = new LoanRangeType
                            {
                                Name = StringConstant.LoanAmount
                            }
                        },
                        new LoanPurposeRangeTypeMapping
                        {
                            LoanPurposeId = purposeIds.Last(),
                            Minimum = 1,
                            Maximum = 5,
                            StepperAmount = (decimal)(0.5),
                            LoanRangeType = new LoanRangeType
                            {
                                Name = StringConstant.Lifecycle
                            }
                        }
                    }
                },
                new LoanPurpose { Id = Guid.NewGuid(), Name = "Purpose2", Order = 2, IsEnabled = false }
            };
            _dataRepositoryMock.Setup(x => x.GetAll<LoanPurpose>()).Returns(loanPurposes.AsQueryable().BuildMock().Object);

            //Act
            List<ApplicationClass.Others.LoanPurposeAC> actualList = await _globalRepository.GetLoanPurposeListAsync();

            //Assert
            Assert.Equal(loanPurposes.Count, actualList.Count);
        }

        /// <summary>
        /// Method to verify if sections don't exist in database then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAllConfigurationsAsync_SectionsNotExist_ThrowsDataNotFoundException()
        {
            //Arrange
            List<Section> sections = new List<Section>();
            _dataRepositoryMock.Setup(x => x.Fetch<Section>(It.IsAny<Expression<Func<Section, bool>>>())).Returns(sections.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.GetAllConfigurationsAsync());
        }

        /// <summary>
        /// Method to verify if sections exist in database then it returns a list of all the required configurations.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAllConfigurationsAsync_SectionsExist_ReturnsConfigurations()
        {
            //Arrange
            List<Section> sections = new List<Section>
            {
                new Section { Id = Guid.NewGuid(), Name = "Loan Needs", Order = 1, IsEnabled = true },
                new Section { Id = Guid.NewGuid(), Name = "Company Info", Order = 2, IsEnabled = false }
            };
            List<IntegratedServiceConfiguration> services = new List<IntegratedServiceConfiguration>();
            var mock = new Mock<IConfigurationSection>();
            _configuration.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mock.Object);

            _dataRepositoryMock.Setup(x => x.Fetch<Section>(It.IsAny<Expression<Func<Section, bool>>>())).Returns(sections.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<IntegratedServiceConfiguration>(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>())).Returns(services.AsQueryable().BuildMock().Object);

            //Act
            ConfigurationAC configuration = await _globalRepository.GetAllConfigurationsAsync();

            //Assert
            Assert.Equal(sections.Count, configuration.Sections.Count);
            Assert.Empty(configuration.ThirdPartyServices);
            Assert.Empty(configuration.AppSettings);
        }

        #region Audit log.
        /// <summary>
        /// Check the user object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void GetAuditLogForCustomFields_CheckUserObject_VerifiedAllFields()
        {
            //Arrange
            CurrentUserAC user = new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                IsBankUser = false,
                IpAddress = "100.20.1.168"
            };
            ResourceType logBlockName = ResourceType.Loan;
            Guid loanId = Guid.NewGuid();

            //Act
            var auditLog = _globalRepository.GetAuditLogForCustomFields(user, logBlockName, loanId);

            //Assert
            Assert.NotNull(auditLog.CreatedByUserId);
            Assert.Equal(logBlockName, auditLog.LogBlockName);
            Assert.Equal(loanId, auditLog.LogBlockNameId);
            Assert.Equal(user.IpAddress, auditLog.IpAddress);
        }
        /// <summary>
        /// Check the bank user object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void GetAuditLogForCustomFields_CheckBankUserObject_VerifiedAllFields()
        {
            //Arrange
            CurrentUserAC user = new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                IsBankUser = true,
                IpAddress = "100.20.1.168"
            };
            ResourceType logBlockName = ResourceType.Loan;
            Guid loanId = Guid.NewGuid();

            //Act
            var auditLog = _globalRepository.GetAuditLogForCustomFields(user, logBlockName, loanId);

            //Assert
            Assert.NotNull(auditLog.CreatedByBankUserId);
            Assert.Equal(logBlockName, auditLog.LogBlockName);
            Assert.Equal(loanId, auditLog.LogBlockNameId);
            Assert.Equal(user.IpAddress, auditLog.IpAddress);
        }

        /// <summary>
        /// Check InvalidParameterException exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogsByLogBlockNameIdAsync_Exception_RaiseInvalidParameterException()
        {
            //Arrange
            CurrentUserAC user = new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                IsBankUser = false
            };
            Guid loanId = Guid.Empty;
            AuditLogFilterAC auditLogFilter = new AuditLogFilterAC() { LogBlockNameId = loanId };
            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _globalRepository.GetAuditLogsByLogBlockNameIdAsync(user, auditLogFilter));

        }
        /// <summary>
        /// Check the audit log objects.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogsByLogBlockNameIdAsync_ExistAuditLog_CheckObjectCount()
        {
            //Arrange
            CurrentUserAC user = new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                IsBankUser = true
            };
            Guid logBlockNameId = Guid.NewGuid();
            AuditLogFilterAC auditLogFilter = new AuditLogFilterAC() { LogBlockNameId = logBlockNameId };
            string loanTableName = nameof(LoanApplication);
            List<AuditLog> auditLogs = new List<AuditLog>() {
            new AuditLog{ Id = Guid.NewGuid(), TableName = loanTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(loanTableName, StringConstant.AuditLogInsertActionName), Action = StringConstant.AuditLogInsertActionName },
            new AuditLog{ Id = Guid.NewGuid(), TableName = loanTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(loanTableName, StringConstant.AuditLogInsertActionName), Action = StringConstant.AuditLogInsertActionName },
            new AuditLog{ Id = Guid.NewGuid(), TableName = loanTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(loanTableName), Action = StringConstant.AuditLogUpdateActionName },
            new AuditLog{ Id = Guid.NewGuid(), TableName = loanTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(loanTableName, StringConstant.AuditLogInsertActionName), Action = StringConstant.AuditLogDeleteActionName }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);
            var entityRelationshipMappings = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act 
            var result = await _globalRepository.GetAuditLogsByLogBlockNameIdAsync(user, auditLogFilter);

            //Assert
            Assert.Equal(4, result[0].AuditLogs.Count);
        }
        /// <summary>
        /// Check the following scenario for user.
        /// 1. Remove +1 in phone number
        /// 2. Update Residence status name from the enum value.
        /// 3. Update DOB with the default date format.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogsByLogBlockNameIdAsync_ValidUserDetails_AssertEqualUserDetails()
        {
            //Arrange
            CurrentUserAC user = new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                IsBankUser = true
            };
            Guid logBlockNameId = Guid.NewGuid();
            AuditLogFilterAC auditLogFilter = new AuditLogFilterAC() { LogBlockNameId = logBlockNameId };
            List<AuditLog> auditLogs = new List<AuditLog>() {
            new AuditLog{ Id = Guid.NewGuid(), TableName = nameof(User), LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(nameof(User)) , Action = StringConstant.AuditLogInsertActionName }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);
            var entityRelationshipMappings = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act 
            var result = await _globalRepository.GetAuditLogsByLogBlockNameIdAsync(user, auditLogFilter);
            var auditLogFields = result[0].AuditLogs[0].AuditLogFields;

            //Assert
            Assert.True(auditLogFields.First(s => s.ColumnName.Equals(nameof(User.Phone))).NewValue.ToString().IndexOf('+') == -1);
            Assert.True(auditLogFields.First(s => s.ColumnName.Equals(nameof(User.ResidencyStatus))).NewValue.ToString().Equals(ResidencyStatus.USPermanentResident.ToString()));
            Assert.True(auditLogFields.First(s => s.ColumnName.Equals(nameof(User.DOB))).NewValue.ToString().Equals("06-01-1987"));
        }

        /// <summary>
        /// Check the Loan application enum status.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogsByLogBlockNameIdAsync_CheckLoanApplicationStatus_AssertEqualLoanStatus()
        {
            //Arrange
            CurrentUserAC user = new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                IsBankUser = true
            };
            Guid logBlockNameId = Guid.NewGuid();
            AuditLogFilterAC auditLogFilter = new AuditLogFilterAC() { LogBlockNameId = logBlockNameId };
            string loanApplicationTableName = nameof(LoanApplication);
            List<AuditLog> auditLogs = new List<AuditLog>() {
            new AuditLog{ Id = Guid.NewGuid(), TableName = loanApplicationTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(loanApplicationTableName) , Action = StringConstant.AuditLogInsertActionName }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);
            var entityRelationshipMappings = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act 
            var result = await _globalRepository.GetAuditLogsByLogBlockNameIdAsync(user, auditLogFilter);
            var auditLogFields = result[0].AuditLogs[0].AuditLogFields;

            //Assert
            Assert.True(auditLogFields.First(s => s.ColumnName.Equals(nameof(LoanApplication.Status))).NewValue.ToString().Equals(LoanApplicationStatusType.Draft.ToString()));
        }

        /// <summary>
        /// Check the following scenario for Address.
        /// 1. Create AddressId field in the field list when change the address.
        /// 2. Concat Street line with "PrimaryNumber StreetLine StreetSuffix" fields. 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogsByLogBlockNameIdAsync_ValidAddressDetails_AssertAddressVerification()
        {
            //Arrange
            CurrentUserAC user = new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                IsBankUser = true
            };
            Guid logBlockNameId = Guid.NewGuid();
            AuditLogFilterAC auditLogFilter = new AuditLogFilterAC() { LogBlockNameId = logBlockNameId };
            string addressTableName = nameof(Address);
            List<AuditLog> auditLogs = new List<AuditLog>() {
            new AuditLog{ Id = Guid.NewGuid(), TableName = addressTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(addressTableName,StringConstant.AuditLogInsertActionName) , Action = StringConstant.AuditLogInsertActionName },
            new AuditLog{ Id = Guid.NewGuid(), TableName = addressTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(addressTableName) , Action = StringConstant.AuditLogUpdateActionName }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);
            var entityRelationshipMappings = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act 
            var auditDateWiseLogs = await _globalRepository.GetAuditLogsByLogBlockNameIdAsync(user, auditLogFilter);
            var resultAuditLogs = auditDateWiseLogs[0].AuditLogs;
            var insertResultAuditLog = resultAuditLogs.First();
            var updateResultAuditLog = resultAuditLogs[1];

            //Assert
            Assert.True(insertResultAuditLog.AuditLogFields.First(s => s.ColumnName.Equals(nameof(Address.StreetLine))).NewValue.Equals("2001 Phillips Cir"));
            Assert.NotNull(updateResultAuditLog.AuditLogFields.First(s => s.ColumnName.Equals("AddressId")));
        }

        /// <summary>
        /// Check update entityId column name to new name for EntityLoanApplicationMapping table.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogsByLogBlockNameIdAsync_CheckEntityLoanApplicationMappingUpdatedId_AssertEqualEntityId()
        {
            //Arrange
            CurrentUserAC user = new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                IsBankUser = true
            };
            Guid logBlockNameId = Guid.NewGuid();
            AuditLogFilterAC auditLogFilter = new AuditLogFilterAC() { LogBlockNameId = logBlockNameId };
            string entityLoanApplicationMappingTableName = nameof(EntityLoanApplicationMapping);
            List<AuditLog> auditLogs = new List<AuditLog>() {
            new AuditLog{ Id = Guid.NewGuid(), TableName = entityLoanApplicationMappingTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(entityLoanApplicationMappingTableName, StringConstant.AuditLogInsertActionName), Action = StringConstant.AuditLogInsertActionName }
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);
            string expectColumnName = $"{entityLoanApplicationMappingTableName}_{nameof(EntityLoanApplicationMapping.EntityId)}";
            var entityRelationshipMappings = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);
            //Act 
            var auditDateWiseLogs = await _globalRepository.GetAuditLogsByLogBlockNameIdAsync(user, auditLogFilter);
            var resultAuditLogs = auditDateWiseLogs[0].AuditLogs;
            var insertResultAuditLog = resultAuditLogs.First();

            //Assert
            Assert.Contains(insertResultAuditLog.AuditLogFields, s => s.ColumnName.Equals(expectColumnName));
        }
        /// <summary>
        /// Check consent logs.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogsByLogBlockNameIdAsync_ValidMergeConsent_AssertConsentCount()
        {
            //Arrange
            CurrentUserAC user = new CurrentUserAC
            {
                Id = Guid.NewGuid(),
                IsBankUser = true
            };
            DateTime createdDate = DateTime.Now;
            Guid logBlockNameId = Guid.NewGuid(), consentCreatedUserId = Guid.NewGuid();
            AuditLogFilterAC auditLogFilter = new AuditLogFilterAC() { LogBlockNameId = logBlockNameId };
            string entityLoanApplicationConsentTableName = nameof(EntityLoanApplicationConsent);
            List<AuditLog> auditLogs = new List<AuditLog>() {
            new AuditLog{ Id = Guid.NewGuid(), TableName = entityLoanApplicationConsentTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(entityLoanApplicationConsentTableName) , Action = StringConstant.AuditLogInsertActionName , CreatedByUserId = consentCreatedUserId ,CreatedOn = createdDate},
            new AuditLog{ Id = Guid.NewGuid(), TableName = entityLoanApplicationConsentTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(entityLoanApplicationConsentTableName) , Action = StringConstant.AuditLogInsertActionName , CreatedByUserId = consentCreatedUserId ,CreatedOn = createdDate},
            new AuditLog{ Id = Guid.NewGuid(), TableName = entityLoanApplicationConsentTableName, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(entityLoanApplicationConsentTableName) , Action = StringConstant.AuditLogInsertActionName , CreatedByUserId = consentCreatedUserId ,CreatedOn = createdDate},
            };
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);
            var entityRelationshipMappings = new List<EntityRelationshipMapping>();
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityRelationshipMapping, bool>>>()))
                .Returns(entityRelationshipMappings.AsQueryable().BuildMock().Object);

            //Act 
            var auditDateWiseLogs = await _globalRepository.GetAuditLogsByLogBlockNameIdAsync(user, auditLogFilter);
            var resultAuditLogs = auditDateWiseLogs[0].AuditLogs;

            //Assert
            Assert.Single(resultAuditLogs);
            Assert.Equal(3, resultAuditLogs.First().AuditLogFields.First().Ids.Count);
        }
        /// <summary>
        /// Verify multiple consent details.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogByPkIdAsync_CheckConsentIds_AssertIdDetails()
        {
            //Arrange
            DateTime createdDate = DateTime.Now;
            Guid logBlockNameId = Guid.NewGuid(), consentCreatedUserId = Guid.NewGuid(), firstPkId = Guid.NewGuid(), secondPkId = Guid.NewGuid(), thirdPkId = Guid.NewGuid();
            string consentTableName = nameof(Consent);
            List<AuditLog> auditLogs = new List<AuditLog>() {
            new AuditLog{ Id = Guid.NewGuid(), TableName = consentTableName, TablePk=firstPkId, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(consentTableName) , Action = StringConstant.AuditLogInsertActionName , CreatedByUserId = consentCreatedUserId ,CreatedOn = createdDate},
            new AuditLog{ Id = Guid.NewGuid(), TableName = consentTableName, TablePk=secondPkId, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(consentTableName) , Action = StringConstant.AuditLogInsertActionName , CreatedByUserId = consentCreatedUserId ,CreatedOn = createdDate},
            new AuditLog{ Id = Guid.NewGuid(), TableName = consentTableName, TablePk=thirdPkId, LogBlockNameId = logBlockNameId, AuditJson = GetAuditLogJson(consentTableName) , Action = StringConstant.AuditLogInsertActionName , CreatedByUserId = consentCreatedUserId ,CreatedOn = createdDate},
            };
            AuditLogFieldAC auditLogField = new AuditLogFieldAC
            {
                ColumnName = "Consentd",
                Ids = new List<object>() { firstPkId, secondPkId, thirdPkId },
                NewValue = Guid.NewGuid()
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);

            //Act 
            var auditLogFields = await _globalRepository.GetAuditLogByPkIdAsync(auditLogField);

            //Assert
            Assert.Equal(3, auditLogFields.Count);
        }
        /// <summary>
        /// Verify user details.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogByPkIdAsync_FetchUserDetail_AsserSingleUser()
        {
            //Arrange
            Guid firstPkId = Guid.NewGuid();
            string userTableName = nameof(User);
            List<AuditLog> auditLogs = new List<AuditLog>() {
                new AuditLog{ Id = Guid.NewGuid(), TableName = userTableName, TablePk=firstPkId, AuditJson = GetAuditLogJson(userTableName) , Action = StringConstant.AuditLogInsertActionName,CreatedOn =DateTime.Now.AddHours(-1) }
            };
            AuditLogFieldAC auditLogField = new AuditLogFieldAC
            {
                ColumnName = "UserId",
                NewValue = firstPkId,
                LogDate = DateTime.Now
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);

            //Act 
            var auditLogFields = await _globalRepository.GetAuditLogByPkIdAsync(auditLogField);

            //Assert
            Assert.Equal(7, auditLogFields.Count);
        }
        /// <summary>
        /// Verify Address for audit log id.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogByPkIdAsync_FetchDetailForAddressAuditLogId_AssertEqualAddressStreetline()
        {
            //Arrange
            Guid auditLogId = Guid.NewGuid();
            List<AuditLog> auditLogs = new List<AuditLog>() {
                new AuditLog{ Id = auditLogId, TableName=nameof(Address), AuditJson = GetAuditLogJson(nameof(Address),StringConstant.AuditLogInsertActionName) , Action = StringConstant.AuditLogInsertActionName },
            };
            AuditLogFieldAC auditLogField = new AuditLogFieldAC
            {
                ColumnName = "AddressId",
                NewValue = auditLogId,
                LogDate = DateTime.Now
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);

            //Act 
            var auditLogFields = await _globalRepository.GetAuditLogByPkIdAsync(auditLogField);

            //Assert
            Assert.True(auditLogFields.First(s => s.ColumnName.Equals(nameof(Address.StreetLine))).NewValue.Equals("2001 Phillips Cir"));
        }
        /// <summary>
        /// Verify the company structure name in the CompanyStructureId.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogByPkIdAsync_CheckCompanyStructureName_AssertEqualCompanyStructure()
        {
            //Arrange
            Guid pkId = Guid.NewGuid();
            AuditLogFieldAC auditLogField = new AuditLogFieldAC
            {
                ColumnName = "CompanyId",
                NewValue = pkId,
                LogDate = DateTime.Now
            };

            string companyTableName = nameof(Company);
            List<AuditLog> auditLogs = new List<AuditLog>() {
            new AuditLog{ Id = Guid.NewGuid(), TableName = companyTableName, TablePk = pkId, AuditJson = GetAuditLogJson(companyTableName) , Action = StringConstant.AuditLogInsertActionName }
            };

            string companyStructureTableName = nameof(CompanyStructure);
            List<AuditLog> companyStructureAuditLogs = new List<AuditLog>() {
            new AuditLog{ Id = Guid.NewGuid(), TableName = companyStructureTableName, TablePk = Guid.Parse("4e25bc9d-660f-45b8-85da-26a3e469be62"), AuditJson = GetAuditLogJson(companyStructureTableName) , Action = StringConstant.AuditLogInsertActionName }
            };
            _dataRepositoryMock.SetupSequence(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object)
                .Returns(companyStructureAuditLogs.AsQueryable().BuildMock().Object);

            //Act 
            var auditLogFields = await _globalRepository.GetAuditLogByPkIdAsync(auditLogField);

            //Assert
            Assert.True(auditLogFields.First(s => s.ColumnName.Equals(nameof(Company.CompanyStructureId))).NewValue.Equals("Proprietorship"));
        }

        /// <summary>
        /// DataNotFoundException exception when data is not exist in the database.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogByPkIdAsync_DataNotFoundException_AssertException()
        {
            //Arrange
            List<AuditLog> auditLogs = new List<AuditLog>();
            AuditLogFieldAC auditLogField = new AuditLogFieldAC
            {
                ColumnName = "AddressId",
                NewValue = Guid.NewGuid(),
                LogDate = DateTime.Now
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<AuditLog, bool>>>()))
                .Returns(auditLogs.AsQueryable().BuildMock().Object);

            //Act 
            //Assert            
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _globalRepository.GetAuditLogByPkIdAsync(auditLogField));
        }
        /// <summary>
        /// InvalidParameterException exception when AuditLogFieldAC has no new value and old value.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuditLogByPkIdAsync_InvalidParameterException_AssertException()
        {
            //Arrange
            AuditLogFieldAC auditLogField = new AuditLogFieldAC
            {
                ColumnName = "AddressId",
                LogDate = DateTime.Now
            };

            //Act 
            //Assert            
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _globalRepository.GetAuditLogByPkIdAsync(auditLogField));
        }
        #endregion

        /// <summary>
        /// Method to check if proper interest is being returned for the given credit score.
        /// </summary>
        [Theory]
        [InlineData(StringConstant.ExcellentCreditScore)]
        [InlineData(StringConstant.GoodCreditScore)]
        [InlineData(StringConstant.FairCreditScore)]
        [InlineData(StringConstant.AverageCreditScore)]
        [InlineData(StringConstant.PoorCreditScore)]
        public void GetInterestRateForGivenSelfDeclaredCreditScore_ValidCreditScoreWithInterestValueAvailableInConfiguration_ReturnsRespectiveInterestRate(string creditScore)
        {
            //Arrange
            decimal? interestRate = null;
            if (creditScore.Equals(StringConstant.ExcellentCreditScore)) { interestRate = (decimal)7.99; }
            else if (creditScore.Equals(StringConstant.GoodCreditScore)) { interestRate = (decimal)9.99; }
            else if (creditScore.Equals(StringConstant.FairCreditScore))
            { interestRate = (decimal)11.99; }
            else if (creditScore.Equals(StringConstant.AverageCreditScore))
            { interestRate = (decimal)13.99; }
            else if (creditScore.Equals(StringConstant.PoorCreditScore))
            { interestRate = (decimal)19.99; }

            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns(interestRate.ToString());

            //Act
            decimal? actual = _globalRepository.GetInterestRateForGivenSelfDeclaredCreditScore(creditScore);

            //Assert
            Assert.Equal(interestRate, actual);
        }

        /// <summary>
        /// Method to check if null credit score is given then it returns null.
        /// </summary>
        [Fact]
        public void GetInterestRateForGivenSelfDeclaredCreditScore_CreditScoreIsNull_ReturnsNull()
        {
            //Arrange

            //Act
            decimal? actual = _globalRepository.GetInterestRateForGivenSelfDeclaredCreditScore(null);

            //Assert
            Assert.Null(actual);
        }

        /// <summary>
        /// Method to check if the credit score given is different than the seeded ones then it throws an exception.
        /// </summary>
        [Fact]
        public void GetInterestRateForGivenSelfDeclaredCreditScore_CreditScoreIsDifferentThanSeededData_ThrowsConfigurationNotFoundException()
        {
            //Arrange
            string creditScore = "180< & <250";

            //Act

            //Assert
            Assert.Throws<ConfigurationNotFoundException>(() => _globalRepository.GetInterestRateForGivenSelfDeclaredCreditScore(creditScore));
        }

        /// <summary>
        /// Method to check if interest rate doesn't exist in configurations for the given credit score then it throws an exception.
        /// </summary>
        [Fact]
        public void GetInterestRateForGivenSelfDeclaredCreditScore_InterestRateNotExistInConfigurations_ThrowsConfigurationNotFoundException()
        {
            //Arrange
            decimal? interestRate = null;
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).Value).Returns(interestRate.ToString());

            //Act

            //Assert
            Assert.Throws<ConfigurationNotFoundException>(() => _globalRepository.GetInterestRateForGivenSelfDeclaredCreditScore(StringConstant.GoodCreditScore));
        }

        /// <summary>
        /// Method to check if 1 is pass as argument then it returns January.
        /// </summary>
        [Fact]
        public void GetMonthNameFromMonthNumber_ArgumentAs1_VerifyReturnJanuary()
        {
            //Arrange
            var expected = StringConstant.January;
            //Act
            var actual = _globalRepository.GetMonthNameFromMonthNumber(1);
            //Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Method to check if invalid number like 20 is pass as argument then it returns Invalid Month Number.
        /// </summary>
        [Fact]
        public void GetMonthNameFromMonthNumber_ArgumentAs20_VerifyReturnInvalidMonthNumber()
        {
            //Arrange
            var expected = StringConstant.InvalidMonthNumber;
            //Act
            var actual = _globalRepository.GetMonthNameFromMonthNumber(20);
            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPathForKeyNameBucketForAdditionalDocument_AllParametersProvided_ReturnsValidFilePath()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            var additionalDocument = new AdditionalDocumentAC
            {
                Id = Guid.NewGuid(),
                Document = new DocumentAC
                {
                    Id = Guid.NewGuid(),
                    DownloadPath = "abc.download.com/downloadPath",
                    Name = "abc.pdf",
                    Path = "abc.download.com/path"
                },
                DocumentType = new AdditionalDocumentTypeAC
                {
                    Id = Guid.NewGuid(),
                    Type = "Certificate",
                    DocumentTypeFor = ResourceType.User
                }
            };
            string fileName = $"{string.Join('.', additionalDocument.Document.Name.Split('.').SkipLast(1).ToList())}_{Guid.NewGuid()}.{additionalDocument.Document.Name.Split('.').Last()}";
            string expectedFilePath = $"{entityId}/{StringConstant.AdditionalDocuments}/{additionalDocument.DocumentType.Type}/{additionalDocument.DocumentType.DocumentTypeFor}/{fileName}";

            //Act
            var actual = _globalRepository.GetPathForKeyNameBucketForAdditionalDocument(entityId, additionalDocument);

            //Assert
            Assert.NotNull(actual);
            Assert.NotEqual(expectedFilePath, actual);
            Assert.Equal(expectedFilePath.Split('_').SkipLast(1), actual.Split('_').SkipLast(1));
        }
        #endregion
    }
}
