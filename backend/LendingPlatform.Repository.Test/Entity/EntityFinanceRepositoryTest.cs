using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using SmartyStreets.USStreetApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
namespace LendingPlatform.Repository.Test.Entity
{
    [Collection("Register Dependency")]
    public class EntityFinanceRepositoryTest : BaseTest
    {

        #region Private variables
        private readonly IEntityFinanceRepository _entityFinanceRepository;
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly CurrentUserAC _currentUserAC;
        private readonly List<FinancialStatement> _financialStatementList;
        private readonly List<IntegratedServiceConfiguration> _integratedServiceConfigurationList;
        private readonly EntityAC _entity;
        private readonly List<EntityFinance> _entityFinanceList;
        private readonly ThirdPartyServiceCallbackDataAC _thirdPartyCallbackData;
        private readonly List<CompanyFinanceAC> _financeAcList;
        private readonly IMapper _mapper;
        private readonly Mock<IRulesUtility> _rulesUtilityMock;
        private readonly Mock<IQuickbooksUtility> _quickbooksUtilityMock;
        private readonly Mock<IXeroUtility> _xeroUtilityMock;
        private readonly Mock<ISmartyStreetsUtility> _smartyStreetUtilityMock;
        #endregion

        #region Constructor
        public EntityFinanceRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _entityFinanceRepository = bootstrap.ServiceProvider.GetService<IEntityFinanceRepository>();
            _configuration = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _rulesUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IRulesUtility>>();
            _quickbooksUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IQuickbooksUtility>>();
            _xeroUtilityMock = bootstrap.ServiceProvider.GetService<Mock<IXeroUtility>>();
            _quickbooksUtilityMock.Reset();
            _xeroUtilityMock.Reset();
            _rulesUtilityMock.Reset();
            _dataRepositoryMock.Reset();
            _globalRepositoryMock.Reset();
            _entity = GetEntity();
            _currentUserAC = FetchLoggedInUserAC();
            _financialStatementList = FetchFinancialStatements();
            _integratedServiceConfigurationList = FetchIntegratedServiceConfiguration();
            _entityFinanceList = FetchEntityFinance();
            _thirdPartyCallbackData = FetchThirdPartyCallbackData();
            _mapper = bootstrap.ServiceProvider.GetService<IMapper>();
            _financeAcList = FetchListOfFinanceAC();
            _smartyStreetUtilityMock = bootstrap.ServiceProvider.GetService<Mock<ISmartyStreetsUtility>>();
            _smartyStreetUtilityMock.Reset();
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Get EntityAC object
        /// </summary>
        /// <returns></returns>
        private EntityAC GetEntity()
        {
            return new EntityAC
            {
                Id = Guid.NewGuid(),
                RelationMapping = null,
                Address = new AddressAC
                {
                    Id = Guid.NewGuid(),
                    City = "XYZ",
                    IntegratedServiceConfigurationId = Guid.NewGuid(),
                    PrimaryNumber = "86",
                    SecondaryNumber = "21",
                    SecondaryDesignator = "ASD",
                    StateAbbreviation = "GH",
                    StreetLine = "LMN BV",
                    StreetSuffix = "HJ",
                    ZipCode = "23568"
                },
                Company = new CompanyAC
                {
                    CompanyStructure = new CompanyStructureAC
                    {
                        Id = Guid.NewGuid(),
                        Structure = StringConstant.Proprietorship,
                        Order = 1
                    },
                    CompanySize = new CompanySizeAC
                    {
                        Id = Guid.NewGuid(),
                        Size = StringConstant.ZeroToTen,
                        Order = 1
                    },
                    BusinessAge = new BusinessAgeAC
                    {
                        Id = Guid.NewGuid(),
                        Age = StringConstant.SixMonthToOneYear,
                        Order = 1
                    },
                    IndustryExperience = new IndustryExperienceAC
                    {
                        Id = Guid.NewGuid(),
                        Experience = StringConstant.IndustryExperienceZeroToFiveYears,
                        Order = 1
                    },
                    IndustryType = new IndustryTypeAC
                    {
                        Id = Guid.NewGuid(),
                        IndustryCode = "1234",
                        IndustryType = "Forstry"
                    },
                    CIN = "123456789",
                    CompanyFiscalYearStartMonth = 1,
                    Name = "Google",
                    CompanyRegisteredState = "Texas"
                },
                User = new UserAC
                {
                    DOB = DateTime.Now,
                    Email = "arjun@promactinfo.com",
                    FirstName = "Arjunsinh",
                    MiddleName = "",
                    LastName = "Jadeja",
                    HasAnyJudgementsSelfDeclared = false,
                    HasBankruptcySelfDeclared = false,
                    Phone = "232332321",
                    SelfDeclaredCreditScore = "701-750",
                    SSN = "456123789"
                }
            };
        }


        /// <summary>
        /// Fetch entity relation object.
        /// </summary>
        /// <returns></returns>
        private EntityRelationMappingAC GetEntityRelation()
        {
            return new EntityRelationMappingAC
            {
                Id = Guid.NewGuid(),
                PrimaryEntityId = Guid.NewGuid(),
                Relation = new RelationshipAC
                {
                    Id = Guid.NewGuid(),
                    Name = "xyz"
                },
                SharePercentage = 20
            };
        }

        /// <summary>
        /// Fetch the logged in user details.
        /// </summary>
        /// <returns></returns>
        private User FetchLoggedInUser()
        {
            return new User()
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@doe.com",
                Phone = "9898989898",
                SSN = "123456789",
                SelfDeclaredCreditScore = "651-700",
                HasAnyJudgementsSelfDeclared = false,
                HasBankruptcySelfDeclared = false
            };
        }

        /// <summary>
        /// Fetch the logged in user details (CurrentUserAC).
        /// </summary>
        /// <returns></returns>
        private CurrentUserAC FetchLoggedInUserAC()
        {
            return new CurrentUserAC()
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@doe.com",
                IsBankUser = false
            };
        }

        /// <summary>
        /// Fetch finance statement list
        /// </summary>
        /// <returns></returns>
        private List<FinancialStatement> FetchFinancialStatements()
        {
            return new List<FinancialStatement>
            {
                new FinancialStatement
                {
                    Name=StringConstant.IncomeStatement
                },
                new FinancialStatement
                {
                    Name=StringConstant.BalanceSheet
                },
                new FinancialStatement
                {
                    Name=StringConstant.CashFlow
                },
                new FinancialStatement
                {
                    Name=StringConstant.FinancialRatios
                }
            };
        }

        /// <summary>
        /// Fetch entity finance list
        /// </summary>
        /// <returns></returns>
        private List<EntityFinance> FetchEntityFinance()
        {
            return new List<EntityFinance>
            {
                new EntityFinance
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityFinanceYearlyMappings=new List<EntityFinanceYearlyMapping>
                    {
                        new EntityFinanceYearlyMapping
                        {
                            EntityFinanceStandardAccounts=new List<EntityFinanceStandardAccount>
                            {
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Income",
                                    Order=1,
                                    ParentId=3,
                                    Amount=12
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Expense",
                                    Order=2,
                                    ParentId=1,
                                    Amount=120
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Cogs",
                                    Order=3,
                                    ParentId=2,
                                    Amount=112
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Profit",
                                    Order=4,
                                    ParentId=4,
                                    Amount=123
                                }
                            },
                            LastAddedDateTime=DateTime.UtcNow,
                            Period="Jan - Dec 2020"

                        },
                        new EntityFinanceYearlyMapping
                        {
                            EntityFinanceStandardAccounts=new List<EntityFinanceStandardAccount>
                            {
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Income",
                                    Order=1,
                                    ParentId=3,
                                    Amount=12
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Expense",
                                    Order=2,
                                    ParentId=1,
                                    Amount=120
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Cogs",
                                    Order=3,
                                    ParentId=2,
                                    Amount=112
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Profit",
                                    Order=4,
                                    ParentId=4,
                                    Amount=123
                                }
                            },
                            LastAddedDateTime=DateTime.UtcNow,
                            Period="Jan - Dec 2019"

                        },

                    },
                    EntityId=_entity.Id.Value,
                    IntegratedServiceConfigurationId=_integratedServiceConfigurationList.First().Id,
                    FinancialInformationJson="some json",
                    FinancialStatement=new FinancialStatement
                    {
                        Name=StringConstant.IncomeStatement,
                        Id=Guid.NewGuid(),
                        IsAutoCalculated=false
                    }

                },
                new EntityFinance
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityFinanceYearlyMappings=new List<EntityFinanceYearlyMapping>
                    {

                        new EntityFinanceYearlyMapping
                        {
                            EntityFinanceStandardAccounts=new List<EntityFinanceStandardAccount>
                            {
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Assets",
                                    Order=1,
                                    ParentId=3,
                                    Amount=12
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Liabilities",
                                    Order=2,
                                    ParentId=1,
                                    Amount=120
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Current Assets",
                                    Order=3,
                                    ParentId=2,
                                    Amount=112
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Equity",
                                    Order=4,
                                    ParentId=4,
                                    Amount=123
                                }
                            },
                            LastAddedDateTime=DateTime.UtcNow,
                            Period="Jan - Dec 2020"
                        },

                        new EntityFinanceYearlyMapping
                        {
                            EntityFinanceStandardAccounts=new List<EntityFinanceStandardAccount>
                            {
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Assets",
                                    Order=1,
                                    ParentId=3,
                                    Amount=12
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Liabilities",
                                    Order=2,
                                    ParentId=1,
                                    Amount=120
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Current Assets",
                                    Order=3,
                                    ParentId=2,
                                    Amount=112
                                },
                                new EntityFinanceStandardAccount
                                {
                                    ExpectedValue=2,
                                    Name="Total Equity",
                                    Order=4,
                                    ParentId=4,
                                    Amount=123
                                }
                            },
                            LastAddedDateTime=DateTime.UtcNow,
                            Period="Jan - Dec 2019"
                        }
                    },
                    EntityId=_entity.Id.Value,
                    IntegratedServiceConfigurationId=_integratedServiceConfigurationList.First().Id,
                    FinancialInformationJson="some json",
                    FinancialStatement=new FinancialStatement
                    {
                        Name=StringConstant.BalanceSheet,
                        Id=Guid.NewGuid(),
                        IsAutoCalculated=false
                    }

                }
            };
        }

        /// <summary>
        /// Fetch integrated service configuration list
        /// </summary>
        /// <returns></returns>
        private List<IntegratedServiceConfiguration> FetchIntegratedServiceConfiguration()
        {
            return new List<IntegratedServiceConfiguration>
            {
                new IntegratedServiceConfiguration
                {
                    ConfigurationJson="[{\"Key\":\"AppEnvironment\",\"Path\":\"Quickbooks:AppEnvironment\",\"Value\":\"sandbox\"},{\"Key\":\"BaseUrl\",\"Path\":\"Quickbooks:BaseUrl\",\"Value\":\"https://sandbox-quickbooks.api.intuit.com/\"},{\"Key\":\"ClientId\",\"Path\":\"Quickbooks:ClientId\",\"Value\":\"AB1TePFIzpgAWdSQWV8st03TEXnd6ViIUacVzPZghu3aWl2Ms6\"},{\"Key\":\"ClientSecret\",\"Path\":\"Quickbooks:ClientSecret\",\"Value\":\"xGmK45Jym2JaHiF3YNN4yjsZlPBApQvhTFYHeVCs\"},{\"Key\":\"ConfigurationVersion\",\"Path\":\"Quickbooks:ConfigurationVersion\",\"Value\":\"29\"},{\"Key\":\"RedirectUri\",\"Path\":\"Quickbooks:RedirectUri\",\"Value\":\"http://localhost:4200/loan/company/quickbooksredirect\"},{\"Key\":\"Token\",\"Path\":\"Quickbooks:Token\",\"Value\":\"eyJlbmMiOiJBMTI4Q0JDLUhTMjU2IiwiYWxnIjoiZGlyIn0..uIIvOYmJY6SPirurCj6RHw.mBN1zpS-BC9UVDmMkoV79TafsD9w68SGNr1GnlzTlYlTH7md_0CC02ccR0cDUe2yHsHEs1c4eYAcUxOweqbVrnlDCgcOhINRXCLknRYw8A0UzNzIRwrlODatGpeRF8FHaA0LIhaWxTuj1iEiuVhzpORgAuBC_ZvYrPo5vYCBxCl0ifcGyUwfXsCV9xR1rCWqsNAT60D7N4vh2-kqQyPjv-yAe-wvUtzTSpNsU2FzE0xva6SdihOoAgoMdOWkkZ2MvGFKDJFui-vJf1tyItWYRIXUf3JzGQzoycZg0MXu2HoWzLSXdAZvCykE4g6DnyTCIYfKSjGABkq0yr5TzyShOo8-rPWpO1l-AB2VFCfovqGgB3R9mCYr1Fq25HK-OxoRJeJ-J80kh2mAO2R_w1XX4B4DMksTdZ-5sM8_BnooyKoXh0_Upz7qKVZHuAqTklJdnDv470YQL8GAQglauYNhbgRWtAf2gpP-u7aI0UkAwgfg3QmyPHgKdi5iJRlLMxl8pFYXVfYVI7xci9fzNNOQfpa7zVpEPaJha5aBLvnUygdMG7KA4quYxF3PVBm27MVHp65GKRFEON7tv1mm6X3fic8mDMPg6uAVwD1MJ7tfuGRArPYdPDzTYWbZw4KpujOudXcbbEzBOEc-gRrlRl2QsRAjy5OgAsjyPLa4sQWI08l2WuJScnNVEcrKv5Sd6k26lenyeXNcxUKWM5XAnBhqV9rV5rMlW2ohGIjcbTBZozc.4KCM1e0JoOYwW5moAaMEDQ\"},{\"Key\":\"RealmId\",\"Path\":\"Quickbooks:RealmId\",\"Value\":\"4620816365045340610\"}]",
                    IsServiceEnabled=true,
                    Name=StringConstant.Quickbooks,
                    Id=Guid.NewGuid()
                },
                new IntegratedServiceConfiguration
                {
                    ConfigurationJson="[{\"Key\":\"AppEnvironment\",\"Path\":\"Quickbooks:AppEnvironment\",\"Value\":\"sandbox\"},{\"Key\":\"BaseUrl\",\"Path\":\"Quickbooks:BaseUrl\",\"Value\":\"https://sandbox-quickbooks.api.intuit.com/\"},{\"Key\":\"ClientId\",\"Path\":\"Quickbooks:ClientId\",\"Value\":\"AB1TePFIzpgAWdSQWV8st03TEXnd6ViIUacVzPZghu3aWl2Ms6\"},{\"Key\":\"ClientSecret\",\"Path\":\"Quickbooks:ClientSecret\",\"Value\":\"xGmK45Jym2JaHiF3YNN4yjsZlPBApQvhTFYHeVCs\"},{\"Key\":\"ConfigurationVersion\",\"Path\":\"Quickbooks:ConfigurationVersion\",\"Value\":\"29\"},{\"Key\":\"RedirectUri\",\"Path\":\"Quickbooks:RedirectUri\",\"Value\":\"http://localhost:4200/loan/company/quickbooksredirect\"},{\"Key\":\"Token\",\"Path\":\"Quickbooks:Token\",\"Value\":\"eyJlbmMiOiJBMTI4Q0JDLUhTMjU2IiwiYWxnIjoiZGlyIn0..uIIvOYmJY6SPirurCj6RHw.mBN1zpS-BC9UVDmMkoV79TafsD9w68SGNr1GnlzTlYlTH7md_0CC02ccR0cDUe2yHsHEs1c4eYAcUxOweqbVrnlDCgcOhINRXCLknRYw8A0UzNzIRwrlODatGpeRF8FHaA0LIhaWxTuj1iEiuVhzpORgAuBC_ZvYrPo5vYCBxCl0ifcGyUwfXsCV9xR1rCWqsNAT60D7N4vh2-kqQyPjv-yAe-wvUtzTSpNsU2FzE0xva6SdihOoAgoMdOWkkZ2MvGFKDJFui-vJf1tyItWYRIXUf3JzGQzoycZg0MXu2HoWzLSXdAZvCykE4g6DnyTCIYfKSjGABkq0yr5TzyShOo8-rPWpO1l-AB2VFCfovqGgB3R9mCYr1Fq25HK-OxoRJeJ-J80kh2mAO2R_w1XX4B4DMksTdZ-5sM8_BnooyKoXh0_Upz7qKVZHuAqTklJdnDv470YQL8GAQglauYNhbgRWtAf2gpP-u7aI0UkAwgfg3QmyPHgKdi5iJRlLMxl8pFYXVfYVI7xci9fzNNOQfpa7zVpEPaJha5aBLvnUygdMG7KA4quYxF3PVBm27MVHp65GKRFEON7tv1mm6X3fic8mDMPg6uAVwD1MJ7tfuGRArPYdPDzTYWbZw4KpujOudXcbbEzBOEc-gRrlRl2QsRAjy5OgAsjyPLa4sQWI08l2WuJScnNVEcrKv5Sd6k26lenyeXNcxUKWM5XAnBhqV9rV5rMlW2ohGIjcbTBZozc.4KCM1e0JoOYwW5moAaMEDQ\"},{\"Key\":\"RealmId\",\"Path\":\"Quickbooks:RealmId\",\"Value\":\"4620816365045340610\"}]",
                    IsServiceEnabled=true,
                    Name=StringConstant.Xero,
                    Id=Guid.NewGuid()
                },
            };
        }

        /// <summary>
        /// Fetch third party callback data
        /// </summary>
        /// <returns></returns>
        private ThirdPartyServiceCallbackDataAC FetchThirdPartyCallbackData()
        {
            return new ThirdPartyServiceCallbackDataAC
            {
                EntityId = Guid.NewGuid(),
                BearerToken = "something",
                AuthorizationCode = "something"
            };
        }

        /// <summary>
        /// Fetch list of finance AC
        /// </summary>
        /// <returns></returns>
        private List<CompanyFinanceAC> FetchListOfFinanceAC()
        {
            var financeACList = new List<CompanyFinanceAC>();
            foreach (var finance in _entityFinanceList)
            {
                var financeAc = _mapper.Map<CompanyFinanceAC>(finance);
                financeAc.DivisionFactor = 1000;
                financeACList.Add(financeAc);
            }
            return financeACList;
        }

        /// <summary>
        /// Method to fetch all personal finance accounts with all mappings
        /// </summary>
        /// <returns></returns>
        private List<PersonalFinanceAccount> FetchPersonalFinanceAccountsWithAllMappings()
        {
            List<Guid> ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            return new List<PersonalFinanceAccount>
            {
                new PersonalFinanceAccount {
                    Id = ids[0],
                    IsEnabled = true,
                    Name = "Account1",
                    Order = 1,
                    PersonalFinanceCategories = new List<PersonalFinanceCategory>
                    {
                        new PersonalFinanceCategory
                        {
                            Id = ids[2],
                            Name = "Category1",
                            Order = 1,
                            IsEnabled = true,
                            PersonalFinanceAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>
                            {
                                new PersonalFinanceAttributeCategoryMapping
                                {
                                    Id = Guid.NewGuid(),
                                    AttributeId = ids[4],
                                    Attribute = new PersonalFinanceAttribute
                                    {
                                        Id = ids[4],
                                        FieldType = PersonalFinanceAttributeFieldType.Boolean,
                                        IsEnabled = true,
                                        Text = "Attribute1"
                                    },
                                    ChildAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>
                                    {
                                        new PersonalFinanceAttributeCategoryMapping
                                        {
                                            Id = Guid.NewGuid(),
                                            AttributeId = ids[5],
                                            Attribute = new PersonalFinanceAttribute
                                            {
                                                Id = ids[5],
                                                FieldType = PersonalFinanceAttributeFieldType.Dropdown,
                                                IsEnabled = true,
                                                Text = "Attribute2"
                                            },
                                            PersonalFinanceConstantId = Guid.NewGuid(),
                                            PersonalFinanceConstant = new PersonalFinanceConstant
                                            {
                                                Id = Guid.NewGuid(),
                                                Name = "Frequency",
                                                ValueJson = "[{\"Id\":1,\"Value\":\"Current\",\"Order\":1,\"IsEnabled\":true},{\"Id\":2,\"Value\":\"Delinquent\",\"Order\":2,\"IsEnabled\":true}]",
                                            },
                                            PersonalFinanceResponses = new List<PersonalFinanceResponse>
                                            {
                                                new PersonalFinanceResponse
                                                {
                                                    Id = Guid.NewGuid(),
                                                    Answer = "Current",
                                                    Order = 1,
                                                    EntityFinanceId = Guid.NewGuid(),
                                                    PersonalFinanceAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                                                    {
                                                        Attribute = new PersonalFinanceAttribute
                                                        {
                                                            Id = ids[5],
                                                            FieldType = PersonalFinanceAttributeFieldType.Dropdown,
                                                            IsEnabled = true,
                                                            Text = "Attribute2"
                                                        },
                                                        PersonalFinanceConstant = new PersonalFinanceConstant
                                                        {
                                                            Id = Guid.NewGuid(),
                                                            Name = "Frequency",
                                                            ValueJson = "{\"Id\":1,\"Name\":\"LoanStatus\",\"Options\":[{\"Id\":1,\"Value\":\"Current\",\"Order\":1,\"IsEnabled\":true},{\"Id\":2,\"Value\":\"Delinquent\",\"Order\":2,\"IsEnabled\":true}]}",
                                                        }
                                                    }
                                                }
                                            },
                                            ChildAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>(),
                                            ParentChildCategoryMappings = new List<PersonalFinanceParentChildCategoryMapping>()
                                        }
                                    },
                                    PersonalFinanceResponses = new List<PersonalFinanceResponse>
                                    {
                                        new PersonalFinanceResponse
                                        {
                                            Id = Guid.NewGuid(),
                                            Answer = "false",
                                            Order = 1,
                                            EntityFinanceId = Guid.NewGuid(),
                                            PersonalFinanceAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                                            {
                                                Attribute = new PersonalFinanceAttribute
                                                {
                                                    Id = ids[4],
                                                    FieldType = PersonalFinanceAttributeFieldType.Boolean,
                                                    IsEnabled = true,
                                                    Text = "Attribute1"
                                                }
                                            }
                                        }
                                    },
                                    ParentChildCategoryMappings = new List<PersonalFinanceParentChildCategoryMapping>()
                                }
                            },
                            MappedAsParentCategoryMappings = new List<PersonalFinanceParentChildCategoryMapping>
                            {
                                new PersonalFinanceParentChildCategoryMapping
                                {
                                    Id = Guid.NewGuid(),
                                    ParentCategoryId = ids[2],
                                    ParentCategory = new PersonalFinanceCategory
                                    {
                                        Id = ids[2],
                                        Name = "Category1",
                                        Order = 1,
                                        IsEnabled = true
                                    },
                                    ChildCategoryId = ids[3],
                                    ChildCategory = new PersonalFinanceCategory
                                    {
                                        Id = ids[3],
                                        Name = "Category2",
                                        Order = 1,
                                        IsEnabled = true,
                                        PersonalFinanceAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>
                                        {
                                            new PersonalFinanceAttributeCategoryMapping
                                            {
                                                Id = Guid.NewGuid(),
                                                AttributeId = ids[6],
                                                Attribute = new PersonalFinanceAttribute
                                                {
                                                    Id = ids[6],
                                                    FieldType = PersonalFinanceAttributeFieldType.Boolean,
                                                    IsEnabled = true,
                                                    Text = "Attribute3"
                                                },
                                                ChildAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>
                                                {
                                                    new PersonalFinanceAttributeCategoryMapping
                                                    {
                                                        Id = Guid.NewGuid(),
                                                        AttributeId = ids[7],
                                                        Attribute = new PersonalFinanceAttribute
                                                        {
                                                            Id = ids[7],
                                                            FieldType = PersonalFinanceAttributeFieldType.Number,
                                                            IsEnabled = true,
                                                            Text = "Attribute4"
                                                        },
                                                        PersonalFinanceResponses = new List<PersonalFinanceResponse>
                                                        {
                                                            new PersonalFinanceResponse
                                                            {
                                                                Id = Guid.NewGuid(),
                                                                Answer = "Current",
                                                                Order = 1,
                                                                EntityFinanceId = Guid.NewGuid(),
                                                                PersonalFinanceAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                                                                {
                                                                    Attribute = new PersonalFinanceAttribute
                                                                    {
                                                                        Id = ids[5],
                                                                        FieldType = PersonalFinanceAttributeFieldType.Dropdown,
                                                                        IsEnabled = true,
                                                                        Text = "Attribute4"
                                                                    },
                                                                    PersonalFinanceConstant = new PersonalFinanceConstant
                                                                    {
                                                                        Id = Guid.NewGuid(),
                                                                        Name = "Frequency",
                                                                        ValueJson = "{\"Id\":1,\"Name\":\"LoanStatus\",\"Options\":[{\"Id\":1,\"Value\":\"Current\",\"Order\":1,\"IsEnabled\":true},{\"Id\":2,\"Value\":\"Delinquent\",\"Order\":2,\"IsEnabled\":true}]}",
                                                                    }
                                                                }
                                                            }
                                                        },
                                                        ChildAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>()
                                                    }
                                                },
                                                PersonalFinanceResponses = new List<PersonalFinanceResponse>
                                                {
                                                    new PersonalFinanceResponse
                                                    {
                                                        Id = Guid.NewGuid(),
                                                        Answer = "false",
                                                        Order = 1,
                                                        EntityFinanceId = Guid.NewGuid(),
                                                        PersonalFinanceAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                                                        {
                                                            Attribute = new PersonalFinanceAttribute
                                                            {
                                                                Id = ids[4],
                                                                FieldType = PersonalFinanceAttributeFieldType.Boolean,
                                                                IsEnabled = true,
                                                                Text = "Attribute3"
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        MappedAsParentCategoryMappings = new List<PersonalFinanceParentChildCategoryMapping>()
                                    },
                                    ParentAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                                    {
                                        Id = Guid.NewGuid(),
                                        AttributeId = ids[4],
                                        CategoryId = ids[2],
                                        IsCurrent = true,
                                        Order = 1,
                                        IsOriginal = true,
                                        Attribute = new PersonalFinanceAttribute
                                        {
                                            Id = ids[4],
                                            FieldType = PersonalFinanceAttributeFieldType.Boolean,
                                            IsEnabled = true,
                                            Text = "Attribute1"
                                        },
                                        ChildAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>(),
                                        ParentChildCategoryMappings = new List<PersonalFinanceParentChildCategoryMapping>(),
                                        PersonalFinanceResponses = new List<PersonalFinanceResponse>()
                                    },
                                    ParentAttributeCategoryMappingId = Guid.NewGuid()
                                }
                            },
                            MappedAsChildCategoryMappings = new List<PersonalFinanceParentChildCategoryMapping>()
                        }
                    }
                },
                new PersonalFinanceAccount {
                    Id = ids[1],
                    IsEnabled = true,
                    Name = "Account2",
                    Order = 2,
                    PersonalFinanceCategories = new List<PersonalFinanceCategory>
                    {
                        new PersonalFinanceCategory
                        {
                            Id = ids[3],
                            Name = "Category2",
                            Order = 1,
                            IsEnabled = true,
                            PersonalFinanceAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>
                            {
                                new PersonalFinanceAttributeCategoryMapping
                                {
                                    Id = Guid.NewGuid(),
                                    AttributeId = ids[6],
                                    Attribute = new PersonalFinanceAttribute
                                    {
                                        Id = ids[6],
                                        FieldType = PersonalFinanceAttributeFieldType.Boolean,
                                        IsEnabled = true,
                                        Text = "Attribute3"
                                    },
                                    ChildAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>
                                    {
                                        new PersonalFinanceAttributeCategoryMapping
                                        {
                                            Id = Guid.NewGuid(),
                                            AttributeId = ids[7],
                                            Attribute = new PersonalFinanceAttribute
                                            {
                                                Id = ids[7],
                                                FieldType = PersonalFinanceAttributeFieldType.Number,
                                                IsEnabled = true,
                                                Text = "Attribute4"
                                            },
                                            ChildAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>()
                                        }
                                    }
                                }
                            },
                            MappedAsChildCategoryMappings = new List<PersonalFinanceParentChildCategoryMapping>
                            {
                                new PersonalFinanceParentChildCategoryMapping
                                {
                                    Id = Guid.NewGuid(),
                                    ParentCategoryId = ids[2],
                                    ParentCategory = new PersonalFinanceCategory
                                    {
                                        Id = ids[2],
                                        Name = "Category1",
                                        Order = 1,
                                        IsEnabled = true
                                    },
                                    ChildCategoryId = ids[3],
                                    ChildCategory = new PersonalFinanceCategory
                                    {
                                        Id = ids[3],
                                        Name = "Category2",
                                        Order = 1,
                                        IsEnabled = true
                                    },
                                    ParentAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                                    {
                                        Id = Guid.NewGuid(),
                                        AttributeId = ids[4],
                                        CategoryId = ids[2],
                                        IsCurrent = true,
                                        Order = 1,
                                        IsOriginal = true,
                                        Attribute = new PersonalFinanceAttribute
                                        {
                                            Id = ids[4],
                                            FieldType = PersonalFinanceAttributeFieldType.Dropdown,
                                            IsEnabled = true,
                                            Text = "Attribute1"
                                        }
                                    }
                                }
                            },
                            MappedAsParentCategoryMappings = new List<PersonalFinanceParentChildCategoryMapping>()
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Method to get PersonalFinanceCategoryAC object for add/update personal finances.
        /// </summary>
        /// <returns></returns>
        private PersonalFinanceCategoryAC GetPersonalFinanceCategoryACObjectToSave()
        {
            var attributeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            return new PersonalFinanceCategoryAC
            {
                Id = Guid.NewGuid(),
                Name = "Category1",
                Order = 1,
                Attributes = new List<PersonalFinanceAttributeAC>
                {
                    new PersonalFinanceAttributeAC
                    {
                        Id = attributeIds[0],
                        Order = 1,
                        Answer = "false",
                        Text = "Do you have attribute1?",
                        Constant = null,
                        FieldType = PersonalFinanceAttributeFieldType.Boolean,
                        ChildAttributeSets = new List<PersonalFinanceOrderedAttributeAC>
                        {
                            new PersonalFinanceOrderedAttributeAC
                            {
                                Order = 1,
                                ChildAttributes = new List<PersonalFinanceAttributeAC>
                                {
                                    new PersonalFinanceAttributeAC
                                    {
                                        Id = attributeIds[1],
                                        Order = 1,
                                        Answer = "50505",
                                        Text = "What is the amount of attribute1?",
                                        Constant = null,
                                        FieldType = PersonalFinanceAttributeFieldType.Number,
                                        ChildAttributeSets = new List<PersonalFinanceOrderedAttributeAC>()
                                    }
                                }
                            },
                            new PersonalFinanceOrderedAttributeAC
                            {
                                Order = 2,
                                ChildAttributes = new List<PersonalFinanceAttributeAC>
                                {
                                    new PersonalFinanceAttributeAC
                                    {
                                        Id = attributeIds[1],
                                        Order = 1,
                                        Answer = "10101",
                                        Text = "What is the amount of attribute1?",
                                        Constant = null,
                                        FieldType = PersonalFinanceAttributeFieldType.Number,
                                        ChildAttributeSets = new List<PersonalFinanceOrderedAttributeAC>()
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Test to verify proper exception is thrown on invalid parameters
        /// </summary>
        /// <param name="entityIdInputted"></param>
        /// <param name="reportsCsvInputted"></param>
        /// <param name="expectedException"></param>
        /// <returns></returns>
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", ResourceType.Company, StringConstant.FinancialReportCsv, typeof(InvalidParameterException))]
        [InlineData("bd920025-22d2-4b3c-8c53-d1b94232d036", ResourceType.Company, null, typeof(InvalidParameterException))]
        [InlineData("bd920025-22d2-4b3c-8c53-d1b94232d036", ResourceType.Company, "Profit and Loss", typeof(InvalidParameterException))]
        [InlineData("bd920025-22d2-4b3c-8c53-d1b94232d036", ResourceType.Other, "Balance Sheet", typeof(InvalidParameterException))]
        public async Task GetFinancesAsync_GetFinancesWithImproperParameters_VerifyProperExceptionIsThrown(string entityIdInputted, ResourceType type, string reportsCsvInputted, Type expectedException)
        {
            // Arrange

            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<FinancialStatement>()).Returns(_financialStatementList.AsQueryable());

            // Act


            // Assert
            Exception thrownException = await Assert.ThrowsAsync(expectedException, () => _entityFinanceRepository.GetFinancesAsync(Guid.Parse(entityIdInputted), type, reportsCsvInputted, _currentUserAC));
            Assert.Equal(StringConstant.MissingStatementsInParameter, thrownException.Message);
        }

        /// <summary>
        /// Verify invalid resource access exception
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(ResourceType.Loan)]
        [InlineData(ResourceType.Company)]
        public async Task GetFinancesAsync_GetFinancesWithInvalidUser_VerifyInvalidResourceExceptionIsThrown(ResourceType type)
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);
            _dataRepositoryMock.Setup(x => x.GetAll<FinancialStatement>()).Returns(_financialStatementList.AsQueryable());

            // Act


            // Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityFinanceRepository.GetFinancesAsync(Guid.NewGuid(), type, StringConstant.FinancialReportCsv, _currentUserAC));

        }


        /// <summary>
        /// Verify data not found exception
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(ResourceType.Loan)]
        [InlineData(ResourceType.Company)]
        public async Task GetFinancesAsync_GetFinancesFromConnectedService_VerifyDataNotFoundExceptionIsThrown(ResourceType type)
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<FinancialStatement>()).Returns(_financialStatementList.AsQueryable());
            _dataRepositoryMock.SetupSequence(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(new List<EntityFinance>().AsQueryable())
                .Returns(new List<EntityFinance>().AsQueryable().BuildMock().Object);

            // Act


            // Assert
            Exception thrownException = await Assert.ThrowsAsync<DataNotFoundException>(() => _entityFinanceRepository.GetFinancesAsync(Guid.NewGuid(), type, StringConstant.FinancialReportCsv, _currentUserAC));
            Assert.Equal(StringConstant.DataNotFound, thrownException.Message);

        }

        /// <summary>
        /// Verify and assert data
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(ResourceType.Loan)]
        [InlineData(ResourceType.Company)]
        public async Task GetFinancesAsync_GetFinancesWithValidUser_VerifyData(ResourceType type)
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            if (type == ResourceType.Loan)
            {
                var version = Guid.NewGuid();
                _entityFinanceList.ForEach(x =>
                {
                    x.Version = version;
                });
            }
            _dataRepositoryMock.Setup(x => x.GetAll<FinancialStatement>()).Returns(_financialStatementList.AsQueryable());
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(_entityFinanceList.AsQueryable().BuildMock().Object);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("1000")
                .Returns("1000");
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            // Act
            var financeList = await _entityFinanceRepository.GetFinancesAsync(Guid.NewGuid(), type, StringConstant.FinancialReportCsv, _currentUserAC);

            // Assert
            Assert.NotEmpty(financeList);
            Assert.True(financeList.All(x => x.StandardAccountList.Count > 0));
            Assert.True(financeList.All(x => x.IsChartOfAccountMapped));
        }

        /// <summary>
        /// Verify proper exception is thrown for this method on invalid parameters
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="source"></param>
        /// <param name="expectedException"></param>
        /// <returns></returns>
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", StringConstant.Quickbooks, typeof(InvalidParameterException))]
        [InlineData("bd920025-22d2-4b3c-8c53-d1b94232d036", null, typeof(InvalidParameterException))]
        [InlineData("bd920025-22d2-4b3c-8c53-d1b94232d036", "SomeRandomServiceName", typeof(InvalidParameterException))]
        public async Task GetAuthorizationUrlAsync_GetUrlWithImproperParameters_VerifyProperExceptionIsThrown(Guid entityId, string source, Type expectedException)
        {
            // Arrange
            if (!string.IsNullOrEmpty(source) && (source != StringConstant.Quickbooks || source != StringConstant.Xero))
            {
                _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
                _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<IntegratedServiceConfiguration>(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>())).ReturnsAsync((IntegratedServiceConfiguration)null);
            }
            // Act


            // Assert
            await Assert.ThrowsAsync(expectedException, () => _entityFinanceRepository.GetAuthorizationUrlAsync(entityId, source, _currentUserAC));
        }

        /// <summary>
        /// Verify invalid resource access exception is thrown
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthorizationUrlAsync_GetUrlWithInvalidUser_VerifyInvalidResourceExceptionIsThrown()
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);


            // Act


            // Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityFinanceRepository.GetAuthorizationUrlAsync(Guid.NewGuid(), StringConstant.Quickbooks, _currentUserAC));

        }

        /// <summary>
        /// Verify invalid url exception is thrown
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthorizationUrlAsync_GetUrlFromConnectedService_VerifyInvalidExceptionIsThrown()
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<IntegratedServiceConfiguration>(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>())).ReturnsAsync((IntegratedServiceConfiguration)null);

            // Act


            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityFinanceRepository.GetAuthorizationUrlAsync(Guid.NewGuid(), StringConstant.Quickbooks, _currentUserAC));


        }

        [Theory]
        [InlineData(StringConstant.Quickbooks)]
        [InlineData(StringConstant.Xero)]
        public async Task GetAuthorizationUrlAsync_GetUrlFromConnectedService_VerifyValidUrlIsComing(string thirdPartyService)
        {
            // Arrange
            var conf = _integratedServiceConfigurationList.First(x => x.Name == thirdPartyService);
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<IntegratedServiceConfiguration>(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>())).ReturnsAsync(conf);

            if (thirdPartyService == StringConstant.Quickbooks)
            {
                _quickbooksUtilityMock.Setup(x => x.GetAuthorizationUrl(It.IsAny<Guid>(), conf.ConfigurationJson)).Returns("somevalidurl");
            }
            else if (thirdPartyService == StringConstant.Xero)
            {
                _xeroUtilityMock.Setup(x => x.GetLoginUrl(It.IsAny<Guid>(), conf.ConfigurationJson)).Returns("somevalidurl");
            }

            // Act


            // Assert
            Assert.NotNull(await _entityFinanceRepository.GetAuthorizationUrlAsync(_entity.Id.Value, thirdPartyService, _currentUserAC));


        }


        /// <summary>
        /// Verify proper exception is thrown while saving with improper parameters
        /// </summary>
        /// <param name="validCallbackData"></param>
        /// <param name="statementsCsv"></param>
        /// <param name="expectedException"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(true, "", typeof(InvalidParameterException))]
        [InlineData(true, null, typeof(InvalidParameterException))]
        [InlineData(false, StringConstant.FinancialReportCsv, typeof(InvalidParameterException))]
        public async Task AddOrUpdateFinancesAsync_SaveWithImproperParameters_VerifyProperExceptionIsThrown(bool validCallbackData, string statementsCsv, Type expectedException)
        {

            // Arrange
            if (!validCallbackData)
            {
                _thirdPartyCallbackData.EntityId = Guid.Empty;
            }

            // Act


            // Assert
            await Assert.ThrowsAsync(expectedException, () => _entityFinanceRepository.AddOrUpdateFinancesAsync(_thirdPartyCallbackData, statementsCsv, _currentUserAC));
        }

        /// <summary>
        /// Verify invalid resource access exception is thrown
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateFinancesAsync_AddOrUpdateWithInvalidUser_VerifyInvalidResourceExceptionIsThrown()
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);


            // Act


            // Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _entityFinanceRepository.AddOrUpdateFinancesAsync(_thirdPartyCallbackData, StringConstant.Quickbooks, _currentUserAC));

        }

        /// <summary>
        /// Verify invalid data exception is thrown
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateFinancesAsync_CheckFinancialStatementValidOrNot_VerifyInvalidDataExceptionIsThrown()
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<FinancialStatement>()).Returns(new List<FinancialStatement>().AsQueryable());

            // Act


            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityFinanceRepository.GetFinancesAsync(Guid.NewGuid(), ResourceType.Company, StringConstant.FinancialReportCsv, _currentUserAC));

        }

        /// <summary>
        /// Verify invalid exception is thrown
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateFinancesAsync_AddOrUpdateFromConnectedService_VerifyInvalidExceptionIsThrown()
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<FinancialStatement>()).Returns(_financialStatementList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.GetAll<IntegratedServiceConfiguration>()).Returns(new List<IntegratedServiceConfiguration>().AsQueryable().BuildMock().Object);

            // Act


            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityFinanceRepository.AddOrUpdateFinancesAsync(_thirdPartyCallbackData, StringConstant.FinancialReportCsv, _currentUserAC));


        }

        /// <summary>
        /// If company type is CCorp then add its start date and end date and verify add and update call.
        /// </summary>
        [Fact]
        public async Task AddOrUpdateFinancesAsync_EntityCompanyCCorp_VerifyAddAndUpdateEntityFinance()
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);

            _dataRepositoryMock.Setup(x => x.GetAll<FinancialStatement>()).Returns(_financialStatementList.AsQueryable().BuildMock().Object);
            var integratedServiceConfrigationList = new List<IntegratedServiceConfiguration>()
            {
                new IntegratedServiceConfiguration
                {
                    ConfigurationJson=@"[{Key:'Token',Path:'test',Value:'test'},{Key:'RealmId',Path:'test',Value:'test'}]",
                    Name=StringConstant.Quickbooks
                }
            };
            _dataRepositoryMock.Setup(x => x.GetAll<IntegratedServiceConfiguration>()).Returns(integratedServiceConfrigationList.AsQueryable().BuildMock().Object);
            _thirdPartyCallbackData.ThirdPartyServiceName = StringConstant.Quickbooks;
            _thirdPartyCallbackData.RealmId = "test";
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(_entityFinanceList.AsQueryable().BuildMock().Object);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("true")
                .Returns("2")
                .Returns(StringConstant.January)
                .Returns("true");
            Company company = new Company
            {
                CompanyFiscalYearStartMonth = 5
            };
            _dataRepositoryMock.Setup(x => x.SingleAsync(It.IsAny<Expression<Func<Company, bool>>>()))
                .Returns(Task.FromResult(company));
            _globalRepositoryMock.Setup(x => x.GetMonthNameFromMonthNumber(It.IsAny<int>())).Returns("January");
            var financialStatementList = new List<FinancialStatementsAC>()
            {
                new FinancialStatementsAC
                {
                    ReportJson=@"",
                    ReportName=StringConstant.IncomeStatement,
                    ThirdPartyWiseCompanyName="",
                    ThirdPartyWiseName=""
                },
                new FinancialStatementsAC
                {
                    ReportJson=@"",
                    ReportName=StringConstant.BalanceSheet,
                    ThirdPartyWiseCompanyName="",
                    ThirdPartyWiseName=""
                },
                new FinancialStatementsAC
                {
                    ReportJson=@"",
                    ReportName=StringConstant.CashFlow,
                    ThirdPartyWiseCompanyName="",
                    ThirdPartyWiseName=""
                },
                new FinancialStatementsAC
                {
                    ReportJson=@"",
                    ReportName=StringConstant.FinancialRatios,
                    ThirdPartyWiseCompanyName="",
                    ThirdPartyWiseName=""
                }
            };
            _quickbooksUtilityMock.Setup(x => x.FetchQuickbooksReport(It.IsAny<ThirdPartyServiceCallbackDataAC>())).Returns(financialStatementList);
            // Act

            await _entityFinanceRepository.AddOrUpdateFinancesAsync(_thirdPartyCallbackData, StringConstant.FinancialReportCsv, _currentUserAC);
            // Assert
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityFinance>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<EntityFinance>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Exactly(2));
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);

        }

        /// <summary>
        /// Verify when invalid parameter exception is thrown
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddOrUpdateFinancesAsync_AddOrUpdateFromConnectedService_VerifyInvalidParameterExceptionIsThrown()
        {
            // Arrange
            _globalRepositoryMock.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.GetAll<FinancialStatement>()).Returns(new List<FinancialStatement>().AsQueryable().BuildMock().Object);

            // Act


            // Assert
            await Assert.ThrowsAsync<InvalidParameterException>(() => _entityFinanceRepository.AddOrUpdateFinancesAsync(_thirdPartyCallbackData, StringConstant.FinancialReportCsv, _currentUserAC));

        }

        /// <summary>
        /// Method to verify data
        /// </summary>
        [Fact]
        public void GetStandardAccountsList_GetList_VerifyData()
        {
            // Arrange
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns("1000")
                .Returns("1000");

            // Act
            var financeList = _entityFinanceRepository.GetStandardAccountsList(_financeAcList, _entityFinanceList);


            // Assert
            Assert.NotEmpty(financeList);
            Assert.True(financeList.All(x => x.StandardAccountList.Count > 0));
            Assert.True(financeList.All(x => x.IsChartOfAccountMapped));
        }

        /// <summary>
        /// If drool returns null in response then set IsDataEmpty to true and verify add range EntityFinanceYearlyMapping never called
        /// </summary>
        [Fact]
        public void MapToStandardChartOfAccountsAsync_DMNDecisionNull_VerifyAddRangeAsyncEntityFinanceYearlyMappingNever()
        {
            //Arrange
            _entityFinanceList.First().FinancialStatement.Name = "";
            _entityFinanceList.Last().FinancialStatement.Name = "";
            //Act
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(_entityFinanceList.AsQueryable().BuildMock().Object);
            _entityFinanceRepository.MapToStandardChartOfAccountsAsync(Guid.NewGuid());

            //Assert
            _dataRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<List<EntityFinance>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<EntityFinanceYearlyMapping>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        #region Personal Finance

        /// <summary>
        /// Method to check if current user is trying to fetch finances of other user then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_CurrentUserAccessingOthersFinances_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            User user = new User { Id = Guid.NewGuid() };

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);

            //Act            

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC));
        }

        /// <summary>
        /// Method to check if invalid scope is provided in request then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(null)]
        [InlineData(StringConstant.FinancialReportCsv)]
        public async Task GetPersonalFinancesAsync_InvalidOrNullScopeProvidedInRequest_ThrowsInvalidParameterException(string scopeCsv)
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, scopeCsv, _currentUserAC));
        }

        /// <summary>
        /// Method to check if personal finance statement is not found in DB then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_PersonalFinanceStatementNotFoundInDB_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            FinancialStatement financialStatement = null;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC));
        }

        /// <summary>
        /// Method to check if there is no any personal finances available for given entity then it returns empty personal finance object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoFinancesFoundForEntity_ReturnsPersonalFinanceObjectWithEmptyAnswers()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>();
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(0);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.Null(actual.Accounts.First().Categories.First().Attributes.First().Answer);
        }

        /// <summary>
        /// Method to check if there are personal finances available with loan id null then it returns those finaces.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_FinancesWithNullLoanIdPresent_ReturnsThoseFinances()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotNull(actual.Accounts.First().Categories.First().Attributes.First().Answer);
        }

        /// <summary>
        /// Method to check if only details of data is required in scope then it returns only details and not summary.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_OnlyDetailsRequired_ReturnsDetailedData()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.Null(actual.Summary);
            Assert.NotEmpty(actual.Accounts);
        }

        /// <summary>
        /// Method to check if only summary of data is required in scope then it returns only summary and not detailed data.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_OnlySummaryRequired_ReturnsSummaryOfData()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceSummary, _currentUserAC);

            //Assert
            Assert.NotNull(actual.Summary);
            Assert.Empty(actual.Accounts);
        }

        /// <summary>
        /// Method to check if there is no any personal finances available for given entity and it is requested to fetch only summary
        /// then it throws an exception as no summary can be formed for empty answers.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_SummaryRequiredWhenNoFinancesFoundForEntity_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>();
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(0);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceSummary, _currentUserAC));
        }

        /// <summary>
        /// Method to check if any account is disabled while calculating summary then it doesn't add that attribute in response object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_DisabledAttributeInCalculatingSummary_ReturnsDataWithoutThatAttribute()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));
            accounts.First().IsEnabled = false;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceSummary, _currentUserAC);

            //Assert
            Assert.NotNull(actual.Summary);
            Assert.Empty(actual.Summary.Accounts);
        }

        /// <summary>
        /// Method to check if any category is disabled while calculating summary then it doesn't add that category in response object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_DisabledCategoryInCalculatingSummary_ReturnsDataWithoutThatCategory()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));
            accounts.First().PersonalFinanceCategories.First().IsEnabled = false;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceSummary, _currentUserAC);

            //Assert
            Assert.NotNull(actual.Summary);
            Assert.NotNull(actual.Summary.Accounts.First());
            Assert.Empty(actual.Summary.Accounts.First().Categories);
        }

        /// <summary>
        /// Method to check if the category has no any field as original amount while calculating summary then it keeps original amount as null.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoAnyOriginalAmountFieldInCalculatingSummary_ReturnsDataWithNullOriginalAmount()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.IsOriginal = false);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceSummary, _currentUserAC);

            //Assert
            Assert.NotNull(actual.Summary);
            Assert.NotNull(actual.Summary.Accounts.First());
            Assert.NotEmpty(actual.Summary.Accounts.First().Categories);
            Assert.Null(actual.Summary.Accounts.First().Categories.First().OriginalAmount);
        }

        /// <summary>
        /// Method to check if the category has no any field as current amount while calculating summary then it keeps current amount as null.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoAnyCurrentAmountFieldInCalculatingSummary_ReturnsDataWithNullCurrentAmount()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.IsCurrent = false);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceSummary, _currentUserAC);

            //Assert
            Assert.NotNull(actual.Summary);
            Assert.NotNull(actual.Summary.Accounts.First());
            Assert.NotEmpty(actual.Summary.Accounts.First().Categories);
            Assert.Null(actual.Summary.Accounts.First().Categories.First().CurrentAmount);
        }

        /// <summary>
        /// Method to check if there are any original or current fields available whose type is not number then it doesn't include that field in counting original or current amount.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_OriginalAndCurrentFieldNotOfTypeNumber_ReturnsNullOriginalAndCurrentAmount()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.IsOriginal = true);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.IsCurrent = true);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceSummary, _currentUserAC);

            //Assert
            Assert.NotNull(actual.Summary);
            Assert.NotNull(actual.Summary.Accounts.First());
            Assert.NotEmpty(actual.Summary.Accounts.First().Categories);
            Assert.Null(actual.Summary.Accounts.First().Categories.First().OriginalAmount);
            Assert.Null(actual.Summary.Accounts.First().Categories.First().CurrentAmount);
        }

        /// <summary>
        /// Method to check if there are no any disabled account or category and also the current and original fields available then it returns fully filled object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoAnyDisabledAccountCategoryAndOriginalCurrentAmountFieldAvailableInCalculatingSummary_ReturnsFullyFilledObject()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.Clear());
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.Attribute.FieldType = PersonalFinanceAttributeFieldType.Number);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.IsOriginal = true);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.IsCurrent = true);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.Answer = "50000"));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceSummary, _currentUserAC);

            //Assert
            Assert.NotNull(actual.Summary);
            Assert.NotNull(actual.Summary.Accounts.First());
            Assert.NotEmpty(actual.Summary.Accounts.First().Categories);
        }

        /// <summary>
        /// Method to check if there is entity finance present for given entity but no responses are there in DB for it and it is requested to fetch only summary
        /// then it throws an exception as no responses are there and no summary can be formed.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_SummaryRequiredWhenNoResponsesFoundForEntityFinance_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceSummary, _currentUserAC));
        }

        /// <summary>
        /// Method to check if both summary and details of data are required in scope then it returns both.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_BothSummaryAndDetailsRequired_ReturnsDetailedDataWithSummary()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetailsSummary, _currentUserAC);

            //Assert
            Assert.NotNull(actual.Summary);
            Assert.NotEmpty(actual.Accounts);
        }

        /// <summary>
        /// Method to check if there are an entry of entity finance but no responses of finance exists in DB then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoResponseInDBForEntityFinanceEntry_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC));
        }

        /// <summary>
        /// Method to check if there no entity finances with loan id null but there are finances available which are previously saved.
        /// Then it saves and returns copy of the latest versioned data with loan id null.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_OnlyPreviouslySavedFinancesAvailable_SavesAndReturnsLatestVersionedDataWithNullLoanId()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            Guid entityFinanceId = Guid.NewGuid();
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var personalFinanceResponses = new List<PersonalFinanceResponse>();
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = entityFinanceId,
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = personalFinanceResponses,
                    LoanApplicationId = Guid.NewGuid(),
                    Version = Guid.NewGuid(),
                    SurrogateId = 1
                }
            };
            var newlyAddedFinances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>()
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => personalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => personalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.SetupSequence(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(finances.AsQueryable().BuildMock().Object)
                .Returns(newlyAddedFinances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceResponse>(It.IsAny<Expression<Func<PersonalFinanceResponse, bool>>>())).Returns(personalFinanceResponses.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.DetachEntities(It.IsAny<IQueryable<EntityFinance>>(), true)).Returns(newlyAddedFinances);
            _dataRepositoryMock.SetupSequence(x => x.DetachEntities(It.IsAny<IQueryable<PersonalFinanceResponse>>(), true)).Returns(personalFinanceResponses);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(null), Times.Once);
            Assert.NotEmpty(actual.Accounts);
        }

        /// <summary>
        /// Method to check if any of the account is kept disabled then it doesn't add that account in response.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_DisabledAccount_ReturnsDataWithoutThatAccount()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().IsEnabled = false;

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotNull(actual);
            Assert.Empty(actual.Accounts);
        }

        /// <summary>
        /// Method to check if any of the category is kept disabled then it doesn't add that category in response.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_DisabledCategory_ReturnsDataWithoutThatCategory()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));
            accounts.First().PersonalFinanceCategories.First().IsEnabled = false;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.Accounts);
            Assert.Empty(actual.Accounts.First().Categories);
        }

        /// <summary>
        /// Method to check if the category has no any child categories then it returns empty list.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoChildCategories_ReturnsDataWithEmptyChildCategoryList()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.Clear();
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.Accounts);
            Assert.NotEmpty(actual.Accounts.First().Categories);
            Assert.Null(actual.Accounts.First().Categories.First().ChildCategories);
        }

        /// <summary>
        /// Method to check if the category has no any parent attribute in its parent-child category mapping then it returns null parent attribute object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoParentAttributeForParentChildCategoryMapping_ReturnsDataWithEmptyParentAttribute()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.First().ParentAttributeCategoryMappingId = null;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.Accounts);
            Assert.NotEmpty(actual.Accounts.First().Categories);
            Assert.NotEmpty(actual.Accounts.First().Categories.First().ChildCategories);
            Assert.Null(actual.Accounts.First().Categories.First().ParentAttribute);
        }

        /// <summary>
        /// Method to check if the category has child category and parent attribute for its mapping then it returns fully filled object with them.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_ChildCategoryAndParentAttributeAvailable_ReturnsFullyFilledObject()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.Accounts);
            Assert.NotEmpty(actual.Accounts.First().Categories);
            Assert.NotEmpty(actual.Accounts.First().Categories.First().ChildCategories);
            Assert.NotNull(actual.Accounts.First().Categories.First().ParentAttribute);
        }

        /// <summary>
        /// Method to check if any of the attribute is kept disabled then it doesn't add that attribute in response.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_DisabledAttribute_ReturnsDataWithoutThatAttribute()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.First().Attribute.IsEnabled = false;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.Accounts);
            Assert.NotEmpty(actual.Accounts.First().Categories);
            Assert.Empty(actual.Accounts.First().Categories.First().Attributes);
        }

        /// <summary>
        /// Method to check if any of the attribute has no constant linked with it then it returns attribute with null constant object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoConstantLinked_ReturnsDataWithNullConstantObject()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.Accounts);
            Assert.NotEmpty(actual.Accounts.First().Categories);
            Assert.NotEmpty(actual.Accounts.First().Categories.First().Attributes);
            Assert.Null(actual.Accounts.First().Categories.First().Attributes.First().Constant.Options);
        }

        /// <summary>
        /// Method to check if attribute has no responses present for it then it returns the object without any responses mapped.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoResponsesForAttributeInDB_ReturnsDataWithoutResponses()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x =>
            {
                x.EntityFinanceId = Guid.NewGuid();
                x.Answer = null;
            }));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x =>
                    {
                        x.EntityFinanceId = Guid.NewGuid();
                        x.Answer = null;
                    })));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.Accounts);
            Assert.NotEmpty(actual.Accounts.First().Categories);
            Assert.NotEmpty(actual.Accounts.First().Categories.First().Attributes);
            Assert.Null(actual.Accounts.First().Categories.First().Attributes.First().Answer);
        }

        /// <summary>
        /// Method to check if there is no any personal finances available and no any child attributes exist for an attribute then it returns empty object without response and empty child attribute list.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoFinancesAndNoChildAttributesFoundForEntity_ReturnsPersonalFinanceObjectWithEmptyAnswersAndChildAttributeList()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>();
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(0);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.First().ChildAttributeCategoryMappings.Clear();

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.Null(actual.Accounts.First().Categories.First().Attributes.First().Answer);

        }

        /// <summary>
        /// Method to check if any of the attribute (with responses) has no child attributes then it returns empty child attribute list.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_NoChildAttributes_ReturnsDataWithEmptyChildAttributeList()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.First().ChildAttributeCategoryMappings.Clear();

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.Accounts);
            Assert.NotEmpty(actual.Accounts.First().Categories);
            Assert.NotEmpty(actual.Accounts.First().Categories.First().Attributes);
            Assert.NotNull(actual.Accounts.First().Categories.First().Attributes.First().Answer);

        }

        /// <summary>
        /// Method to check if the enabled attribute has child attribute and constant linked along with its answer then it returns fully filled object with them.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPersonalFinancesAsync_EnalbledAttributeWithConstantAndChildAttributeAvailableWithResponse_ReturnsFullyFilledObject()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            User user = new User { Id = Guid.NewGuid() };
            var financialStatement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var finances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    FinancialStatementId = financialStatement.Id,
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    LoanApplicationId = null
                }
            };
            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = finances.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => finances.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<User>(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(financialStatement);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(finances.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.GetPersonalFinancesAsync(entityId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotEmpty(actual.Accounts);
            Assert.NotEmpty(actual.Accounts.First().Categories);
            Assert.NotEmpty(actual.Accounts.First().Categories.First().Attributes);
            Assert.NotNull(actual.Accounts.First().Categories.First().Attributes.First().Answer);
            Assert.NotEmpty(actual.Accounts.First().Categories.First().Attributes.First().ChildAttributeSets);
            Assert.NotNull(actual.Accounts.First().Categories.First().Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Constant);
        }

        /// <summary>
        /// Method to check if current user is trying to save finances of other user then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_CurrentUserAccessingDifferentUserData_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();

            //Act            

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, new PersonalFinanceCategoryAC(), _currentUserAC));
        }

        /// <summary>
        /// Method to check if personal finance statement is not found in DB or user id is sent empty then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public async Task SavePersonalFinancesAsync_PersonalFinanceStatementNotFoundInDBOrEmptyEntityId_ThrowsInvalidParameterException(bool isStatementAvailable, bool isEntityIdEmpty)
        {
            //Arrange
            _currentUserAC.Id = Guid.Empty;
            Guid entityId = isEntityIdEmpty ? Guid.Empty : _currentUserAC.Id;
            FinancialStatement statement = isStatementAvailable ? new FinancialStatement() : null;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);

            //Act          

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, new PersonalFinanceCategoryAC(), _currentUserAC));
        }

        /// <summary>
        /// Method to check if the category provided in the request doesn't exist in DB then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_RequestedCategoryNotExists_ThrowsInvalidParameterException()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            PersonalFinanceCategoryAC categoryAC = new PersonalFinanceCategoryAC { Id = Guid.NewGuid() };
            PersonalFinanceCategory category = null;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC));
        }

        /// <summary>
        /// Method to check if there are no any attributes provided in request then it doesn't perform any operation.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_NoAttributesToAdd_PerformsNoOperations()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            PersonalFinanceCategoryAC categoryAC = new PersonalFinanceCategoryAC { Id = Guid.NewGuid(), Attributes = new List<PersonalFinanceAttributeAC>() };
            PersonalFinanceCategory category = new PersonalFinanceCategory { Id = Guid.NewGuid() };

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);

            //Act
            await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PersonalFinanceResponse>()), Times.Never);
        }

        /// <summary>
        /// Method to check if any of the attribute provided in the request is not present in the DB then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_AttributeDoesNotExistInDB_ThrowsInvalidParameterException()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            var statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var categoryAC = GetPersonalFinanceCategoryACObjectToSave();
            var category = new PersonalFinanceCategory { Id = Guid.NewGuid() };
            var attributesInDB = new List<PersonalFinanceAttribute> { new PersonalFinanceAttribute() };

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttribute>(It.IsAny<Expression<Func<PersonalFinanceAttribute, bool>>>())).Returns(attributesInDB.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidParameterException>(async () => await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC));
        }

        /// <summary>
        /// Method to check if no entity finance is available in DB for given entity id then it performs an add operation of both entity finance and its responses.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_NoEntityFinanceForGivenEntity_PerformsAddOperationOfBothEntityFinanceAndResponses()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            var statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var categoryAC = GetPersonalFinanceCategoryACObjectToSave();
            var category = new PersonalFinanceCategory { Id = Guid.NewGuid() };
            var attributesInDB = new List<PersonalFinanceAttribute> { new PersonalFinanceAttribute(), new PersonalFinanceAttribute() };
            var categoryAttributeMappings = new List<PersonalFinanceAttributeCategoryMapping>
            {
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().Id,
                    CategoryId = categoryAC.Id
                },
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Id,
                    CategoryId = categoryAC.Id
                }
            };
            EntityFinance entityFinance = null;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttribute>(It.IsAny<Expression<Func<PersonalFinanceAttribute, bool>>>())).Returns(attributesInDB.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttributeCategoryMapping>(It.IsAny<Expression<Func<PersonalFinanceAttributeCategoryMapping, bool>>>())).Returns(categoryAttributeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).ReturnsAsync(entityFinance);

            //Act
            await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<PersonalFinanceResponse>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Method to check if entity finance exists for given entity id but there are no any entries exist for 
        /// the attributes' responses then it performs an add operation of only responses of those attributes.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_EntityFinanceExistsButNoResponsesExist_PerformsAddOperationOfResponsesOnly()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            var statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var categoryAC = GetPersonalFinanceCategoryACObjectToSave();
            var category = new PersonalFinanceCategory { Id = Guid.NewGuid() };
            var attributesInDB = new List<PersonalFinanceAttribute> { new PersonalFinanceAttribute(), new PersonalFinanceAttribute() };
            var categoryAttributeMappings = new List<PersonalFinanceAttributeCategoryMapping>
            {
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().Id,
                    CategoryId = categoryAC.Id
                },
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Id,
                    CategoryId = categoryAC.Id
                }
            };
            EntityFinance entityFinance = new EntityFinance { EntityId = entityId };
            var existingResponses = new List<PersonalFinanceResponse>();

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttribute>(It.IsAny<Expression<Func<PersonalFinanceAttribute, bool>>>())).Returns(attributesInDB.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttributeCategoryMapping>(It.IsAny<Expression<Func<PersonalFinanceAttributeCategoryMapping, bool>>>())).Returns(categoryAttributeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).ReturnsAsync(entityFinance);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceResponse>(It.IsAny<Expression<Func<PersonalFinanceResponse, bool>>>())).Returns(existingResponses.AsQueryable().BuildMock().Object);

            //Act
            await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<PersonalFinanceResponse>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Method to check if there are N level of hierarchy of children attributes then it performs an add operation of all attributes after fetching them recursively.
        /// (Tested for 3 level hierarchy)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_NLevelChildrenAvailable_PerformsAddOperationAfterFetchingRecursively()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            var statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var categoryAC = GetPersonalFinanceCategoryACObjectToSave();
            categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().ChildAttributeSets = new List<PersonalFinanceOrderedAttributeAC>
            {
                new PersonalFinanceOrderedAttributeAC
                {
                    Order = 1,
                    ChildAttributes = new List<PersonalFinanceAttributeAC>
                    {
                        new PersonalFinanceAttributeAC
                        {
                            Id = Guid.NewGuid(),
                            Order = 1,
                            Answer = "90909",
                            Text = "What is the amount of attribute last?",
                            Constant = null,
                            FieldType = PersonalFinanceAttributeFieldType.Number,
                            ChildAttributeSets = new List<PersonalFinanceOrderedAttributeAC>()
                        }
                    }
                }
            };
            var category = new PersonalFinanceCategory { Id = Guid.NewGuid() };
            var attributesInDB = new List<PersonalFinanceAttribute> { new PersonalFinanceAttribute(), new PersonalFinanceAttribute(), new PersonalFinanceAttribute() };
            var categoryAttributeMappings = new List<PersonalFinanceAttributeCategoryMapping>
            {
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().Id,
                    CategoryId = categoryAC.Id
                },
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Id,
                    CategoryId = categoryAC.Id
                },
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().ChildAttributeSets.First().ChildAttributes.First().Id,
                    CategoryId = categoryAC.Id
                }
            };
            EntityFinance entityFinance = new EntityFinance { EntityId = entityId };
            var existingResponses = new List<PersonalFinanceResponse>();

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttribute>(It.IsAny<Expression<Func<PersonalFinanceAttribute, bool>>>())).Returns(attributesInDB.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttributeCategoryMapping>(It.IsAny<Expression<Func<PersonalFinanceAttributeCategoryMapping, bool>>>())).Returns(categoryAttributeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).ReturnsAsync(entityFinance);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceResponse>(It.IsAny<Expression<Func<PersonalFinanceResponse, bool>>>())).Returns(existingResponses.AsQueryable().BuildMock().Object);

            //Act
            await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<PersonalFinanceResponse>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Method to check if entity finance exists for given entity id and responses are also exist in DB for it
        /// but no any response for given category is there in DB then it performs an add operation of only responses of those attributes.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_EntityFinanceExistsButNoResponsesExistForGivenCategory_PerformsAddOperationOfResponsesOnly()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            var statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var categoryAC = GetPersonalFinanceCategoryACObjectToSave();
            var category = new PersonalFinanceCategory { Id = Guid.NewGuid() };
            var attributesInDB = new List<PersonalFinanceAttribute> { new PersonalFinanceAttribute(), new PersonalFinanceAttribute() };
            var categoryAttributeMappings = new List<PersonalFinanceAttributeCategoryMapping>
            {
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().Id,
                    CategoryId = categoryAC.Id
                },
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Id,
                    CategoryId = categoryAC.Id
                }
            };
            EntityFinance entityFinance = new EntityFinance { EntityId = entityId };
            var existingResponses = new List<PersonalFinanceResponse> {
                new PersonalFinanceResponse
                {
                    PersonalFinanceAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                    {
                        CategoryId = Guid.NewGuid()
                    }
                }
            };

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttribute>(It.IsAny<Expression<Func<PersonalFinanceAttribute, bool>>>())).Returns(attributesInDB.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttributeCategoryMapping>(It.IsAny<Expression<Func<PersonalFinanceAttributeCategoryMapping, bool>>>())).Returns(categoryAttributeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).ReturnsAsync(entityFinance);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceResponse>(It.IsAny<Expression<Func<PersonalFinanceResponse, bool>>>())).Returns(existingResponses.AsQueryable().BuildMock().Object);

            //Act
            await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<PersonalFinanceResponse>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Method to check if entity finance exists for given entity id and responses are also exist in DB, for it, for given category also,
        /// then it performs a remove operation to for all existing responses and an add operation of new attributes' responses provided in request.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_EntityFinanceExistsResponsesExistForGivenCategory_PerformsRemoveAndAddOperationOfExistingAndNewResponsesRespectively()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            var statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var categoryAC = GetPersonalFinanceCategoryACObjectToSave();
            var category = new PersonalFinanceCategory { Id = Guid.NewGuid() };
            var attributesInDB = new List<PersonalFinanceAttribute> { new PersonalFinanceAttribute(), new PersonalFinanceAttribute() };
            var categoryAttributeMappings = new List<PersonalFinanceAttributeCategoryMapping>
            {
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().Id,
                    CategoryId = categoryAC.Id
                },
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Id,
                    CategoryId = categoryAC.Id
                }
            };
            EntityFinance entityFinance = new EntityFinance { EntityId = entityId };
            var existingResponses = new List<PersonalFinanceResponse> {
                new PersonalFinanceResponse
                {
                    PersonalFinanceAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                    {
                        CategoryId = categoryAC.Id
                    }
                },
                new PersonalFinanceResponse
                {
                    PersonalFinanceAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                    {
                        CategoryId = categoryAC.Id
                    }
                }
            };

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttribute>(It.IsAny<Expression<Func<PersonalFinanceAttribute, bool>>>())).Returns(attributesInDB.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttributeCategoryMapping>(It.IsAny<Expression<Func<PersonalFinanceAttributeCategoryMapping, bool>>>())).Returns(categoryAttributeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).ReturnsAsync(entityFinance);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceResponse>(It.IsAny<Expression<Func<PersonalFinanceResponse, bool>>>())).Returns(existingResponses.AsQueryable().BuildMock().Object);

            //Act
            await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<List<PersonalFinanceResponse>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<PersonalFinanceResponse>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Method to check if attribute field type is address then check for address validatity and the performs add operation.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SavePersonalFinancesAsync_AttributeFieldTypeAddress_PerformsAddOperationAfterCheckingAddressValidity(bool isParentTypeAddress)
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            var statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var categoryAC = GetPersonalFinanceCategoryACObjectToSave();
            var addressAC = new AddressAC
            {
                PrimaryNumber = "86",
                StreetLine = "Frontage",
                City = "Belmont",
                StateAbbreviation = "MA",
                ZipCode = "22001",
                IntegratedServiceConfigurationId = Guid.NewGuid()
            };
            if (isParentTypeAddress)
            {
                categoryAC.Attributes.First().FieldType = PersonalFinanceAttributeFieldType.Address;
                categoryAC.Attributes.First().Answer = JsonConvert.SerializeObject(addressAC);
            }
            else
            {
                categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().FieldType = PersonalFinanceAttributeFieldType.Address;
                categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Answer = JsonConvert.SerializeObject(addressAC);
            }
            var category = new PersonalFinanceCategory { Id = Guid.NewGuid() };
            var attributesInDB = new List<PersonalFinanceAttribute> { new PersonalFinanceAttribute(), new PersonalFinanceAttribute() };
            var categoryAttributeMappings = new List<PersonalFinanceAttributeCategoryMapping>
            {
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().Id,
                    CategoryId = categoryAC.Id
                },
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Id,
                    CategoryId = categoryAC.Id
                }
            };
            EntityFinance entityFinance = new EntityFinance { EntityId = entityId };
            var existingResponses = new List<PersonalFinanceResponse> {
                new PersonalFinanceResponse
                {
                    PersonalFinanceAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                    {
                        CategoryId = Guid.NewGuid()
                    }
                }
            };
            var integratedServiceConfiguration = new IntegratedServiceConfiguration
            {
                Id = addressAC.IntegratedServiceConfigurationId.Value,
                IsServiceEnabled = true,
                Name = StringConstant.SmartyStreets,
                ConfigurationJson = @"{ }"
            };
            Candidate validAddress = new Candidate()
            {
                Components = new SmartyStreets.USStreetApi.Components
                {
                    PrimaryNumber = "86",
                    StreetName = "Frontage",
                    CityName = "Belmont",
                    State = "MA",
                    ZipCode = "22001"
                }
            };

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttribute>(It.IsAny<Expression<Func<PersonalFinanceAttribute, bool>>>())).Returns(attributesInDB.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttributeCategoryMapping>(It.IsAny<Expression<Func<PersonalFinanceAttributeCategoryMapping, bool>>>())).Returns(categoryAttributeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).ReturnsAsync(entityFinance);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceResponse>(It.IsAny<Expression<Func<PersonalFinanceResponse, bool>>>())).Returns(existingResponses.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<IntegratedServiceConfiguration>(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>())).ReturnsAsync(integratedServiceConfiguration);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>())).Returns(validAddress).Verifiable();

            //Act
            await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC);

            //Assert
            _smartyStreetUtilityMock.Verify(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<PersonalFinanceResponse>>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        /// <summary>
        /// Method to check if attribute field type is address and invalid address is provided in the request then it throws a validation exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SavePersonalFinancesAsync_InvalidAddress_ThrowsValidationException()
        {
            //Arrange
            Guid entityId = _currentUserAC.Id;
            var statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.PersonalFinances };
            var categoryAC = GetPersonalFinanceCategoryACObjectToSave();
            categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().FieldType = PersonalFinanceAttributeFieldType.Address;
            var addressAC = new AddressAC
            {
                PrimaryNumber = "86",
                StreetLine = "Frontage",
                City = "Belmont",
                StateAbbreviation = "MA",
                ZipCode = "22001",
                IntegratedServiceConfigurationId = Guid.NewGuid()
            };
            categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Answer = JsonConvert.SerializeObject(addressAC);
            var category = new PersonalFinanceCategory { Id = Guid.NewGuid() };
            var attributesInDB = new List<PersonalFinanceAttribute> { new PersonalFinanceAttribute(), new PersonalFinanceAttribute() };
            var categoryAttributeMappings = new List<PersonalFinanceAttributeCategoryMapping>
            {
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().Id,
                    CategoryId = categoryAC.Id
                },
                new PersonalFinanceAttributeCategoryMapping
                {
                    Id = Guid.NewGuid(),
                    AttributeId = categoryAC.Attributes.First().ChildAttributeSets.First().ChildAttributes.First().Id,
                    CategoryId = categoryAC.Id
                }
            };
            EntityFinance entityFinance = new EntityFinance { EntityId = entityId };
            var existingResponses = new List<PersonalFinanceResponse> {
                new PersonalFinanceResponse
                {
                    PersonalFinanceAttributeCategoryMapping = new PersonalFinanceAttributeCategoryMapping
                    {
                        CategoryId = Guid.NewGuid()
                    }
                }
            };
            var integratedServiceConfiguration = new IntegratedServiceConfiguration
            {
                Id = addressAC.IntegratedServiceConfigurationId.Value,
                IsServiceEnabled = true,
                Name = StringConstant.SmartyStreets,
                ConfigurationJson = @"{ }"
            };
            Candidate validAddress = null;

            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<FinancialStatement>(It.IsAny<Expression<Func<FinancialStatement, bool>>>())).ReturnsAsync(statement);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<PersonalFinanceCategory>(It.IsAny<Expression<Func<PersonalFinanceCategory, bool>>>())).ReturnsAsync(category);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttribute>(It.IsAny<Expression<Func<PersonalFinanceAttribute, bool>>>())).Returns(attributesInDB.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAttributeCategoryMapping>(It.IsAny<Expression<Func<PersonalFinanceAttributeCategoryMapping, bool>>>())).Returns(categoryAttributeMappings.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).ReturnsAsync(entityFinance);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceResponse>(It.IsAny<Expression<Func<PersonalFinanceResponse, bool>>>())).Returns(existingResponses.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync<IntegratedServiceConfiguration>(It.IsAny<Expression<Func<IntegratedServiceConfiguration, bool>>>())).ReturnsAsync(integratedServiceConfiguration);
            _smartyStreetUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>())).Returns(validAddress);

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _entityFinanceRepository.SavePersonalFinancesAsync(entityId, categoryAC, _currentUserAC));
            _smartyStreetUtilityMock.Verify(x => x.GetValidatedAddress(It.IsAny<AddressAC>(), It.IsAny<string>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<List<PersonalFinanceResponse>>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<AuditLog>()), Times.Never);
        }

        /// <summary>
        /// Method to check if user has not access to loan then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchPersonalFinancesForApplicationAsync_UserDoesntHaveAccessToLoan_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid applicationId = Guid.NewGuid();
            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(async () => await _entityFinanceRepository.FetchPersonalFinancesForApplicationAsync(applicationId, StringConstant.PersonalFinanceDetails, _currentUserAC));
        }

        /// <summary>
        /// Method to check if loan is in draft status then fetch finances of shareholders where loan id is null.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchPersonalFinancesForApplicationAsync_LoanInDraftStatus_ReturnsPersonalFinancesWithLoanIdNull()
        {
            //Arrange
            Guid applicationId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = applicationId,
                    Status = LoanApplicationStatusType.Draft,
                    EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping>
                    {
                        new EntityLoanApplicationMapping
                        {
                            LoanApplicationId = applicationId,
                            Entity = new DomainModel.Models.EntityInfo.Entity
                            {
                                Id = Guid.NewGuid(),
                                PrimaryEntityRelationships = new List<EntityRelationshipMapping>
                                {
                                    new EntityRelationshipMapping
                                    {
                                        RelativeEntityId = entityId
                                    }
                                },
                                Company=new Company
                                {
                                    CompanyStructure=new CompanyStructure
                                    {
                                        Structure=StringConstant.Proprietorship
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var entityFinancesOfShareholders = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    LoanApplicationId = null,
                    Entity = new DomainModel.Models.EntityInfo.Entity
                    {
                        Id = entityId,
                        Type = EntityType.User,
                        User = new User
                        {
                            FirstName = "ABC"
                        }
                    },
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>()
                }
            };

            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = entityFinancesOfShareholders.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = entityFinancesOfShareholders.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = entityFinancesOfShareholders.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = entityFinancesOfShareholders.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => entityFinancesOfShareholders.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => entityFinancesOfShareholders.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinancesOfShareholders.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.FetchPersonalFinancesForApplicationAsync(applicationId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotNull(actual.First().PersonalFinance);
            Assert.NotEmpty(actual.First().PersonalFinance.Accounts);
        }

        /// <summary>
        /// Method to check if loan is not in draft status then fetch latest version of entity finances for given loan id.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchPersonalFinancesForApplicationAsync_LoanInNonDraftStatus_ReturnsLatestVersionOfFinances()
        {
            //Arrange
            Guid applicationId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = applicationId,
                    Status = LoanApplicationStatusType.Approved,
                    EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping>
                    {
                        new EntityLoanApplicationMapping
                        {
                            LoanApplicationId = applicationId,
                            Entity = new DomainModel.Models.EntityInfo.Entity
                            {
                                Id = Guid.NewGuid(),
                                PrimaryEntityRelationships = new List<EntityRelationshipMapping>
                                {
                                    new EntityRelationshipMapping
                                    {
                                        RelativeEntityId = entityId
                                    }
                                },
                                Company=new Company
                                {
                                    CompanyStructure=new CompanyStructure
                                    {
                                        Structure=StringConstant.Proprietorship
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var entityFinancesOfShareholders = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    LoanApplicationId = applicationId,
                    Entity = new DomainModel.Models.EntityInfo.Entity
                    {
                        Id = entityId,
                        Type = EntityType.User,
                        User = new User
                        {
                            FirstName = "ABC"
                        }
                    },
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    Version = Guid.NewGuid(),
                    SurrogateId = 2
                },
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    LoanApplicationId = applicationId,
                    Entity = new DomainModel.Models.EntityInfo.Entity
                    {
                        Id = entityId,
                        Type = EntityType.User,
                        User = new User
                        {
                            FirstName = "ABC"
                        }
                    },
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    Version = Guid.NewGuid(),
                    SurrogateId = 1
                }
            };

            var accounts = FetchPersonalFinanceAccountsWithAllMappings();
            accounts.RemoveAt(1);
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = entityFinancesOfShareholders.First().Id));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = entityFinancesOfShareholders.First().Id)));
            accounts.First().PersonalFinanceCategories.First().MappedAsParentCategoryMappings.ForEach(x => x.ChildCategory.PersonalFinanceAttributeCategoryMappings.ForEach(x => x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = entityFinancesOfShareholders.First().Id))));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => x.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = entityFinancesOfShareholders.First().Id)));

            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x => entityFinancesOfShareholders.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses));
            accounts.First().PersonalFinanceCategories.First().PersonalFinanceAttributeCategoryMappings.ForEach(x =>
                    x.ChildAttributeCategoryMappings.ForEach(x => entityFinancesOfShareholders.First().PersonalFinanceResponses.AddRange(x.PersonalFinanceResponses)));

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinancesOfShareholders.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>())).Returns(accounts.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.FetchPersonalFinancesForApplicationAsync(applicationId, StringConstant.PersonalFinanceDetails, _currentUserAC);

            //Assert
            Assert.NotNull(actual.First().PersonalFinance);
            Assert.NotEmpty(actual.First().PersonalFinance.Accounts);
        }

        /// <summary>
        /// Method to check if there are no any entity finances available for for given conditions then it throws an exception.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(LoanApplicationStatusType.Draft)]
        [InlineData(LoanApplicationStatusType.Approved)]
        public async Task FetchPersonalFinancesForApplicationAsync_EntityfinancesForShareholdersNotFound_ThrowsDataNotFoundException(LoanApplicationStatusType status)
        {
            //Arrange
            Guid applicationId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = applicationId,
                    Status = status,
                    EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping>
                    {
                        new EntityLoanApplicationMapping
                        {
                            LoanApplicationId = applicationId,
                            Entity = new DomainModel.Models.EntityInfo.Entity
                            {
                                Id = Guid.NewGuid(),
                                PrimaryEntityRelationships = new List<EntityRelationshipMapping>
                                {
                                    new EntityRelationshipMapping
                                    {
                                        RelativeEntityId = entityId
                                    }
                                },
                                Company=new Company
                                {
                                    CompanyStructure=new CompanyStructure
                                    {
                                        Structure=StringConstant.Partnership
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var entityFinancesOfShareholders = new List<EntityFinance>();

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinancesOfShareholders.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _entityFinanceRepository.FetchPersonalFinancesForApplicationAsync(applicationId, StringConstant.PersonalFinanceDetails, _currentUserAC));
        }

        /// <summary>
        /// Method to check if loan is not in draft status and only summary is required then it doesn't make a DB call to fetch entire list of accounts and all, 
        /// it just uses JSON information instead which is saved for all submitted loan applications.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task FetchPersonalFinancesForApplicationAsync_NonDraftStatusAndOnlySummaryRequired_CheckNoDBCallMadeToFetchAccountsAndAll()
        {
            //Arrange
            Guid applicationId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var loanApplications = new List<LoanApplication>
            {
                new LoanApplication
                {
                    Id = applicationId,
                    Status = LoanApplicationStatusType.Approved,
                    EntityLoanApplicationMappings = new List<EntityLoanApplicationMapping>
                    {
                        new EntityLoanApplicationMapping
                        {
                            LoanApplicationId = applicationId,
                            Entity = new DomainModel.Models.EntityInfo.Entity
                            {
                                Id = Guid.NewGuid(),
                                PrimaryEntityRelationships = new List<EntityRelationshipMapping>
                                {
                                    new EntityRelationshipMapping
                                    {
                                        RelativeEntityId = entityId
                                    }
                                },
                                Company=new Company
                                {
                                    CompanyStructure=new CompanyStructure
                                    {
                                        Structure=StringConstant.Proprietorship
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var entityFinancesOfShareholders = new List<EntityFinance>
            {
                new EntityFinance
                {
                    Id = Guid.NewGuid(),
                    EntityId = entityId,
                    LoanApplicationId = applicationId,
                    Entity = new DomainModel.Models.EntityInfo.Entity
                    {
                        Id = entityId,
                        Type = EntityType.User,
                        User = new User
                        {
                            FirstName = "ABC"
                        }
                    },
                    FinancialInformationJson = JsonConvert.SerializeObject(new PersonalFinanceAC {
                        Summary = new PersonalFinanceSummaryAC(),
                        Accounts = new List<PersonalFinanceAccountAC> { new PersonalFinanceAccountAC() },
                        FinancialStatement = StringConstant.PersonalFinances
                    }),
                    PersonalFinanceResponses = new List<PersonalFinanceResponse>(),
                    Version = Guid.NewGuid(),
                    SurrogateId = 1
                }
            };

            _globalRepositoryMock.Setup(x => x.CheckUserLoanAccessAsync(It.IsAny<CurrentUserAC>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<LoanApplication>(It.IsAny<Expression<Func<LoanApplication, bool>>>())).Returns(loanApplications.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinancesOfShareholders.AsQueryable().BuildMock().Object);

            //Act
            var actual = await _entityFinanceRepository.FetchPersonalFinancesForApplicationAsync(applicationId, StringConstant.PersonalFinanceSummary, _currentUserAC);

            //Assert
            _dataRepositoryMock.Verify(x => x.Fetch<PersonalFinanceAccount>(It.IsAny<Expression<Func<PersonalFinanceAccount, bool>>>()), Times.Never);
            Assert.NotNull(actual.First().PersonalFinance);
            Assert.NotEmpty(actual.First().PersonalFinance.Accounts);
        }


        #endregion

        #endregion
    }
}
