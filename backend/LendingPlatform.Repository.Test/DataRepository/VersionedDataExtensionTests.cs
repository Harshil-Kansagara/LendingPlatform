using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LendingPlatform.DomainModel.DataRepository.Tests
{
    [Collection("Register Dependency")]
    public class VersionedDataExtensionTests : BaseTest
    {
        private readonly Guid _latestVersion;
        private readonly IQueryable<EntityFinance> _entityFinanceList;
        private readonly IQueryable<EntityTaxForm> _entityTaxFormList;
        private readonly IQueryable<CreditReport> _creditReportList;

        public VersionedDataExtensionTests(Bootstrap bootstrap) : base(bootstrap)
        {
            _latestVersion = Guid.NewGuid();
            _entityFinanceList = FetchFinances();
            _entityTaxFormList = FetchTaxCollection();
            _creditReportList = FetchCreditReport();
        }


        private IQueryable<EntityTaxForm> FetchTaxCollection()
        {
            var entityId = Guid.NewGuid();
            var loanId = Guid.NewGuid();
            var version = Guid.NewGuid();
            var taxFormId = Guid.NewGuid();
            var someOtherVersion = _latestVersion;
            List<EntityTaxForm> entityTaxForms = new List<EntityTaxForm>
            {
                new EntityTaxForm
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    LoanApplicationId=loanId,
                    SurrogateId=1,
                    TaxFormId=taxFormId,
                    Version=version
                },
                new EntityTaxForm
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    LoanApplicationId=loanId,
                    SurrogateId=2,
                    TaxFormId=taxFormId,
                    Version=version
                },
                new EntityTaxForm
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    LoanApplicationId=loanId,
                    SurrogateId=3,
                    TaxFormId=taxFormId,
                    Version=someOtherVersion
                },
                new EntityTaxForm
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    LoanApplicationId=loanId,
                    SurrogateId=4,
                    TaxFormId=taxFormId,
                    Version=someOtherVersion
                }
            };
            return entityTaxForms.AsQueryable();
        }

        private IQueryable<EntityFinance> FetchFinances()
        {
            var entityId = Guid.NewGuid();
            var loanId = Guid.NewGuid();
            var version = Guid.NewGuid();
            var statementId = Guid.NewGuid();
            var someOtherVersion = _latestVersion;
            List<EntityFinance> entityFinances = new List<EntityFinance>
            {
                new EntityFinance
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    FinancialInformationJson="some json",
                    FinancialStatementId=statementId,
                    IntegratedServiceConfigurationId=Guid.NewGuid(),
                    LoanApplicationId=loanId,
                    Version=version,
                    SurrogateId=1
                },
                new EntityFinance
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    FinancialInformationJson="some json",
                    FinancialStatementId=statementId,
                    IntegratedServiceConfigurationId=Guid.NewGuid(),
                    LoanApplicationId=loanId,
                    Version=version,
                    SurrogateId=2
                },
                new EntityFinance
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    FinancialInformationJson="some json",
                    FinancialStatementId=statementId,
                    IntegratedServiceConfigurationId=Guid.NewGuid(),
                    LoanApplicationId=loanId,
                    Version=someOtherVersion,
                    SurrogateId=3
                },
                new EntityFinance
                {
                    CreatedByUserId=Guid.NewGuid(),
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    FinancialInformationJson="some json",
                    FinancialStatementId=statementId,
                    IntegratedServiceConfigurationId=Guid.NewGuid(),
                    LoanApplicationId=loanId,
                    Version=someOtherVersion,
                    SurrogateId=4
                }
            };
            return entityFinances.AsQueryable();
        }

        private IQueryable<CreditReport> FetchCreditReport()
        {
            var entityId = Guid.NewGuid();
            var loanId = Guid.NewGuid();
            var version = Guid.NewGuid();
            var someOtherVersion = _latestVersion;
            var creditReports = new List<CreditReport>
            {
                new CreditReport
                {
                    CommercialScore=1,
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    FsrScore=2,
                    HasPendingJudgment=false,
                    HasPendingLien=false,
                    IntegratedServiceConfigurationId=Guid.NewGuid(),
                    IsBankrupted=false,
                    LoanApplicationId=loanId,
                    Response="some json",
                    SurrogateId=1,
                    Version=version
                },
                 new CreditReport
                {
                    CommercialScore=1,
                    CreatedOn=DateTime.UtcNow,
                    EntityId=entityId,
                    FsrScore=2,
                    HasPendingJudgment=false,
                    HasPendingLien=false,
                    IntegratedServiceConfigurationId=Guid.NewGuid(),
                    IsBankrupted=false,
                    LoanApplicationId=loanId,
                    Response="some json",
                    SurrogateId=2,
                    Version=someOtherVersion
                }
            };
            return creditReports.AsQueryable();
        }



        [Fact()]
        public static void GetLatestVersionForLoan_CheckEmptyTaxesQueryable_ReturnsEmptyQueryable()
        {
            // Arrange
            var emptyTaxForm = new List<EntityTaxForm>().AsQueryable();

            // Act
            VersionedDataExtension.GetLatestVersionForLoan(emptyTaxForm);
            // Assert
            Assert.NotNull(emptyTaxForm);
        }

        [Fact()]
        public static void GetLatestVersionForLoan_CheckEmptyFinancesQueryable_ReturnsEmptyQueryable()
        {
            // Arrange
            var emptyFinanceForm = new List<EntityFinance>().AsQueryable();

            // Act
            VersionedDataExtension.GetLatestVersionForLoan(emptyFinanceForm);

            // Assert
            Assert.NotNull(emptyFinanceForm);
        }

        [Fact()]
        public static void GetLatestVersionForLoan_CheckEmptyCreditReportQueryable_ReturnsEmptyQueryable()
        {
            // Arrange
            var emptyCreditReportForm = new List<EntityTaxForm>().AsQueryable();

            // Act
            VersionedDataExtension.GetLatestVersionForLoan(emptyCreditReportForm);

            // Assert
            Assert.NotNull(emptyCreditReportForm);
        }

        [Theory]
        [InlineData(typeof(EntityTaxForm))]
        [InlineData(typeof(CreditReport))]
        [InlineData(typeof(EntityFinance))]
        public void GetLatestVersionForLoan_CheckValidQueryable_ReturnsValidQueryable(Type type)
        {
            //Arrange

            // Act
            if (type == typeof(EntityTaxForm))
            {
                var taxForm = _entityTaxFormList.GetLatestVersionForLoan();
                // Assert
                Assert.NotEmpty(taxForm);
                Assert.Equal(_latestVersion, taxForm.First().Version);
            }
            else if (type == typeof(EntityFinance))
            {
                var finance = _entityFinanceList.GetLatestVersionForLoan();
                // Assert
                Assert.NotEmpty(finance);
                Assert.Equal(_latestVersion, finance.First().Version);
            }
            else
            {
                var creditReport = _creditReportList.GetLatestVersionForLoan();
                // Assert
                Assert.NotEmpty(creditReport);
                Assert.Equal(_latestVersion, creditReport.First().Version);
            }


        }


        [Theory]
        [InlineData(typeof(EntityTaxForm))]
        [InlineData(typeof(CreditReport))]
        [InlineData(typeof(EntityFinance))]
        public void VersionThisQueryable_CheckValidQueryable_SetsVersionInQueryable(Type type)
        {
            //Arrange
            var loanId = Guid.NewGuid();
            // Act
            if (type == typeof(EntityTaxForm))
            {
                foreach (var form in _entityTaxFormList)
                {
                    form.LoanApplicationId = null;
                    form.Version = null;
                }

                var taxForm = _entityTaxFormList.VersionThisQueryable(loanId);
                // Assert
                Assert.NotEmpty(taxForm);
                Assert.All(taxForm, x => Assert.True(x.Version != null));
                Assert.All(taxForm, x => Assert.True(x.LoanApplicationId != null));
                Assert.All(taxForm, x => Assert.True(x.LoanApplicationId == loanId));
            }
            else if (type == typeof(EntityFinance))
            {
                foreach (var form in _entityFinanceList)
                {
                    form.LoanApplicationId = null;
                    form.Version = null;
                }
                var finance = _entityFinanceList.VersionThisQueryable(loanId);
                // Assert
                Assert.NotEmpty(finance);
                Assert.All(finance, x => Assert.True(x.Version != null));
                Assert.All(finance, x => Assert.True(x.LoanApplicationId != null));
                Assert.All(finance, x => Assert.True(x.LoanApplicationId == loanId));
            }
            else
            {
                foreach (var form in _creditReportList)
                {
                    form.LoanApplicationId = null;
                    form.Version = null;
                }
                var creditReport = _creditReportList.VersionThisQueryable(loanId);
                // Assert
                Assert.NotEmpty(creditReport);
                Assert.All(creditReport, x => Assert.True(x.Version != null));
                Assert.All(creditReport, x => Assert.True(x.LoanApplicationId != null));
                Assert.All(creditReport, x => Assert.True(x.LoanApplicationId == loanId));
            }


        }

        [Theory]
        [InlineData(typeof(EntityTaxForm))]
        [InlineData(typeof(CreditReport))]
        [InlineData(typeof(EntityFinance))]
        public void SanitizeIdentifiers_CheckValidQueryable_ResetsVersionInQueryable(Type type)
        {
            //Arrange

            // Act
            if (type == typeof(EntityTaxForm))
            {
                var taxForm = VersionedDataExtension.SanitizeIdentifiers(_entityTaxFormList.ToList());
                // Assert
                Assert.NotEmpty(taxForm);
                Assert.All(taxForm, x => Assert.True(x.Version == null));
                Assert.All(taxForm, x => Assert.True(x.LoanApplicationId == null));

            }
            else if (type == typeof(EntityFinance))
            {

                var finance = VersionedDataExtension.SanitizeIdentifiers(_entityFinanceList.ToList());
                // Assert
                Assert.NotEmpty(finance);
                Assert.All(finance, x => Assert.True(x.Version == null));
                Assert.All(finance, x => Assert.True(x.LoanApplicationId == null));
            }
            else
            {
                foreach (var form in _creditReportList)
                {
                    form.LoanApplicationId = null;
                    form.Version = null;
                }
                var creditReport = VersionedDataExtension.SanitizeIdentifiers(_creditReportList.ToList());
                // Assert
                Assert.NotEmpty(creditReport);
                Assert.All(creditReport, x => Assert.True(x.Version == null));
                Assert.All(creditReport, x => Assert.True(x.LoanApplicationId == null));
            }


        }
    }
}