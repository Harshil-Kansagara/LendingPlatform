using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.ApplicationClass.EntityInfo;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass.PayPal;
using LendingPlatform.Utils.ApplicationClass.Quickbooks;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using Square.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Stripe;
using Xunit;

namespace LendingPlatform.Repository.Test.EntityInfo
{
    [Collection("Register Dependency")]
    public class FinancialInfoRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly IFinancialInfoRepository _financialInfoRepository;
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IQuickbooksUtility> _quickbooksUtility;
        private readonly Mock<IGlobalRepository> _globalRepository;
        private readonly Mock<IXeroUtility> _xeroUtility;
        private readonly Mock<IPayPalUtility> _payPalUtility;
        private readonly Mock<ISquareUtility> _squareUtility;
        private readonly Mock<IStripeUtility> _stripeUtility;
        private readonly IMapper _mapper;
        private readonly User _loggedInUser;
        #endregion

        #region Constructor
        public FinancialInfoRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _globalRepository = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _financialInfoRepository = bootstrap.ServiceProvider.GetService<IFinancialInfoRepository>();
            _configuration = bootstrap.ServiceProvider.GetService<Mock<IConfiguration>>();
            _quickbooksUtility = bootstrap.ServiceProvider.GetService<Mock<IQuickbooksUtility>>();
            _xeroUtility = bootstrap.ServiceProvider.GetService<Mock<IXeroUtility>>();
            _payPalUtility = bootstrap.ServiceProvider.GetService<Mock<IPayPalUtility>>();
            _squareUtility = bootstrap.ServiceProvider.GetService<Mock<ISquareUtility>>();
            _stripeUtility = bootstrap.ServiceProvider.GetService<Mock<IStripeUtility>>();
            _mapper = bootstrap.ServiceProvider.GetService<IMapper>();
            _quickbooksUtility.Reset();
            _dataRepositoryMock.Reset();
            _globalRepository.Reset();
            _xeroUtility.Reset();
            _loggedInUser = new User() { Id = Guid.NewGuid() };
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Get SearchInvoicesResponse object (Square)
        /// </summary>
        /// <returns>Object of SearchInvoicesResponse</returns>
        private SearchInvoicesResponse GetSearchInvoicesResponse()
        {
            return new SearchInvoicesResponse(new List<Square.Models.Invoice>()
            {
                new Square.Models.Invoice(id:"sds", version:2,locationId:"dsfsdf",orderId:"23",new InvoiceRecipient(customerId:"021",givenName:"Arjunsinh",familyName:"jadeja",emailAddress:"arjun@promactinfo.com",companyName:"titan",phoneNumber:"56454545545"),new List<InvoicePaymentRequest>(){ new InvoicePaymentRequest(computedAmountMoney: new Money(amount:500,currency:"USD") { }) { } },invoiceNumber:"01",title:"computer",description:"great",scheduledAt:"2020-08-30T10:00:00Z",createdAt:"2020-08-30T10:00:00Z",updatedAt:"2020-08-30T10:00:00Z" )
            });
        }

        /// <summary>
        /// Get list of InvoiceAC object
        /// </summary>
        /// <returns></returns>
        private List<InvoiceAC> GetSquareInvoices()
        {
            return new List<InvoiceAC>
                {
                    new InvoiceAC
                    {
                        Id = "123",
                        Status = "SENT",
                        InvoiceNumber = "000004",
                        InvoiceDate = "2020-07-15",
                        InvoiceCreateDateTime = "2020-08-11T02:49:51Z",
                        InvoicerEmail = "abc@def.com",
                        RecipientEmail = "pqr@xyz.com",
                        CurrencyCode = "INR",
                        TotalAmount = "230"
                    },
                    new InvoiceAC
                    {
                        Id = "123",
                        Status = "SENT",
                        InvoiceNumber = "001",
                        InvoiceDate = "2020-07-15",
                        InvoiceCreateDateTime = "2020-08-11T02:49:51Z",
                        InvoicerEmail = "abc@def.com",
                        RecipientEmail = "pqr@xyz.com",
                        CurrencyCode = "INR",
                        TotalAmount = "230"
                    },
                    new InvoiceAC
                    {
                        Id = "123",
                        Status = "SENT",
                        InvoiceNumber = "001",
                        InvoiceDate = "2020-07-15",
                        InvoiceCreateDateTime = "2020-08-11T02:49:51Z",
                        InvoicerEmail = "abc@def.com",
                        RecipientEmail = "pqr@xyz.com",
                        CurrencyCode = "INR",
                        TotalAmount = "230"
                    },
                    new InvoiceAC
                    {
                        Id = "123",
                        Status = "SENT",
                        InvoiceNumber = "001",
                        InvoiceDate = "2020-07-15",
                        InvoiceCreateDateTime = "2020-08-11T02:49:51Z",
                        InvoicerEmail = "abc@def.com",
                        RecipientEmail = "pqr@xyz.com",
                        CurrencyCode = "INR",
                        TotalAmount = "230"
                    }
                };
        }

        /// <summary>
        /// Get account types based on the statement name.
        /// </summary>
        /// <param name="statementName"></param>
        /// <returns></returns>
        private List<FinancialAccountType> GetFinancialAccountTypes(string statementName)
        {
            var financialStatementList = new List<FinancialStatement>
            {
                new FinancialStatement
                {
                    Id=Guid.NewGuid(),
                    Name=statementName
                }
            };
            List<string> incomestatementAccountTypes = new List<string> { "Income", "Expense", "Cost of goods" };
            List<string> balanceSheetAccountTypes = new List<string> { "Assets", "Liabilities", "Equity" };
            var financialAccountType = new List<FinancialAccountType>();
            foreach (var accountType in statementName == StringConstant.IncomeStatement ? incomestatementAccountTypes : balanceSheetAccountTypes)
            {
                financialAccountType.Add(
                new FinancialAccountType
                {
                    Id = Guid.NewGuid(),
                    FinancialStatementId = financialStatementList.First().Id,
                    FinancialStatement = financialStatementList.First(),
                    Name = accountType
                });
            }
            return financialAccountType;
        }
        /// <summary>
        /// It returns list of financial years in the string.
        /// </summary>
        /// <returns></returns>
        private List<string> GetListOfLastNFinancialYears()
        {
            return new List<string>()
            {
                "Jan Dec 2017",
                "Jan Dec 2018",
                "Jan Dec 2019",
                "Jan Jun 2020"
            };
        }
        /// <summary>
        /// Prepared object for manual finance report.
        /// </summary>
        /// <param name="financialAccountTypes">list of account types eg. {Income,expense}</param>
        /// <param name="listOfPeriods">List of periods.</param>
        /// <returns></returns>
        private ManualFinanceReportAC GetManualFinanceReport(List<FinancialAccountType> financialAccountTypes, List<string> listOfPeriods)
        {
            ManualFinanceReportAC manualFinanceReport = new ManualFinanceReportAC
            {
                Rows = new List<ManualReportRowAC>()
            };

            // Add the date header.
            ManualReportRowAC headerRow = new ManualReportRowAC()
            {
                RowType = ManualRowType.Header,
                Cells = listOfPeriods
            };
            manualFinanceReport.Rows.Add(headerRow);

            // Add the standard account empty rows for n years.
            foreach (var accountType in financialAccountTypes)
            {
                // Add the standard values.
                var parentRow = new ManualReportRowAC()
                {
                    Id = accountType.Id,
                    Title = accountType.Name,
                    RowType = ManualRowType.Parent,
                    Cells = Enumerable.Repeat("150.00", listOfPeriods.Count).ToList()
                };
                var childRow = new ManualReportRowAC()
                {
                    Title = "child 1",
                    RowType = ManualRowType.Child,
                    Cells = Enumerable.Repeat("100.00", listOfPeriods.Count).ToList()
                };
                var subChildRow = new ManualReportRowAC()
                {
                    Title = "sub child 1",
                    RowType = ManualRowType.SubChild,
                    Cells = Enumerable.Repeat("50.00", listOfPeriods.Count).ToList()
                };
                childRow.Rows.Add(subChildRow);
                parentRow.Rows.Add(childRow);
                manualFinanceReport.Rows.Add(parentRow);
            }
            return manualFinanceReport;
        }
        /// <summary>
        /// Get entity finances for manual report.
        /// </summary>
        /// <param name="listOfPeriods">List of periods.</param>
        /// <returns></returns>
        private List<EntityFinance> GetManualEntityFinances(List<string> listOfPeriods)
        {
            Guid loanId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();

            List<EntityFinance> entityFinances = new List<EntityFinance>();
            EntityFinance entityFinance = new EntityFinance();
            List<string> financialStatementNames = new List<string>
            {
                StringConstant.IncomeStatement,
                StringConstant.BalanceSheet
            };
            foreach (var statementName in financialStatementNames)
            {
                // Get the Financial Account type based on the report name(financial statement name).
                List<FinancialAccountType> financialAccountTypes = GetFinancialAccountTypes(statementName);

                ManualFinanceReportAC manualFinanceReport = GetManualFinanceReport(financialAccountTypes, listOfPeriods);
                entityFinance.FinancialInformationJson = JsonConvert.SerializeObject(manualFinanceReport);
                entityFinance.FinancialInformationFrom = FinancialInformationFrom.Manual;
                entityFinance.LoanApplicationId = loanId;
                entityFinance.EntityId = entityId;
                entityFinance.Id = Guid.NewGuid();
                entityFinance.FinancialStatementId = financialAccountTypes[0].FinancialStatementId;
                entityFinance.CreatedByUserId = _loggedInUser.Id;
                entityFinance.FinancialStatement = new FinancialStatement()
                {
                    Name = statementName
                };
                // Add entity finance yearly mapping object for add an upload files.
                entityFinance.EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMapping>();
                foreach (var period in listOfPeriods)
                {
                    entityFinance.EntityFinanceYearlyMappings.Add(
                        new EntityFinanceYearlyMapping()
                        {
                            Period = period
                        });
                }
                entityFinances.Add(entityFinance);
            }
            return entityFinances;
        }

        /// <summary>
        /// Get InvoicesResponseJsonAC object.
        /// </summary>
        /// <returns>InvoicesResponseJsonAC object</returns>
        private InvoicesResponseJsonAC GetInvoicesResponseJsonACObject()
        {
            return new InvoicesResponseJsonAC
            {
                InvoicesResponseJson = "{\"total_pages\":1,\"total_items\":1,\"items\":[{\"id\":\"123\",\"status\":\"SENT\",\"detail\":{\"currency_code\":\"INR\",\"invoice_number\":\"001\",\"invoice_date\":\"2020-07-15\",\"payment_term\":{\"due_date\":\"2020-08-01\"},\"viewed_by_recipient\":false,\"group_draft\":false,\"metadata\":{\"create_time\":\"2020-08-11T02:49:51Z\"}},\"invoicer\":{\"email_address\":\"pqr@xyz.com\"},\"primary_recipients\":[{\"billing_info\":{\"email_address\":\"abc@def.com\"}}],\"amount\":{\"currency_code\":\"INR\",\"value\":\"230\"},\"due_amount\":{\"currency_code\":\"INR\",\"value\":\"72.80\"}}]}",
                InvoicesResponse = new InvoicesResponseAC
                {
                    TotalItems = 1,
                    TotalPages = 1,
                    Items = new List<InvoicesItemsAC>
                    {
                        new InvoicesItemsAC
                        {
                            Id = "123",
                            Status = "SENT",
                            Detail = new InvoiceDetailAC
                            {
                                InvoiceNumber = "001",
                                InvoiceDate = "2020-07-15",
                                Metadata = new InvoiceDetailMetadataAC { CreateTime = "2020-08-11T02:49:51Z" }
                            },
                            Invoicer = new InvoiceInvoicerAC { EmailAddress = "abc@def.com" },
                            PrimaryRecipients = new List<InvoicePrimaryRecipientsAC>
                            {
                                new InvoicePrimaryRecipientsAC { BillingInfo = new InvoicePrimaryRecipientsBillingInfoAC { EmailAddress = "pqr@def.com" } }
                            },
                            Amount = new InvoiceAmountAC { CurrencyCode = "INR", Value = "230" }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Get StripeList<Invoice> object.
        /// </summary>
        /// <returns>StripeList<Invoice> object</returns>
        private StripeList<Stripe.Invoice> GetStripeListOfTypeInvoiceObject()
        {
            return new StripeList<Stripe.Invoice>
            {
                Data = new List<Stripe.Invoice>
                {
                    new Stripe.Invoice
                    {
                        Id = "in_1HN0nTD6dRWPotWPHBmGTeC1",
                        Currency = "inr",
                        Total = 123500,
                        Created = new DateTime(),
                        Status = "paid",
                        CustomerEmail = "john@doe.com",
                        Number = "82699AB8-0001"
                    }
                }
            };
        }

        #endregion

        #region Manual
        #region Public methods

        #region NEW IMPLEMENTATION
        /// <summary>
        /// Check the valid object count.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDefaultEmptyManualReportAsync_PerformGet_AssertValidateToValidObjectCountAsync()
        {
            // Arrange
            Guid loanId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var financialAccountTypes = GetFinancialAccountTypes(StringConstant.IncomeStatement);
            var listOfYears = GetListOfLastNFinancialYears();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(listOfYears);
            _globalRepository.Setup(x => x.GetFinancialAccountTypeAsync(It.IsAny<string>(), false)).ReturnsAsync(financialAccountTypes);

            // Act
            var entityFinanceACs = await _financialInfoRepository.GetDefaultEmptyManualReportAsync(entityId, loanId);

            // Assert
            // entityFinanceACs returns always 2 because income statement and balancesheet.
            Assert.True(entityFinanceACs.Count == 2);
        }
        /// <summary>
        /// Check the valid entityid in the entity finances object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDefaultEmptyManualReportAsync_PerformGet_AssertValidateToValidEntityIdForObjectAsync()
        {
            // Arrange
            Guid loanId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var financialAccountTypes = GetFinancialAccountTypes(StringConstant.IncomeStatement);
            var listOfYears = GetListOfLastNFinancialYears();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(listOfYears);
            _globalRepository.Setup(x => x.GetFinancialAccountTypeAsync(It.IsAny<string>(), false)).ReturnsAsync(financialAccountTypes);

            // Act
            var entityFinanceACs = await _financialInfoRepository.GetDefaultEmptyManualReportAsync(entityId, loanId);

            // Assert
            // Check valid entityId in the entityFinanceACs.
            Assert.True(entityFinanceACs[0].EntityId == entityId);
        }
        /// <summary>
        /// Check the valid LoanApplicationId in the entity finances object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDefaultEmptyManualReportAsync_PerformGet_AssertValidateToValidLoanIdForObjectAsync()
        {
            // Arrange
            Guid loanId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var financialAccountTypes = GetFinancialAccountTypes(StringConstant.IncomeStatement);
            var listOfYears = GetListOfLastNFinancialYears();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(listOfYears);
            _globalRepository.Setup(x => x.GetFinancialAccountTypeAsync(It.IsAny<string>(), false)).ReturnsAsync(financialAccountTypes);

            // Act
            var entityFinanceACs = await _financialInfoRepository.GetDefaultEmptyManualReportAsync(entityId, loanId);

            // Assert
            // Check valid loanId in the entityFinanceACs.
            Assert.True(entityFinanceACs[0].LoanApplicationId == loanId);
        }
        /// <summary>
        /// Check list of parent account exist based on the financial account types.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDefaultEmptyManualReportAsync_PerformGet_AssertValidateToValidParentAccountOfJsonCountAsync()
        {
            // Arrange
            Guid loanId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var financialAccountTypes = GetFinancialAccountTypes(StringConstant.IncomeStatement);
            var listOfYears = GetListOfLastNFinancialYears();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(listOfYears);
            _globalRepository.Setup(x => x.GetFinancialAccountTypeAsync(It.IsAny<string>(), false)).ReturnsAsync(financialAccountTypes);

            // Act
            var entityFinanceACs = await _financialInfoRepository.GetDefaultEmptyManualReportAsync(entityId, loanId);
            ManualFinanceReportAC manualFinanceReportAC = JsonConvert.DeserializeObject<ManualFinanceReportAC>(entityFinanceACs[0].FinancialInformationJson);
            int totalParentCount = manualFinanceReportAC.Rows.Count(s => s.RowType == ManualRowType.Parent);

            // Assert
            // Check list of financial account types with the Json parent account count.
            Assert.Equal(financialAccountTypes.Count, totalParentCount);
        }
        /// <summary>
        /// Check EntityFinanceYearlyMappings count objects with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDefaultEmptyManualReportAsync_PerformGet_AssertValidateToValidEntityFinanceYearlyMappingObjectsAsync()
        {
            // Arrange
            Guid loanId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            var financialAccountTypes = GetFinancialAccountTypes(StringConstant.IncomeStatement);
            var listOfYears = GetListOfLastNFinancialYears();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(true)).Returns(listOfYears);
            _globalRepository.Setup(x => x.GetFinancialAccountTypeAsync(It.IsAny<string>(), false)).ReturnsAsync(financialAccountTypes);

            // Act
            var entityFinanceACs = await _financialInfoRepository.GetDefaultEmptyManualReportAsync(entityId, loanId);

            // Assert
            // Check list of years with the list of EntityFinanceYearlyMappings count.
            Assert.Equal(listOfYears.Count, entityFinanceACs[0].EntityFinanceYearlyMappings.Count);
        }

        /// <summary>
        /// Check perform add the manual finance report.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveCompanyManualFinancialInfoAsync_PerformAdd_AssertCallToDatabasesAsync()
        {
            // Arrange
            var listOfYears = GetListOfLastNFinancialYears();
            List<EntityFinance> entityFinances = GetManualEntityFinances(listOfYears);
            List<EntityFinanceAC> entityFinanceACs = _mapper.Map<List<EntityFinanceAC>>(entityFinances);
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(new List<EntityFinance>().AsQueryable());
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Verifiable();

            // Act
            await _financialInfoRepository.SaveCompanyManualFinancialInfoAsync(entityFinanceACs, _loggedInUser);

            // Assert
            // Check how many times of add operation perform in the EntityFinance table.
            _dataRepositoryMock.Verify(x => x.AddAsync<EntityFinance>(It.IsAny<EntityFinance>()), Times.Exactly(2));
        }
        /// <summary>
        /// Check perform update the manual finance report.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveCompanyManualFinancialInfoAsync_PerformUpdate_AssertCallToDatabases()
        {
            // Arrange
            var listOfYears = GetListOfLastNFinancialYears();
            List<EntityFinance> entityFinanceDbList = GetManualEntityFinances(listOfYears);
            List<EntityFinanceAC> entityFinances = _mapper.Map<List<EntityFinanceAC>>(entityFinanceDbList);
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinanceDbList.AsQueryable());
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Verifiable();

            // Act
            await _financialInfoRepository.SaveCompanyManualFinancialInfoAsync(entityFinances, _loggedInUser);

            // Assert
            // Check how many times of update operation perform in the EntityFinance table.
            _dataRepositoryMock.Verify(x => x.Update<EntityFinance>(It.IsAny<EntityFinance>()), Times.Exactly(2));
        }

        #endregion

        /// <summary>
        /// Test to add child financial information.
        /// </summary>
        [Fact]
        public async Task AddSubFinancialInfo_VerifyInfoAdded_AssertAddTestAsync()
        {
            // Arrange  
            var financialInfoList = new List<EntityFinanceAC>();
            financialInfoList.AddRange(GetEntityFinancialManualInformation());
            var financialInfo = new EntityFinanceAC()
            {
                EntityId = Guid.NewGuid(),
                FinancialStatementId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                {
                    new EntityFinanceYearlyMappingAC()
                    {
                        Id=Guid.NewGuid(),
                        EntityFinanceId=Guid.NewGuid(),
                        EntityFinanceStandardAccounts=new List<EntityFinanceStandardAccountsAC>()
                        {
                            new EntityFinanceStandardAccountsAC()
                            {
                                Id=Guid.NewGuid(),
                                Amount=2410,
                                EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                {
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=2222,
                                        Id=Guid.NewGuid(),
                                        SubAccountName="Abc"
                                    },
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=1735,
                                        Id=Guid.NewGuid(),
                                        EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                        {
                                            new EntityFinancialManualInformationAC()
                                            {
                                                Id=Guid.NewGuid(),
                                                SubAccountName="Sub",
                                                Amount=385676
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinanceAC, bool>>>()))
              .Returns(financialInfoList.AsQueryable().BuildMock().Object.Where(x => x.LoanApplicationId == financialInfo.LoanApplicationId));

            // Act
            await _financialInfoRepository.AddUpdateFinancialInformationManualAsync(financialInfo);

            // Assert            
            _dataRepositoryMock.Verify(mock => mock.AddAsync(It.IsAny<EntityFinance>()), Times.Once);
        }

        /// <summary>
        /// Test to delete child financial information.
        /// </summary>
        [Fact]
        public async Task DeleteSubFinancialInfo_VerifyInfoDeleted_AssertDeleteTestAsync()
        {
            // Arrange
            var financialInfoList = new List<EntityFinanceAC>();
            financialInfoList.AddRange(GetEntityFinancialManualInformation());
            var subFinancialInfo = new EntityFinanceAC()
            {
                EntityId = Guid.NewGuid(),
                FinancialStatementId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                {
                    new EntityFinanceYearlyMappingAC()
                    {
                        Id=Guid.NewGuid(),
                        EntityFinanceId=Guid.NewGuid(),
                        EntityFinanceStandardAccounts=new List<EntityFinanceStandardAccountsAC>()
                        {
                            new EntityFinanceStandardAccountsAC()
                            {
                                Id=Guid.NewGuid(),
                                Amount=2410,
                                EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                {
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=2222,
                                        Id=Guid.NewGuid(),
                                        SubAccountName="Abc"
                                    },
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=1735,
                                        Id=Guid.NewGuid(),
                                        EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                        {
                                            new EntityFinancialManualInformationAC()
                                            {
                                                Id=Guid.NewGuid(),
                                                SubAccountName="Sub",
                                                Amount=385676
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinanceAC, bool>>>()))
              .Returns(financialInfoList.AsQueryable().BuildMock().Object.Where(x => x.LoanApplicationId == subFinancialInfo.LoanApplicationId));

            // Act            
            await _financialInfoRepository.AddUpdateFinancialInformationManualAsync(subFinancialInfo);

            // Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Once);
        }

        /// <summary>
        /// Test to add financial information.
        /// </summary>
        [Fact]
        public async Task AddFinancialInfo_VerifyInfoAdded_AssertAddTestAsync()
        {
            // Arrange   
            var financialInfoList = new List<EntityFinanceAC>();
            financialInfoList.AddRange(GetEntityFinancialManualInformation());

            var financialInfo = new EntityFinanceAC()
            {
                Id = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                FinancialStatementId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                {
                    new EntityFinanceYearlyMappingAC()
                    {
                        Period="Jan - Dec 2019",
                        Id=Guid.NewGuid(),
                        EntityFinanceId=Guid.NewGuid(),
                        EntityFinanceStandardAccounts=new List<EntityFinanceStandardAccountsAC>()
                        {
                            new EntityFinanceStandardAccountsAC()
                            {
                                Id=Guid.NewGuid(),
                                Amount=2410,
                                EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                {
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=2222,
                                        Id=Guid.NewGuid(),
                                        SubAccountName="Abc"
                                    },
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=1735,
                                        Id=Guid.NewGuid(),
                                        EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                        {
                                            new EntityFinancialManualInformationAC()
                                            {
                                                Id=Guid.NewGuid(),
                                                SubAccountName="Sub",
                                                Amount=385676
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };


            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinanceAC, bool>>>()))
                .Returns(financialInfoList.AsQueryable().BuildMock().Object.Where(x => x.LoanApplicationId == financialInfo.LoanApplicationId));

            // Act
            await _financialInfoRepository.AddUpdateFinancialInformationManualAsync(financialInfo);

            // Assert            
            _dataRepositoryMock.Verify(mock => mock.AddAsync(It.IsAny<EntityFinance>()), Times.Once);
        }

        /// <summary>
        /// Test to delete financial information.
        /// </summary>
        [Fact]
        public async Task DeleteFinancialInfo_VerifyInfoDeleted_AssertDeleteTestAsync()
        {
            // Arrange
            var financialInfoList = new List<EntityFinanceAC>();
            financialInfoList.AddRange(GetEntityFinancialManualInformation());
            var financialInfo = new EntityFinanceAC()
            {
                EntityId = Guid.NewGuid(),
                FinancialStatementId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                {
                    new EntityFinanceYearlyMappingAC()
                    {
                        Id=Guid.NewGuid(),
                        EntityFinanceId=Guid.NewGuid(),
                        EntityFinanceStandardAccounts=new List<EntityFinanceStandardAccountsAC>()
                        {
                            new EntityFinanceStandardAccountsAC()
                            {
                                Id=Guid.NewGuid(),
                                Amount=2410,
                                EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                {
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=2222,
                                        Id=Guid.NewGuid(),
                                        SubAccountName="Abc"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinanceAC, bool>>>()))
               .Returns(financialInfoList.AsQueryable().BuildMock().Object.Where(x => x.LoanApplicationId == financialInfo.LoanApplicationId));

            // Act
            await _financialInfoRepository.AddUpdateFinancialInformationManualAsync(financialInfo);

            // Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<EntityFinance>()), Times.Once);
        }

        /// <summary>
        /// Test to add the same parent in same request format.
        /// </summary>
        [Fact]
        public async Task AddParentInSameRequestObject_VerifyFinancialInfoAdded_AssertAddTestAsync()
        {
            // Arrange  
            var financialInfoList = new List<EntityFinanceAC>();
            financialInfoList.AddRange(GetEntityFinancialManualInformation());
            var financialInfo = new EntityFinanceAC()
            {
                EntityId = Guid.NewGuid(),
                FinancialStatementId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                {
                    new EntityFinanceYearlyMappingAC()
                    {
                        Id=Guid.NewGuid(),
                        EntityFinanceId=Guid.NewGuid(),
                        EntityFinanceStandardAccounts=new List<EntityFinanceStandardAccountsAC>()
                        {
                            new EntityFinanceStandardAccountsAC()
                            {
                                Id=Guid.NewGuid(),
                                Amount=2410,
                                EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                {
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=2222,
                                        Id=Guid.NewGuid(),
                                        SubAccountName="Abc"
                                    },
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=1735,
                                        Id=Guid.NewGuid(),
                                        EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                        {
                                            new EntityFinancialManualInformationAC()
                                            {
                                                Id=Guid.NewGuid(),
                                                SubAccountName="Sub",
                                                Amount=385676
                                            }
                                        }
                                    }
                                }
                            },new EntityFinanceStandardAccountsAC()
                            {
                                Id=Guid.NewGuid(),
                                Amount=244565410,
                                EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                {
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=227522,
                                        Id=Guid.NewGuid(),
                                        SubAccountName="Abc"
                                    },
                                    new EntityFinancialManualInformationAC()
                                    {
                                        Amount=1735,
                                        Id=Guid.NewGuid(),
                                        EntityFinancialManualInformations=new List<EntityFinancialManualInformationAC>()
                                        {
                                            new EntityFinancialManualInformationAC()
                                            {
                                                Id=Guid.NewGuid(),
                                                SubAccountName="Sub",
                                                Amount=385676
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinanceAC, bool>>>()))
              .Returns(financialInfoList.AsQueryable().BuildMock().Object.Where(x => x.LoanApplicationId == financialInfo.LoanApplicationId));

            // Act
            await _financialInfoRepository.AddUpdateFinancialInformationManualAsync(financialInfo);

            // Assert            
            _dataRepositoryMock.Verify(mock => mock.AddAsync(It.IsAny<EntityFinance>()), Times.Once);
        }

        /// <summary>
        /// Test to get the financial manual information by loan id.
        /// </summary>
        [Fact]
        public async Task GetFinancialInfoByLoanIdManual_VerifyLoadId_AssertLoanIdTestAsync()
        {
            // Arrange            
            var expected = new List<EntityFinanceAC>()
            {
                new EntityFinanceAC
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    FinancialStatementId = Guid.NewGuid(),
                    LoanApplicationId = Guid.NewGuid(),
                    EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                    {
                        new EntityFinanceYearlyMappingAC()
                        {
                            Period = "Jan - Dec 2019",
                            Id = Guid.NewGuid(),
                            EntityFinanceId = Guid.NewGuid(),
                            EntityFinanceStandardAccounts = new List<EntityFinanceStandardAccountsAC>()
                            {
                                new EntityFinanceStandardAccountsAC()
                                {
                                    Id = Guid.NewGuid(),
                                    Amount = 2410,
                                    EntityFinancialManualInformations = new List<EntityFinancialManualInformationAC>()
                                    {
                                        new EntityFinancialManualInformationAC()
                                        {
                                            Amount = 2222,
                                            Id = Guid.NewGuid(),
                                            SubAccountName = "Abc"
                                        },
                                        new EntityFinancialManualInformationAC()
                                        {
                                            Amount = 1735,
                                            Id = Guid.NewGuid(),
                                            EntityFinancialManualInformations = new List<EntityFinancialManualInformationAC>()
                                            {
                                                new EntityFinancialManualInformationAC()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    SubAccountName = "Sub",
                                                    Amount = 385676
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new EntityFinanceAC
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    FinancialStatementId = Guid.NewGuid(),
                    LoanApplicationId = Guid.NewGuid(),
                    EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                    {
                        new EntityFinanceYearlyMappingAC()
                        {
                            Period = "Jan - Dec 2019",
                            Id = Guid.NewGuid(),
                            EntityFinanceId = Guid.NewGuid(),
                            EntityFinanceStandardAccounts = new List<EntityFinanceStandardAccountsAC>()
                            {
                                new EntityFinanceStandardAccountsAC()
                                {
                                    Id = Guid.NewGuid(),
                                    Amount = 2410,
                                    EntityFinancialManualInformations = new List<EntityFinancialManualInformationAC>()
                                    {
                                        new EntityFinancialManualInformationAC()
                                        {
                                            Amount = 2222,
                                            Id = Guid.NewGuid(),
                                            SubAccountName = "Abc"
                                        },
                                        new EntityFinancialManualInformationAC()
                                        {
                                            Amount = 1735,
                                            Id = Guid.NewGuid(),
                                            EntityFinancialManualInformations = new List<EntityFinancialManualInformationAC>()
                                            {
                                                new EntityFinancialManualInformationAC()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    SubAccountName = "Sub",
                                                    Amount = 385676
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinanceAC, bool>>>()))
                .Returns(expected.AsQueryable().BuildMock().Object.Where(x => x.LoanApplicationId == expected[0].LoanApplicationId));

            // Assert
            await Assert.ThrowsAsync<DataNotFoundException>(() => _financialInfoRepository.GetFinancialInfoByLoanIdManualAsync(expected[0].LoanApplicationId));
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get static entity financial manual information.
        /// </summary>
        /// <returns>List of entity financial manual information.</returns>
        private List<EntityFinanceAC> GetEntityFinancialManualInformation()
        {
            return new List<EntityFinanceAC>()
            {
                new EntityFinanceAC
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    FinancialStatementId = Guid.NewGuid(),
                    LoanApplicationId = Guid.NewGuid(),
                    EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                    {
                        new EntityFinanceYearlyMappingAC()
                        {
                            Period = "Jan - Dec 2019",
                            Id = Guid.NewGuid(),
                            EntityFinanceId = Guid.NewGuid(),
                            EntityFinanceStandardAccounts = new List<EntityFinanceStandardAccountsAC>()
                            {
                                new EntityFinanceStandardAccountsAC()
                                {
                                    Id = Guid.NewGuid(),
                                    Amount = 2410,
                                    EntityFinancialManualInformations = new List<EntityFinancialManualInformationAC>()
                                    {
                                        new EntityFinancialManualInformationAC()
                                        {
                                            Amount = 2222,
                                            Id = Guid.NewGuid(),
                                            SubAccountName = "Abc"
                                        },
                                        new EntityFinancialManualInformationAC()
                                        {
                                            Amount = 1735,
                                            Id = Guid.NewGuid(),
                                            EntityFinancialManualInformations = new List<EntityFinancialManualInformationAC>()
                                            {
                                                new EntityFinancialManualInformationAC()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    SubAccountName = "Sub",
                                                    Amount = 385676
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new EntityFinanceAC
                {
                    Id = Guid.NewGuid(),
                    EntityId = Guid.NewGuid(),
                    FinancialStatementId = Guid.NewGuid(),
                    LoanApplicationId = Guid.NewGuid(),
                    EntityFinanceYearlyMappings = new List<EntityFinanceYearlyMappingAC>()
                    {
                        new EntityFinanceYearlyMappingAC()
                        {
                            Period = "Jan - Dec 2019",
                            Id = Guid.NewGuid(),
                            EntityFinanceId = Guid.NewGuid(),
                            EntityFinanceStandardAccounts = new List<EntityFinanceStandardAccountsAC>()
                            {
                                new EntityFinanceStandardAccountsAC()
                                {
                                    Id = Guid.NewGuid(),
                                    Amount = 2410,
                                    EntityFinancialManualInformations = new List<EntityFinancialManualInformationAC>()
                                    {
                                        new EntityFinancialManualInformationAC()
                                        {
                                            Amount = 2222,
                                            Id = Guid.NewGuid(),
                                            SubAccountName = "Abc"
                                        },
                                        new EntityFinancialManualInformationAC()
                                        {
                                            Amount = 1735,
                                            Id = Guid.NewGuid(),
                                            EntityFinancialManualInformations = new List<EntityFinancialManualInformationAC>()
                                            {
                                                new EntityFinancialManualInformationAC()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    SubAccountName = "Sub",
                                                    Amount = 385676
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };
        }
        #endregion
        #endregion

        #region Quickbooks

        [Fact]
        public async Task GetFinancialInfoAsync_ValidObject_PerformsCallToFetchFinancialInfoInUtils()
        {
            //Arrange

            var currentUser = new User
            {
                Id = Guid.NewGuid()
            };
            var entityFinanceAC = new EntityFinanceAC
            {
                EntityId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid(),
                QuickbooksTokens = new QuickbooksTokenAC()
                {
                    BearerToken = "sometoken"
                },
                IsCallFromRedirect = true,
                UserId = currentUser.Id
            };

            var financialStatementList = new List<FinancialStatement>
            {
                new FinancialStatement
                {
                    Id=Guid.NewGuid(),
                    Name = StringConstant.IncomeStatement
                },
                new FinancialStatement
                {
                    Id=Guid.NewGuid(),
                    Name = StringConstant.BalanceSheet
                }
            };

            var quickbooksReportListAC = new List<QuickbooksReportAC>();

            var quickbooksReportAC = new QuickbooksReportAC
            {
                ReportName = "ProfitAndLoss"
            };
            quickbooksReportListAC.Add(quickbooksReportAC);
            var stringList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };

            List<EntityFinance> entityFinances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    EntityId = entityFinanceAC.EntityId,
                    FinancialStatement = financialStatementList.Find(s => s.Name == StringConstant.IncomeStatement)
                },
                new EntityFinance
                {
                    EntityId = entityFinanceAC.EntityId,
                    FinancialStatement = financialStatementList.Find(s => s.Name == StringConstant.BalanceSheet)
                }
            };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(entityFinanceAC.EntityId, currentUser.Id, true)).ReturnsAsync(true);
            _quickbooksUtility.Setup(x => x.FetchQuickbooksTokensAsync(It.IsAny<QuickbooksTokenAC>())).ReturnsAsync(entityFinanceAC.QuickbooksTokens);
            _quickbooksUtility.Setup(x => x.FetchFinancialInfoFromQuickbooks(It.IsAny<QuickbooksTokenAC>())).Returns(quickbooksReportListAC);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinances.AsQueryable());
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(false)).Returns(stringList);
            _globalRepository.Setup(x => x.GetFinancialStatementFromNameAsync(It.IsAny<string>())).ReturnsAsync(entityFinances[0].FinancialStatement);

            // Save financialinfo method mock 
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                  .Returns((Expression<Func<FinancialStatement, bool>> expression) => financialStatementList.AsQueryable().Where(expression).BuildMock().Object);

            //Act
            await _financialInfoRepository.GetFinancialInfoAsync(entityFinanceAC);

            //Assert
            _quickbooksUtility.Verify(x => x.FetchFinancialInfoFromQuickbooks(It.IsAny<QuickbooksTokenAC>()), Times.Once);

        }


        [Fact]
        public async Task GetFinancialInfoAsync_ValidObject_AssertReturnedData()
        {
            //Arrange
            var currentUser = new User
            {
                Id = Guid.NewGuid()
            };
            var entityFinanceAC = new EntityFinanceAC
            {
                EntityId = Guid.NewGuid(),
                IsCallFromRedirect = false,
                LoanApplicationId = Guid.NewGuid(),
                UserId = currentUser.Id
            };

            var entityFinanceYearlyMappings = new List<EntityFinanceYearlyMapping>()
            {
                new EntityFinanceYearlyMapping
                {
                    Period="Jan-Dec, 2018"
                }
            };
            var entityFinance = new EntityFinance
            {
                EntityId = entityFinanceAC.EntityId,
                LoanApplicationId = entityFinanceAC.EntityId,
                FinancialStatement = new FinancialStatement
                {
                    Name = StringConstant.IncomeStatement
                },
                EntityFinanceYearlyMappings = entityFinanceYearlyMappings

            };
            var financialStatementList = new List<FinancialStatement>
            {
                new FinancialStatement
                {
                    Id=Guid.NewGuid(),
                    Name = StringConstant.IncomeStatement
                },
                new FinancialStatement
                {
                    Id=Guid.NewGuid(),
                    Name = StringConstant.BalanceSheet
                }
            };
            var entityFinanceBalanceSheet = new EntityFinance
            {
                EntityId = entityFinanceAC.EntityId,
                LoanApplicationId = entityFinanceAC.EntityId,
                FinancialStatement = financialStatementList.Find(s => s.Name == StringConstant.BalanceSheet),
                EntityFinanceYearlyMappings = entityFinanceYearlyMappings
            };
            var stringList = new List<string>()
            {
                "Jan-Dec, 2018",
                "Jan-Dec, 2019",
                "Jan-Dec, 2020"
            };

            List<EntityFinance> entityFinances = new List<EntityFinance>();
            entityFinances.Add(entityFinance);
            entityFinances.Add(entityFinanceBalanceSheet);
            _globalRepository.Setup(x => x.GetFinancialStatementFromNameAsync(It.IsAny<string>())).ReturnsAsync(entityFinance.FinancialStatement);
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), true)).ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(entityFinances.AsQueryable());
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _configuration.Setup(x => x.GetSection("FinancialYear:EndMonth").Value).Returns("February");
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(false)).Returns(stringList);

            // Save financialinfo method mock 
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                  .Returns((Expression<Func<FinancialStatement, bool>> expression) => financialStatementList.AsQueryable().Where(expression).BuildMock().Object);

            //Act
            var reportData = await _financialInfoRepository.GetFinancialInfoAsync(entityFinanceAC);

            //Assert
            Assert.Equal(2, reportData.Count);
        }

        [Fact]
        public async Task SaveCompanyFinancialInfoFromQuickbooksAsync_PerformAdd_AssertCallToDatabases()
        {
            // Arrange
            string financialInformationJson = "{\"Header\":{\"Time\":\"2020-06-09\",\"ReportName\":\"ProfitAndLoss\",\"DateMacro\":null,\"ReportBasis\":0,\"StartPeriod\":\"2017 - 01 - 01\",\"EndPeriod\":\"2020 - 06 - 09\",\"SummarizeColumnsBy\":\"Year\",\"Currency\":\"USD\",\"Customer\":null,\"Vendor\":null,\"Employee\":null,\"AnyIntuitObject\":null,\"Class\":null,\"Department\":null,\"Option\":[{\"Name\":\"AccountingStandard\",\"Value\":\"GAAP\"},{\"Name\":\"NoReportData\",\"Value\":\"false\"}]},\"Columns\":[{\"ColTitle\":\"\",\"ColType\":\"Account\",\"MetaData\":[{\"Name\":\"ColKey\",\"Value\":\"account\"}],\"Columns\":null},{\"ColTitle\":\"Jan - Dec 2017\",\"ColType\":\"Money\",\"MetaData\":[{\"Name\":\"StartDate\",\"Value\":\"2017 - 01 - 01\"},{\"Name\":\"EndDate\",\"Value\":\"2017 - 12 - 31\"},{\"Name\":\"ColKey\",\"Value\":\"Jan - Dec 2017\"}],\"Columns\":null},{\"ColTitle\":\"Jan - Dec 2018\",\"ColType\":\"Money\",\"MetaData\":[{\"Name\":\"StartDate\",\"Value\":\"2018 - 01 - 01\"},{\"Name\":\"EndDate\",\"Value\":\"2018 - 12 - 31\"},{\"Name\":\"ColKey\",\"Value\":\"Jan - Dec 2018\"}],\"Columns\":null},{\"ColTitle\":\"Jan - Dec 2019\",\"ColType\":\"Money\",\"MetaData\":[{\"Name\":\"StartDate\",\"Value\":\"2019 - 01 - 01\"},{\"Name\":\"EndDate\",\"Value\":\"2019 - 12 - 31\"},{\"Name\":\"ColKey\",\"Value\":\"Jan - Dec 2019\"}],\"Columns\":null},{\"ColTitle\":\"Jan 1 - Jun 9, 2020\",\"ColType\":\"Money\",\"MetaData\":[{\"Name\":\"StartDate\",\"Value\":\"2020 - 01 - 01\"},{\"Name\":\"EndDate\",\"Value\":\"2020 - 06 - 09\"},{\"Name\":\"ColKey\",\"Value\":\"Jan 1 - Jun 9, 2020\"}],\"Columns\":null},{\"ColTitle\":\"Total\",\"ColType\":\"Money\",\"MetaData\":[{\"Name\":\"ColKey\",\"Value\":\"total\"}],\"Columns\":null}],\"Rows\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Income\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Design income\",\"id\":\"82\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2250.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2250.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Discounts given\",\"id\":\"86\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\" - 89.50\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\" - 89.50\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Landscaping Services\",\"id\":\"45\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"1477.50\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"1477.50\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Job Materials\",\"id\":\"46\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Fountains and Garden Lighting\",\"id\":\"48\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2246.50\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2246.50\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Plants and Soil\",\"id\":\"49\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2351.97\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2351.97\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Sprinklers and Drip Systems\",\"id\":\"50\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"138.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"138.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Job Materials\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"4736.47\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"4736.47\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Labor\",\"id\":\"51\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Installation\",\"id\":\"52\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"250.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"250.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Maintenance and Repair\",\"id\":\"53\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"50.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"50.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Labor\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"300.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"300.00\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Landscaping Services\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"6513.97\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"6513.97\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Pest Control Services\",\"id\":\"54\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"110.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"110.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Sales of Product Income\",\"id\":\"79\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"912.75\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"912.75\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Services\",\"id\":\"1\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"503.55\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"503.55\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Income\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"10200.77\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"10200.77\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":\"Income\"},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Cost of Goods Sold\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Cost of Goods Sold\",\"id\":\"80\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"405.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"405.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Cost of Goods Sold\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"405.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"405.00\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":\"COGS\"},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Gross Profit\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"9795.77\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"9795.77\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":\"GrossProfit\"},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Expenses\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Advertising\",\"id\":\"7\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"74.86\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"74.86\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Automobile\",\"id\":\"55\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"113.96\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"113.96\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Fuel\",\"id\":\"56\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"349.41\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"349.41\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Automobile\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"463.37\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"463.37\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Equipment Rental\",\"id\":\"29\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"112.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"112.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Insurance\",\"id\":\"11\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"241.23\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"241.23\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Job Expenses\",\"id\":\"58\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"155.07\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"155.07\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Job Materials\",\"id\":\"63\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Decks and Patios\",\"id\":\"64\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"234.04\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"234.04\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Plants and Soil\",\"id\":\"66\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"353.12\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"353.12\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Sprinklers and Drip Systems\",\"id\":\"67\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"215.66\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"215.66\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Job Materials\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"802.82\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"802.82\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Job Expenses\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"957.89\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"957.89\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Legal & Professional Fees\",\"id\":\"12\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"75.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"75.00\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Accounting\",\"id\":\"69\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"640.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"640.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Bookkeeper\",\"id\":\"70\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"55.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"55.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Lawyer\",\"id\":\"71\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"400.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"400.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Legal &Professional Fees\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"1170.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"1170.00\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Maintenance and Repair\",\"id\":\"72\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"185.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"185.00\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Equipment Repairs\",\"id\":\"75\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"755.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"755.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Maintenance and Repair\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"940.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"940.00\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Meals and Entertainment\",\"id\":\"13\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"28.49\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"28.49\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Office Expenses\",\"id\":\"15\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"18.08\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"18.08\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Rent or Lease\",\"id\":\"17\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"900.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"900.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Utilities\",\"id\":\"24\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Gas and Electric\",\"id\":\"76\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"200.53\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"200.53\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Telephone\",\"id\":\"77\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"130.86\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"130.86\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Utilities\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"331.39\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"331.39\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Expenses\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"5237.31\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"5237.31\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":\"Expenses\"},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Net Operating Income\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"4558.46\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"4558.46\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":\"NetOperatingIncome\"},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Other Expenses\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null}]},{\"Row\":[{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[[{\"Attributes\":null,\"value\":\"Miscellaneous\",\"id\":\"14\",\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2916.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2916.00\",\"id\":null,\"href\":null}]],\"type\":1,\"group\":null}]},{\"ColData\":[{\"Attributes\":null,\"value\":\"Total Other Expenses\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2916.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"2916.00\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":\"OtherExpenses\"},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Net Other Income\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\" - 2916.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\" - 2916.00\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":\"NetOtherIncome\"},{\"id\":null,\"parentId\":null,\"AnyIntuitObjects\":[{\"ColData\":[{\"Attributes\":null,\"value\":\"Net Income\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"0.00\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"1642.46\",\"id\":null,\"href\":null},{\"Attributes\":null,\"value\":\"1642.46\",\"id\":null,\"href\":null}]}],\"type\":0,\"group\":\"NetIncome\"}]}";

            _dataRepositoryMock.Setup(x => x.Fetch<EntityFinance>(It.IsAny<Expression<Func<EntityFinance, bool>>>())).Returns(new List<EntityFinance>().AsQueryable());

            var entityFinances = new List<EntityFinanceAC>{
                new EntityFinanceAC
                {
                    EntityId = Guid.NewGuid(),
                    FinancialInformationJson = financialInformationJson
                },new EntityFinanceAC
                {
                    EntityId = Guid.NewGuid(),
                    FinancialInformationJson = financialInformationJson
                }
            };

            var user = new User
            {
                Id = Guid.NewGuid()
            };
            var financialStatementList = new List<FinancialStatement>
            {
                new FinancialStatement
                {
                    Id=Guid.NewGuid(),
                    Name=StringConstant.ProfitAndLossStatement
                }
            };

            _globalRepository.Setup(x => x.GetFinancialStatementFromNameAsync(It.IsAny<string>())).ReturnsAsync(financialStatementList.First());
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);

            var financialAccountType = new List<FinancialAccountType>
            {
                new FinancialAccountType
                {
                    Id=Guid.NewGuid(),
                    FinancialStatementId=financialStatementList.First().Id,
                    FinancialStatement=financialStatementList.First(),
                    Name="Income",
                    QuickbooksAccountName = StringConstant.QuickbooksTotalIncome
                },
                new FinancialAccountType
                {
                    Id=Guid.NewGuid(),
                    FinancialStatementId=financialStatementList.First().Id,
                    FinancialStatement=financialStatementList.First(),
                    Name="Expense",
                    QuickbooksAccountName = StringConstant.QuickbooksTotalExpense
                },
                new FinancialAccountType
                {
                    Id=Guid.NewGuid(),
                    FinancialStatementId=financialStatementList.First().Id,
                    FinancialStatement=financialStatementList.First(),
                    Name="Cogs",
                    QuickbooksAccountName = StringConstant.QuickbooksTotalCogs
                }

            };



            var reportData = JsonConvert.DeserializeObject<Intuit.Ipp.Data.Report>(financialInformationJson);

            var quickbooksReportAC = new QuickbooksReportAC
            {
                QuickbooksReport = reportData
            };

            _quickbooksUtility.Setup(x => x.DeserializeReportJson(financialInformationJson)).Returns(quickbooksReportAC);
            List<QuickbooksAccountAC> quickbooksAccounts = new List<QuickbooksAccountAC>();
            IEnumerable<string> accountNames = new List<string> { StringConstant.QuickbooksTotalCogs, StringConstant.QuickbooksTotalExpense };
            _quickbooksUtility.Setup(x => x.GetAccountsWithAmountFromReportRows(null, accountNames, quickbooksAccounts)).Returns(quickbooksAccounts);
            _globalRepository.Setup(x => x.GetFinancialAccountTypeAsync(It.IsAny<string>(), true)).ReturnsAsync(financialAccountType);
            _dataRepositoryMock.Setup(x => x.BeginTransactionAsync()).Verifiable();

            // Act
            await _financialInfoRepository.SaveCompanyFinancialInfoAsync(entityFinances);
            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync<EntityFinance>(It.IsAny<EntityFinance>()), Times.Exactly(2));
        }


        #endregion

        #region Invoices

        /// <summary>
        /// Test case to check if user is not linked with given entity(company) then throws InvalidResourceAccess exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPayPalAuthorizationUrlAsync_UnauthorizedUser_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _financialInfoRepository.GetPayPalAuthorizationUrlAsync(entityId, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if any one of the configurations is null then throws ConfigurationNotFound exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPayPalAuthorizationUrlAsync_ConfigurationNotFound_ThrowsConfigurationNotFoundException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            string clientId = "123";
            string authorizationUrl = "abc.com/def?";
            string scopes = null;
            string redirectUri = "pqr.com/xyz";
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns(authorizationUrl).Returns(clientId).Returns(scopes).Returns(redirectUri);

            //Act

            //Assert
            await Assert.ThrowsAsync<ConfigurationNotFoundException>(() => _financialInfoRepository.GetPayPalAuthorizationUrlAsync(entityId, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if user is authorized and all the configurations are found then 
        /// returns an object of InvoiceRequestParametersAC.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPayPalAuthorizationUrlAsync_AuthorizedUserConfigurationFound_ReturnsAuthorizationUrl()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            string clientId = "123";
            string authorizationUrl = "abc.com/def?";
            string scopes = "john";
            string redirectUri = "pqr.com/xyz";
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns(authorizationUrl).Returns(clientId).Returns(scopes).Returns(redirectUri)
                .Returns(authorizationUrl).Returns(clientId).Returns(scopes).Returns(redirectUri);

            //Act
            string actual = await _financialInfoRepository.GetPayPalAuthorizationUrlAsync(entityId, _loggedInUser);

            //Assert
            Assert.Contains(clientId, actual);
            Assert.Contains(scopes, actual);
            Assert.Contains(redirectUri, actual);
            Assert.Contains(authorizationUrl, actual);
            Assert.Contains(entityId.ToString(), actual);
        }

        /// <summary>
        /// Test case to check if user is not linked with given entity(company) then throws InvalidResourceAccess exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetInvoicesAsync_UnauthorizedUser_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            EntityLoanMappingAC entityLoanMapping = new EntityLoanMappingAC { EntityId = Guid.NewGuid() };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _financialInfoRepository.GetInvoicesAsync(entityLoanMapping, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if no entry for invoices for given entity id then throws DataNotFoundException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetInvoicesAsync_NoEntryForInvoicesInDatabase_ReturnsDataNotFoundException()
        {
            //Arrange
            EntityLoanMappingAC entityLoanMapping = new EntityLoanMappingAC
            {
                EntityId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid()
            };
            List<FinancialStatement> statementList = new List<FinancialStatement>(){
                new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices }
            };
            EntityFinance entityFinance = null;
            List<EntityFinance> entityFinanceList = new List<EntityFinance>();
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(statementList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(entityFinanceList.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(() => _financialInfoRepository.GetInvoicesAsync(entityLoanMapping, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if no invoices are there for given entity id then returns an empty list of invoices.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetInvoicesAsync_EntityExistsWithNoInvoices_ReturnsInvoiceListObjectWithEmptyInvoiceList()
        {
            //Arrange
            EntityLoanMappingAC entityLoanMapping = new EntityLoanMappingAC
            {
                EntityId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid()
            };
            List<FinancialStatement> statementList = new List<FinancialStatement>(){
                new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices }
            };
            EntityFinance entityFinance = new EntityFinance
            {
                EntityId = entityLoanMapping.EntityId,
                LoanApplicationId = entityLoanMapping.LoanApplicationId,
                FinancialInformationFrom = null,
                FinancialInformationJson = null,
                FinancialStatementId = statementList.Single().Id
            };
            InvoiceListAC invoiceList = new InvoiceListAC
            {
                TotalCount = 0,
                Invoices = new List<InvoiceAC>()
            };
            List<string> years = new List<string> { "Jan-2018", "Jan-2019", "Jan-2020" };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(statementList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(It.IsAny<bool>()))
                .Returns(years);

            //Act
            InvoiceListAC actual = await _financialInfoRepository.GetInvoicesAsync(entityLoanMapping, _loggedInUser);

            //Assert
            Assert.Equal(invoiceList.TotalCount, actual.TotalCount);
            Assert.Equal(invoiceList.Invoices.Count, actual.Invoices.Count);
            Assert.Equal(actual.StartingYear, years.First());
            Assert.Equal(actual.EndingYear, years.Last());
        }

        /// <summary>
        /// Test case to check if paypal invoices exist for given entity then returns InvoiceListAC object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetInvoicesAsync_PaypalInvoicesExist_ReturnsInvoiceListACObject()
        {
            //Arrange
            EntityLoanMappingAC entityLoanMapping = new EntityLoanMappingAC
            {
                EntityId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid()
            };
            List<FinancialStatement> statementList = new List<FinancialStatement>(){
                new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices }
            };
            EntityFinance entityFinance = new EntityFinance
            {
                EntityId = entityLoanMapping.EntityId,
                LoanApplicationId = entityLoanMapping.LoanApplicationId,
                FinancialInformationFrom = FinancialInformationFrom.PayPal,
                FinancialStatementId = statementList.Single().Id,
                FinancialInformationJson = "{\"total_pages\":1,\"total_items\":1,\"items\":[{\"id\":\"123\",\"status\":\"SENT\",\"detail\":{\"currency_code\":\"INR\",\"invoice_number\":\"001\",\"invoice_date\":\"2020-07-15\",\"payment_term\":{\"due_date\":\"2020-08-01\"},\"viewed_by_recipient\":false,\"group_draft\":false,\"metadata\":{\"create_time\":\"2020-08-11T02:49:51Z\"}},\"invoicer\":{\"email_address\":\"pqr@xyz.com\"},\"primary_recipients\":[{\"billing_info\":{\"email_address\":\"abc@def.com\"}}],\"amount\":{\"currency_code\":\"INR\",\"value\":\"230\"},\"due_amount\":{\"currency_code\":\"INR\",\"value\":\"72.80\"}}]}"
            };
            InvoiceListAC invoiceList = new InvoiceListAC
            {
                TotalCount = 1,
                Invoices = new List<InvoiceAC>
                {
                    new InvoiceAC
                    {
                        Id = "123",
                        Status = "SENT",
                        InvoiceNumber = "001",
                        InvoiceDate = "2020-07-15",
                        InvoiceCreateDateTime = "2020-08-11T02:49:51Z",
                        InvoicerEmail = "abc@def.com",
                        RecipientEmail = "pqr@xyz.com",
                        CurrencyCode = "INR",
                        TotalAmount = "230"
                    }
                }
            };
            List<string> years = new List<string> { "Jan-2018", "Jan-2019", "Jan-2020" };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(statementList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(It.IsAny<bool>()))
                .Returns(years);

            //Act
            InvoiceListAC actual = await _financialInfoRepository.GetInvoicesAsync(entityLoanMapping, _loggedInUser);

            //Assert
            Assert.Equal(invoiceList.TotalCount, actual.TotalCount);
            Assert.Equal(invoiceList.Invoices.Count, actual.Invoices.Count);
            Assert.Equal(invoiceList.Invoices.First().InvoiceNumber, actual.Invoices.First().InvoiceNumber);
        }

        /// <summary>
        /// Test case to check if user is not linked with given entity(company) then throws InvalidResourceAccess exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_UnauthorizedUser_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC { State = guid.ToString() };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser));
            _globalRepository.Verify(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);
        }

        /// <summary>
        /// Test case to check if financial statement of type invoice doesn't exist then throws DataNotFound exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_FinancialStatementTypeInvoiceNotExists_ThrowsInvalidDataException()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC { State = guid.ToString() };
            FinancialStatement statement = null;
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidDataException>(() => _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if no data exists for given entity id then check for add operation allowance,
        /// if not allowed then throws InvaliOperation exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_NoDataExistsForGivenEntityIdAddOperationNotAllowed_ThrowsInvalidOperationException()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC { State = guid.ToString() };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = null;
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if paypal api returns null data for given account then throws DataNotFoundException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_NoDataExistsInPayPal_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC { State = guid.ToString(), LoanApplicationId = Guid.NewGuid(), FinancialInformationFrom = FinancialInformationFrom.PayPal };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = null;
            InvoicesResponseJsonAC searchResponse = new InvoicesResponseJsonAC();
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _payPalUtility.Setup(x => x.GetPayPalInvoicesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(searchResponse));

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(() => _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if paypal api returns data for given account then performs add operation for invoices,
        /// updates the section name and returns an object of InvoiceListAC.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_PayPalDataExistsForGivenAccount_AddInvoicesUpdateSectionReturnInvoiceListACObject()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC
            {
                State = guid.ToString(),
                LoanApplicationId = Guid.NewGuid(),
                AuthorizationCode = "C21AAG-05Ndht66FXT2SWFm",
                FinancialInformationFrom = FinancialInformationFrom.PayPal
            };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = null;
            InvoicesResponseJsonAC searchResponse = GetInvoicesResponseJsonACObject();
            List<string> years = new List<string> { "Jan-2018", "Jan-2019", "Jan-2020" };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _payPalUtility.Setup(x => x.GetPayPalInvoicesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(searchResponse));
            _globalRepository.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(It.IsAny<bool>()))
               .Returns(years);

            //Act
            InvoiceListAC actual = await _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<EntityFinance>(It.IsAny<EntityFinance>()), Times.Once);
            _globalRepository.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.NotNull(actual);
            Assert.InRange(actual.TotalCount, 1, actual.TotalCount);
            Assert.NotEmpty(actual.Invoices);
            Assert.Equal(actual.StartingYear, years.First());
            Assert.Equal(actual.EndingYear, years.Last());
        }

        /// <summary>
        /// Test case to check if data exists for given entity id then check for update operation allowance,
        /// if not allowed then throws InvaliOperation exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_DataExistsForGivenEntityIdUpdateOperationNotAllowed_ThrowsInvalidOperationException()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC { State = guid.ToString() };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = new EntityFinance();
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if paypal api returns data for given account then performs update operation for invoices 
        /// and returns an object of InvoiceListAC.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_PayPalDataExistsForGivenAccount_UpdateInvoicesReturnInvoiceListACObject()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC
            {
                State = guid.ToString(),
                LoanApplicationId = Guid.NewGuid(),
                AuthorizationCode = "C21AAG-05Ndht66FXT2SWFm",
                FinancialInformationFrom = FinancialInformationFrom.PayPal
            };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = new EntityFinance();
            InvoicesResponseJsonAC searchResponse = GetInvoicesResponseJsonACObject();
            List<string> years = new List<string> { "Jan-2018", "Jan-2019", "Jan-2020" };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _payPalUtility.Setup(x => x.GetPayPalInvoicesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(searchResponse));
            _globalRepository.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(It.IsAny<bool>()))
               .Returns(years);

            //Act
            InvoiceListAC actual = await _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityFinance>(It.IsAny<EntityFinance>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.NotNull(actual);
            Assert.InRange(actual.TotalCount, 1, actual.TotalCount);
            Assert.NotEmpty(actual.Invoices);
            Assert.Equal(actual.StartingYear, years.First());
            Assert.Equal(actual.EndingYear, years.Last());
        }

        /// <summary>
        /// Test case to check if square api returns data for given account then performs update operation for invoices 
        /// and returns an object of InvoiceListAC.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_SquareDataExistsForGivenAccount_UpdateInvoicesReturnInvoiceListACObject()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC
            {
                State = guid.ToString(),
                LoanApplicationId = Guid.NewGuid(),
                AuthorizationCode = "C21AAG-05Ndht66FXT2SWFm",
                FinancialInformationFrom = FinancialInformationFrom.Square
            };
            SearchInvoicesResponse searchInvoicesResponse = GetSearchInvoicesResponse();
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = new EntityFinance();
            List<string> years = new List<string> { "Jan-2018", "Jan-2019", "Jan-2020" };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _squareUtility.Setup(x => x.GetSquareInvoicesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(searchInvoicesResponse));
            _globalRepository.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(It.IsAny<bool>()))
               .Returns(years);

            //Act
            InvoiceListAC actual = await _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityFinance>(It.IsAny<EntityFinance>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.NotNull(actual);
            Assert.InRange(actual.TotalCount, 1, actual.TotalCount);
            Assert.NotEmpty(actual.Invoices);
            Assert.Equal(actual.StartingYear, years.First());
            Assert.Equal(actual.EndingYear, years.Last());
        }

        /// <summary>
        /// Test case to check if InvalidFinancialInformationFrom for update invoices then throws InvalidDataException
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_InvalidFinancialInformationFromForUpdateInvoice_ThrowsInvalidDataException()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC
            {
                State = guid.ToString(),
                LoanApplicationId = Guid.NewGuid(),
                AuthorizationCode = "C21AAG-05Ndht66FXT2SWFm",
                FinancialInformationFrom = FinancialInformationFrom.Quickbooks
            };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = new EntityFinance();
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidDataException>(() => _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if square api returns data for given account then performs add operation for invoices,
        /// updates the section name and returns an object of InvoiceListAC.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_SquareDataExistsForGivenAccount_AddInvoicesUpdateSectionReturnInvoiceListACObject()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC
            {
                State = guid.ToString(),
                LoanApplicationId = Guid.NewGuid(),
                AuthorizationCode = "C21AAG-05Ndht66FXT2SWFm",
                FinancialInformationFrom = FinancialInformationFrom.Square
            };
            SearchInvoicesResponse searchInvoicesResponse = GetSearchInvoicesResponse();
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = null;
            List<string> years = new List<string> { "Jan-2018", "Jan-2019", "Jan-2020" };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _squareUtility.Setup(x => x.GetSquareInvoicesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(searchInvoicesResponse));
            _globalRepository.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(It.IsAny<bool>()))
               .Returns(years);

            //Act
            InvoiceListAC actual = await _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
            _dataRepositoryMock.Verify(x => x.AddAsync<EntityFinance>(It.IsAny<EntityFinance>()), Times.Once);
            _globalRepository.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.NotNull(actual);
            Assert.InRange(actual.TotalCount, 1, actual.TotalCount);
            Assert.NotEmpty(actual.Invoices);
            Assert.Equal(actual.StartingYear, years.First());
            Assert.Equal(actual.EndingYear, years.Last());
        }

        /// <summary>
        /// Test case to check if InvalidFinancialInformationFrom for add invoices then throws InvalidDataException
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_InvalidFinancialInformationFrom_ThrowsInvalidDataException()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC
            {
                State = guid.ToString(),
                LoanApplicationId = Guid.NewGuid(),
                AuthorizationCode = "C21AAG-05Ndht66FXT2SWFm",
                FinancialInformationFrom = FinancialInformationFrom.Quickbooks
            };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = null;
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidDataException>(() => _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if square api returns null data for given account then throws DataNotFoundException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_NoDataExistsInSquare_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC { State = guid.ToString(), LoanApplicationId = Guid.NewGuid(), FinancialInformationFrom = FinancialInformationFrom.Square };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = null;
            SearchInvoicesResponse searchInvoicesResponse = new SearchInvoicesResponse(new List<Square.Models.Invoice>() { });

            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _squareUtility.Setup(x => x.GetSquareInvoicesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(searchInvoicesResponse));

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(() => _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser));
        }

        /// <summary>
        /// Test case to Check if current user is not linked with entity Id than throw InvalidResourceAccessException
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSquareAuthorizationUrlAsync_InvalidEntityId_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _financialInfoRepository.GetSquareAuthorizationUrlAsync(entityId, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if any one of the configurations is null then throws ConfigurationNotFound exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSquareAuthorizationUrlAsync_ConfigurationNotFound_ThrowsConfigurationNotFoundException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            string clientId = null;
            string scopes = "INVOICES_READ";
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns(clientId).Returns(scopes);

            //Act

            //Assert
            await Assert.ThrowsAsync<ConfigurationNotFoundException>(() => _financialInfoRepository.GetSquareAuthorizationUrlAsync(entityId, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if user is authorized and all the configurations are found then return valid auth url
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSquareAuthorizationUrlAsync_ValidUser_ReturnsAuthorizationUrl()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            string clientId = "sandbox-sq0idb-27r9tcZAbNWr0mfn718Hdw";
            string authUrl = "https://squareupsandbox.com/oauth2/authorize";

            var oneSectionMock = new Mock<IConfigurationSection>();
            oneSectionMock.Setup(s => s.Value).Returns("INVOICES_READ");
            var twoSectionMock = new Mock<IConfigurationSection>();
            twoSectionMock.Setup(s => s.Value).Returns("MERCHANT_PROFILE_READ");
            var scopeSectionMock = new Mock<IConfigurationSection>();
            scopeSectionMock.Setup(s => s.GetChildren()).Returns(new List<IConfigurationSection> { oneSectionMock.Object, twoSectionMock.Object });

            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns(authUrl)
                .Returns(clientId)
                .Returns(authUrl)
                .Returns(clientId);
            _configuration.Setup(x => x.GetSection(It.IsAny<string>()).GetSection(It.IsAny<string>()))
                .Returns(scopeSectionMock.Object);

            string expected = "https://squareupsandbox.com/oauth2/authorize?client_id=sandbox-sq0idb-27r9tcZAbNWr0mfn718Hdw&scope=INVOICES_READ+MERCHANT_PROFILE_READ&state=" + entityId;

            //Act
            string actual = await _financialInfoRepository.GetSquareAuthorizationUrlAsync(entityId, _loggedInUser);

            //Assert
            Assert.Contains(expected, actual);
        }

        /// <summary>
        /// Test case to check if square invoices exist for given entity then returns InvoiceListAC object.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetInvoicesAsync_SquareInvoicesExist_ReturnsInvoiceListACObject()
        {
            //Arrange
            EntityLoanMappingAC entityLoanMapping = new EntityLoanMappingAC
            {
                EntityId = Guid.NewGuid(),
                LoanApplicationId = Guid.NewGuid()
            };
            List<FinancialStatement> statementList = new List<FinancialStatement>(){
                new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices }
            };
            EntityFinance entityFinance = new EntityFinance
            {
                EntityId = entityLoanMapping.EntityId,
                LoanApplicationId = entityLoanMapping.LoanApplicationId,
                FinancialInformationFrom = FinancialInformationFrom.Square,
                FinancialStatementId = statementList.Single().Id,
                FinancialInformationJson = "{\"invoices\":[{\"id\":\"Nn0Hdett7Bu4ULJJrXecUQ\",\"version\":1,\"location_id\":\"LHA70WMJ5KVJS\",\"payment_requests\":[{\"uid\":\"57e1e298-942a-4ca0-80c1-eb8224dcb513\",\"request_method\":\"EMAIL\",\"request_type\":\"BALANCE\",\"due_date\":\"2020-08-17\",\"tipping_enabled\":true,\"reminders\":[{\"uid\":\"31b283e6-591e-449d-b0b8-4b40ea6afac6\",\"relative_scheduled_days\":-7,\"status\":\"NOT_APPLICABLE\"},{\"uid\":\"9c0e8fa0-ad9c-41be-925f-459b753eb21c\",\"relative_scheduled_days\":0,\"status\":\"NOT_APPLICABLE\"},{\"uid\":\"de55baac-74cb-4980-a026-198986999bc0\",\"relative_scheduled_days\":1,\"status\":\"SENT\",\"sent_at\":\"2020-08-18T11:00:00Z\"},{\"uid\":\"29d0123c-0ae8-47f4-a921-5d1867379498\",\"relative_scheduled_days\":3,\"status\":\"SENT\",\"sent_at\":\"2020-08-20T11:00:00Z\"}],\"computed_amount_money\":{\"amount\":1800,\"currency\":\"USD\"},\"total_completed_amount_money\":{\"amount\":0,\"currency\":\"USD\"}}],\"invoice_number\":\"000004\",\"title\":\"\",\"description\":\"We appreciate your business.\",\"public_url\":\"https:squareupsandbox.compay-invoiceNn0Hdett7Bu4ULJJrXecUQ\",\"status\":\"UNPAID\",\"timezone\":\"UTC\",\"created_at\":\"2020-08-17T09:40:07Z\",\"updated_at\":\"2020-08-20T11:00:00Z\",\"primary_recipient\":{\"customer_id\":\"1A0QPVD4NMS9V3R8W2HAVFJZBR\",\"given_name\":\"Kaitlyn\",\"family_name\":\"Spindel\",\"email_address\":\"kaitlyn@square-example.com\"},\"next_payment_amount_money\":{\"amount\":1800,\"currency\":\"USD\"}},{\"id\":\"6jnRYnJt-gZDFYVnpP0yEA\",\"version\":1,\"location_id\":\"LHA70WMJ5KVJS\",\"payment_requests\":[{\"uid\":\"3981237e-8bc0-40d3-83f8-5fb75c9d2ebf\",\"request_method\":\"EMAIL\",\"request_type\":\"BALANCE\",\"due_date\":\"2020-08-17\",\"tipping_enabled\":true,\"reminders\":[{\"uid\":\"a13eb3a2-d02e-4f68-b8a3-c66b2c950e91\",\"relative_scheduled_days\":-7,\"status\":\"NOT_APPLICABLE\"},{\"uid\":\"7fb320eb-18a6-4d9f-9e59-acd57388a483\",\"relative_scheduled_days\":0,\"status\":\"NOT_APPLICABLE\"},{\"uid\":\"f501d64c-4515-41e1-8a9f-d665543f4661\",\"relative_scheduled_days\":1,\"status\":\"SENT\",\"sent_at\":\"2020-08-18T11:00:00Z\"},{\"uid\":\"097e2685-9415-486b-baf9-b6970acf0e6f\",\"relative_scheduled_days\":3,\"status\":\"SENT\",\"sent_at\":\"2020-08-20T11:00:00Z\"}],\"computed_amount_money\":{\"amount\":50000,\"currency\":\"USD\"},\"total_completed_amount_money\":{\"amount\":0,\"currency\":\"USD\"}}],\"invoice_number\":\"000003\",\"title\":\"Test Title\",\"description\":\"We appreciate your business.\",\"public_url\":\"https:squareupsandbox.compay-invoice6jnRYnJt-gZDFYVnpP0yEA\",\"status\":\"UNPAID\",\"timezone\":\"UTC\",\"created_at\":\"2020-08-17T09:39:11Z\",\"updated_at\":\"2020-08-20T11:00:00Z\",\"primary_recipient\":{\"customer_id\":\"ATK052CM28YC9FNC3VYKBCEHHM\",\"given_name\":\"Ryan\",\"family_name\":\"Nakamura\",\"email_address\":\"nakamura710@square-example.com\"},\"next_payment_amount_money\":{\"amount\":50000,\"currency\":\"USD\"}},{\"id\":\"mtsjPNbIJp8tYBj1fkjBIg\",\"version\":0,\"location_id\":\"LHA70WMJ5KVJS\",\"payment_requests\":[{\"uid\":\"72ff1a5e-c68e-4b4d-8a0f-6a1d1b387517\",\"request_method\":\"EMAIL\",\"request_type\":\"BALANCE\",\"due_date\":\"2020-08-17\",\"tipping_enabled\":true,\"reminders\":[{\"uid\":\"5324f542-7bee-4ffb-bb67-495f8d9e449c\",\"relative_scheduled_days\":-7,\"status\":\"NOT_APPLICABLE\"},{\"uid\":\"6442dd93-dea9-4bdb-b7e6-f812666009d7\",\"relative_scheduled_days\":0,\"status\":\"NOT_APPLICABLE\"},{\"uid\":\"792a9952-5199-40c2-81db-e128e138f286\",\"relative_scheduled_days\":1,\"status\":\"SENT\",\"sent_at\":\"2020-08-18T11:00:00Z\"},{\"uid\":\"97e6a67f-ac3d-4181-b3d9-a366d2a64972\",\"relative_scheduled_days\":3,\"status\":\"SENT\",\"sent_at\":\"2020-08-20T11:00:00Z\"}],\"computed_amount_money\":{\"amount\":50000,\"currency\":\"USD\"},\"total_completed_amount_money\":{\"amount\":0,\"currency\":\"USD\"}}],\"invoice_number\":\"000002\",\"title\":\"Test Title\",\"description\":\"We appreciate your business.\",\"public_url\":\"https:squareupsandbox.compay-invoicemtsjPNbIJp8tYBj1fkjBIg\",\"status\":\"UNPAID\",\"timezone\":\"UTC\",\"created_at\":\"2020-08-17T09:38:44Z\",\"updated_at\":\"2020-08-20T11:00:00Z\",\"primary_recipient\":{\"customer_id\":\"ATK052CM28YC9FNC3VYKBCEHHM\",\"given_name\":\"Ryan\",\"family_name\":\"Nakamura\",\"email_address\":\"nakamura710@square-example.com\"},\"next_payment_amount_money\":{\"amount\":50000,\"currency\":\"USD\"}},{\"id\":\"EdBpR3luOzFmx-fq7pBCDQ\",\"version\":0,\"location_id\":\"LHA70WMJ5KVJS\",\"payment_requests\":[{\"uid\":\"3ee26a34-22ff-47e8-a1ac-8e3a0851057a\",\"request_method\":\"EMAIL\",\"request_type\":\"BALANCE\",\"due_date\":\"2020-08-17\",\"tipping_enabled\":true,\"reminders\":[{\"uid\":\"c3972353-00ec-4c26-aeb4-3a665af1233f\",\"relative_scheduled_days\":-7,\"status\":\"NOT_APPLICABLE\"},{\"uid\":\"b01a88a5-36cb-4ca2-a4f1-c365f77d71ee\",\"relative_scheduled_days\":0,\"status\":\"NOT_APPLICABLE\"},{\"uid\":\"dd628f6a-385b-4122-91d1-7a0751921ba5\",\"relative_scheduled_days\":1,\"status\":\"SENT\",\"sent_at\":\"2020-08-18T11:00:00Z\"},{\"uid\":\"a5764e7b-2aee-4236-8e0c-8375147d239e\",\"relative_scheduled_days\":3,\"status\":\"SENT\",\"sent_at\":\"2020-08-20T11:00:00Z\"}],\"computed_amount_money\":{\"amount\":4400000,\"currency\":\"USD\"},\"total_completed_amount_money\":{\"amount\":0,\"currency\":\"USD\"}}],\"invoice_number\":\"000001\",\"title\":\"\",\"description\":\"We appreciate your business.\",\"public_url\":\"https:squareupsandbox.compay-invoiceEdBpR3luOzFmx-fq7pBCDQ\",\"status\":\"UNPAID\",\"timezone\":\"UTC\",\"created_at\":\"2020-08-17T09:37:16Z\",\"updated_at\":\"2020-08-20T11:00:00Z\",\"primary_recipient\":{\"customer_id\":\"1A0QPVD4NMS9V3R8W2HAVFJZBR\",\"given_name\":\"Kaitlyn\",\"family_name\":\"Spindel\",\"email_address\":\"kaitlyn@square-example.com\"},\"next_payment_amount_money\":{\"amount\":4400000,\"currency\":\"USD\"}}]}"
            };
            InvoiceListAC invoiceList = new InvoiceListAC
            {
                TotalCount = 4,
                Invoices = GetSquareInvoices()
            };

            List<EntityFinance> entityFinanceList = new List<EntityFinance>();
            List<string> years = new List<string> { "Jan-2018", "Jan-2019", "Jan-2020" };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(statementList.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(entityFinanceList.AsQueryable().BuildMock().Object);
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(It.IsAny<bool>()))
                .Returns(years);

            //Act
            InvoiceListAC actual = await _financialInfoRepository.GetInvoicesAsync(entityLoanMapping, _loggedInUser);

            //Assert
            Assert.Equal(invoiceList.TotalCount, actual.TotalCount);
            Assert.Equal(invoiceList.Invoices.Count, actual.Invoices.Count);
            Assert.Equal(invoiceList.Invoices.First().InvoiceNumber, actual.Invoices.First().InvoiceNumber);
        }

        /// <summary>
        /// Test case to check if user is not linked with given entity(company) then throws InvalidResourceAccess exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetStripeAuthorizationUrlAsync_UnauthorizedUser_ThrowsInvalidResourceAccessException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<InvalidResourceAccessException>(() => _financialInfoRepository.GetStripeAuthorizationUrlAsync(entityId, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if any one of the configurations is null then throws ConfigurationNotFound exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetStripeAuthorizationUrlAsync_ConfigurationNotFound_ThrowsConfigurationNotFoundException()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            string clientId = "123";
            string authorizationUrl = "abc.com/def?";
            string scopes = null;
            string redirectUri = "pqr.com/xyz";
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns(authorizationUrl).Returns(clientId).Returns(scopes).Returns(redirectUri);

            //Act

            //Assert
            await Assert.ThrowsAsync<ConfigurationNotFoundException>(() => _financialInfoRepository.GetStripeAuthorizationUrlAsync(entityId, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if user is authorized and all the configurations are found then 
        /// returns an object of InvoiceRequestParametersAC.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetStripeAuthorizationUrlAsync_AuthorizedUserConfigurationFound_ReturnsAuthorizationUrl()
        {
            //Arrange
            Guid entityId = Guid.NewGuid();
            string clientId = "123";
            string authorizationUrl = "abc.com/def?";
            string scopes = "john";
            string redirectUri = "pqr.com/xyz";
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.SetupSequence(x => x.GetSection(It.IsAny<string>()).Value)
                .Returns(authorizationUrl).Returns(clientId).Returns(scopes).Returns(redirectUri)
                .Returns(authorizationUrl).Returns(clientId).Returns(scopes).Returns(redirectUri);

            //Act
            string actual = await _financialInfoRepository.GetStripeAuthorizationUrlAsync(entityId, _loggedInUser);

            //Assert
            Assert.Contains(clientId, actual);
            Assert.Contains(scopes, actual);
            Assert.Contains(redirectUri, actual);
            Assert.Contains(authorizationUrl, actual);
            Assert.Contains(entityId.ToString(), actual);
        }

        /// <summary>
        /// Test case to check if stripe api returns null data for given account then throws DataNotFound exception.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_NoDataExistsInStripe_ThrowsDataNotFoundException()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC { State = guid.ToString(), LoanApplicationId = Guid.NewGuid(), FinancialInformationFrom = FinancialInformationFrom.Stripe };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = null;
            StripeList<Stripe.Invoice> searchResponse = new StripeList<Stripe.Invoice> { Data = new List<Stripe.Invoice>() };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _stripeUtility.Setup(x => x.GetStripeInvoicesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(searchResponse));

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(() => _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser));
        }

        /// <summary>
        /// Test case to check if stripe api returns data for given account then performs add operation for invoices,
        /// updates the section name and returns an object of InvoiceListAC.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_StripeDataExistsForGivenAccount_AddInvoicesUpdateSectionReturnInvoiceListACObject()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC
            {
                State = guid.ToString(),
                LoanApplicationId = Guid.NewGuid(),
                AuthorizationCode = "C21AAG-05Ndht66FXT2SWFm",
                FinancialInformationFrom = FinancialInformationFrom.Stripe
            };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = null;
            StripeList<Stripe.Invoice> searchResponse = GetStripeListOfTypeInvoiceObject();
            List<string> years = new List<string> { "Jan-2018", "Jan-2019", "Jan-2020" };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _stripeUtility.Setup(x => x.GetStripeInvoicesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(searchResponse));
            _globalRepository.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(It.IsAny<bool>()))
               .Returns(years);

            //Act
            InvoiceListAC actual = await _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync<EntityFinance>(It.IsAny<EntityFinance>()), Times.Once);
            _globalRepository.Verify(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.NotNull(actual);
            Assert.InRange(actual.TotalCount, 1, actual.TotalCount);
            Assert.NotEmpty(actual.Invoices);
            Assert.Equal(actual.StartingYear, years.First());
            Assert.Equal(actual.EndingYear, years.Last());
        }

        /// <summary>
        /// Test case to check if stripe api returns data for given account then performs update operation for invoices 
        /// and returns an object of InvoiceListAC.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveInvoicesAsync_StripeDataExistsForGivenAccount_UpdateInvoicesReturnInvoiceListACObject()
        {
            //Arrange
            Guid guid = Guid.NewGuid();
            InvoiceRequestParametersAC invoiceRequest = new InvoiceRequestParametersAC
            {
                State = guid.ToString(),
                LoanApplicationId = Guid.NewGuid(),
                AuthorizationCode = "C21AAG-05Ndht66FXT2SWFm",
                FinancialInformationFrom = FinancialInformationFrom.Stripe
            };
            FinancialStatement statement = new FinancialStatement { Id = Guid.NewGuid(), Name = StringConstant.Invoices };
            EntityFinance entityFinance = new EntityFinance();
            StripeList<Stripe.Invoice> searchResponse = GetStripeListOfTypeInvoiceObject();
            List<string> years = new List<string> { "Jan-2018", "Jan-2019", "Jan-2020" };
            _globalRepository.Setup(x => x.CheckEntityRelationshipMappingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<FinancialStatement, bool>>>()))
                .Returns(Task.FromResult(statement));
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<EntityFinance, bool>>>()))
                .Returns(Task.FromResult(entityFinance));
            _globalRepository.Setup(x => x.IsAddOrUpdateAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            _configuration.Setup(x => x.GetSection("FinancialYear:Years").Value).Returns("3");
            _configuration.Setup(x => x.GetSection("FinancialYear:StartMonth").Value).Returns("January");
            _stripeUtility.Setup(x => x.GetStripeInvoicesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(searchResponse));
            _globalRepository.Setup(x => x.UpdateSectionNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();
            _globalRepository.Setup(x => x.GetListOfLastNFinancialYears(It.IsAny<bool>()))
               .Returns(years);

            //Act
            InvoiceListAC actual = await _financialInfoRepository.SaveInvoicesAsync(invoiceRequest, _loggedInUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update<EntityFinance>(It.IsAny<EntityFinance>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.NotNull(actual);
            Assert.InRange(actual.TotalCount, 1, actual.TotalCount);
            Assert.NotEmpty(actual.Invoices);
            Assert.Equal(actual.StartingYear, years.First());
            Assert.Equal(actual.EndingYear, years.Last());
        }
        #endregion
    }
}
