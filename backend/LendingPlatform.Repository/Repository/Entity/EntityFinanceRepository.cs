using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.Quickbooks;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SmartyStreets.USStreetApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace LendingPlatform.Repository.Repository.EntityInfo
{
    public class EntityFinanceRepository : IEntityFinanceRepository
    {

        #region Private Variables
        private readonly IDataRepository _dataRepository;
        private readonly IQuickbooksUtility _quickbooksUtility;
        private readonly IXeroUtility _xeroUtility;
        private readonly IGlobalRepository _globalRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IAmazonServicesUtility _amazonServicesUtility;
        private readonly ISmartyStreetsUtility _smartyStreetsUtility;
        private readonly IRulesUtility _rulesUtility;
        #endregion

        #region Constructor
        public EntityFinanceRepository(IDataRepository dataRepository, IQuickbooksUtility quickbooksUtility,
            IXeroUtility xeroUtility, IGlobalRepository globalRepository,
            IConfiguration configuration, IMapper mapper,
            IAmazonServicesUtility amazonServicesUtility,
            ISmartyStreetsUtility smartyStreetsUtility,
            IRulesUtility rulesUtility)
        {
            _dataRepository = dataRepository;
            _quickbooksUtility = quickbooksUtility;
            _xeroUtility = xeroUtility;
            _globalRepository = globalRepository;
            _configuration = configuration;
            _mapper = mapper;
            _amazonServicesUtility = amazonServicesUtility;
            _smartyStreetsUtility = smartyStreetsUtility;
            _rulesUtility = rulesUtility;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get third party finances
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="statementNames"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<List<CompanyFinanceAC>> GetFinancesAsync(Guid id, ResourceType type, string statementNames, CurrentUserAC currentUser)
        {
            #region Validations
            // If Guid is missing or statementnames are missing
            if (id == Guid.Empty || string.IsNullOrEmpty(statementNames) || type == ResourceType.Other || type == ResourceType.User)
            {
                // Throw exception
                throw new InvalidParameterException(StringConstant.MissingStatementsInParameter);
            }


            // Separate statementNames csv
            List<string> statementList = statementNames.Split(",").ToList();

            var financialStatementListInDb = _dataRepository.GetAll<FinancialStatement>().Select(x => x.Name).ToList();

            // Check if all statement names are properly sent in request
            if (financialStatementListInDb.Intersect(statementList).Count() != statementList.Count)
            {
                // Throw exception
                throw new InvalidParameterException(StringConstant.MissingStatementsInParameter);
            }

            List<EntityFinance> entityFinance = new List<EntityFinance>();

            // Fetch finances from  CompanyId
            if (type == ResourceType.Company)
            {
                // If user sent companyId whose access he doesnt have
                if (!await _globalRepository.CheckEntityRelationshipMappingAsync(id, currentUser.Id, !currentUser.IsBankUser))
                {
                    // Throw exception
                    throw new InvalidResourceAccessException();
                }

                var financeEntriesForTheEntity = _dataRepository.Fetch<EntityFinance>(x => x.EntityId == id);

                // Fetch the latest finances (non-versioned), if exists
                if (financeEntriesForTheEntity.Any(x => x.LoanApplicationId == null))
                {
                    // Fetch the finances (with loanId null)
                    entityFinance = await financeEntriesForTheEntity.Where(x => x.LoanApplicationId == null)
                        .Include(x => x.FinancialStatement)
                        .Include(x => x.IntegratedServiceConfiguration)
                        .Include(x => x.EntityFinanceYearlyMappings)
                        .ThenInclude(x => x.EntityFinanceStandardAccounts)
                        .ToListAsync();
                }
                // If non-versioned finances doesn't exist, fetch latest versioned finances, clone them and make a new non-versioned finance
                else
                {
                    if (!currentUser.IsBankUser)
                        await CloneEntityFinanceAndRelatedTablesAsync(financeEntriesForTheEntity);

                    entityFinance = await _dataRepository.Fetch<EntityFinance>(x => x.EntityId == id && x.LoanApplicationId == null)
                                   .Include(x => x.FinancialStatement)
                                   .Include(x => x.IntegratedServiceConfiguration)
                                   .Include(x => x.EntityFinanceYearlyMappings)
                                   .ThenInclude(x => x.EntityFinanceStandardAccounts)
                                   .ToListAsync();
                }
            }
            // Fetch finances (versioned) from LoanId
            else if (type == ResourceType.Loan)
            {
                // If user sent LoanId whose access he doesnt have
                if (!await _globalRepository.CheckUserLoanAccessAsync(currentUser, id, !currentUser.IsBankUser))
                {
                    // Throw exception
                    throw new InvalidResourceAccessException();
                }
                var latestEntityFinance = _dataRepository.Fetch<EntityFinance>(x => x.LoanApplicationId == id).Include(x => x.FinancialStatement).Where(x => !x.FinancialStatement.Name.Equals(StringConstant.PersonalFinances)).GetLatestVersionForLoan();

                if (latestEntityFinance.Any())
                {
                    entityFinance = await latestEntityFinance
                         .Include(x => x.FinancialStatement)
                   .Include(x => x.IntegratedServiceConfiguration)
                   .Include(x => x.EntityFinanceYearlyMappings)
                   .ThenInclude(x => x.EntityFinanceStandardAccounts)
                   .ToListAsync();
                }


            }



            #endregion

            #region Fetch statements


            var entityFinanceForCsv = entityFinance.Where(x => statementList.Contains(x.FinancialStatement.Name, StringComparer.InvariantCultureIgnoreCase))
                .ToList();

            // This exception indicates that no third party service is connected for this entityid (or manual entry is done)
            if (entityFinance.Count == 0 || entityFinanceForCsv.Count == 0)
            {
                throw new DataNotFoundException(StringConstant.DataNotFound);
            }
            #endregion

            // Map finances
            var financeAC = _mapper.Map<List<CompanyFinanceAC>>(entityFinance);

            return financeAC;

        }

        /// <summary>
        /// Method to map finances and standard account List
        /// </summary>
        /// <param name="financeAcList"></param>
        /// <param name="entityFinanceList"></param>
        public List<CompanyFinanceAC> GetStandardAccountsList(List<CompanyFinanceAC> financeAcList, List<EntityFinance> entityFinanceList)
        {
            // Check if the third party response is mapped with standard jamoon account
            foreach (var finance in financeAcList)
            {

                finance.DivisionFactor = _configuration.GetValue<decimal>("Currency:AmountDivisonFactor");
                // This is to prevent fetching of old mapped records in case the user has updated the finances
                if (finance.UpdatedOn != null)
                {
                    finance.CreationDateTime = _globalRepository.ConvertUtcDateToLocalDate(finance.UpdatedOn.Value);
                    finance.IsChartOfAccountMapped = entityFinanceList.Any(x => x.Id == finance.Id && x.EntityFinanceYearlyMappings != null && x.EntityFinanceYearlyMappings.Count > 0 && x.EntityFinanceYearlyMappings.Any(y => y.LastAddedDateTime >= x.UpdatedOn));
                }
                else
                {
                    finance.CreationDateTime = _globalRepository.ConvertUtcDateToLocalDate(entityFinanceList.First().CreatedOn);
                    finance.IsChartOfAccountMapped = entityFinanceList.Any(x => x.Id == finance.Id && x.EntityFinanceYearlyMappings != null && x.EntityFinanceYearlyMappings.Count > 0);
                }

            }
            if (financeAcList.All(x => x.IsChartOfAccountMapped))
            {
                FetchMappedAccountDetails(entityFinanceList, financeAcList);
            }
            return financeAcList;
        }

        /// <summary>
        /// Method to fetch authorization url for third party source
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="source"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<string> GetAuthorizationUrlAsync(Guid entityId, string source, CurrentUserAC currentUser)
        {

            if (entityId == Guid.Empty || string.IsNullOrEmpty(source))
            {
                // Throw exception
                throw new InvalidParameterException();
            }
            if (!await _globalRepository.CheckEntityRelationshipMappingAsync(entityId, currentUser.Id))
            {
                // Throw exception
                throw new InvalidResourceAccessException();
            }


            var thirdPartyService = await _dataRepository.SingleOrDefaultAsync<IntegratedServiceConfiguration>(x => x.Name == source);

            if (thirdPartyService == null)
            {
                // Throw exception
                throw new InvalidParameterException();
            }

            if (source.Equals(StringConstant.Quickbooks, StringComparison.InvariantCultureIgnoreCase))
            {

                return _quickbooksUtility.GetAuthorizationUrl(entityId, thirdPartyService.ConfigurationJson);
            }
            else
            {
                return _xeroUtility.GetLoginUrl(entityId, thirdPartyService.ConfigurationJson);
            }

        }

        /// <summary>
        /// Add or update entity's finances from third party services
        /// </summary>
        /// <param name="redirectUrlCallbackData"></param>
        /// <param name="statementsCsv"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task AddOrUpdateFinancesAsync(ThirdPartyServiceCallbackDataAC redirectUrlCallbackData, string statementsCsv, CurrentUserAC currentUser)
        {

            #region Validations
            // Validations
            // 1. Check if request parameters are proper
            if (redirectUrlCallbackData.EntityId == Guid.Empty || string.IsNullOrEmpty(statementsCsv))
            {
                // Throw exception
                throw new InvalidParameterException();
            }
            // 2. Check if the currentUser is linked with the entityId
            else if (!await _globalRepository.CheckEntityRelationshipMappingAsync(redirectUrlCallbackData.EntityId, currentUser.Id, true))
            {
                // Throw exception
                throw new InvalidResourceAccessException();
            }

            // 3. Check if the finance statement name is valid (Exists or not)
            var statementList = statementsCsv.Split(",").ToList();
            var financialStatementList = (await _dataRepository.GetAll<FinancialStatement>().ToListAsync()).Where(x => statementList.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase)).ToList();
            if (financialStatementList.Count != statementList.Count)
            {
                // Throw Exception
                throw new InvalidParameterException();
            }
            // 4. If the third party service id is invalid
            var thirdPartyFinanceService = (await _dataRepository.GetAll<IntegratedServiceConfiguration>().ToListAsync()).SingleOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name.Equals(redirectUrlCallbackData.ThirdPartyServiceName, StringComparison.InvariantCultureIgnoreCase));
            if (thirdPartyFinanceService == null)
            {
                // Throw Exception
                throw new InvalidParameterException();
            }

            redirectUrlCallbackData.ConfigurationJson = thirdPartyFinanceService.ConfigurationJson;
            var configurationList = JsonConvert.DeserializeObject<List<Utils.ApplicationClass.ThirdPartyConfigurationAC>>(redirectUrlCallbackData.ConfigurationJson);
            #endregion

            var financialReportList = new List<FinancialStatementsAC>();

            // Be it add or update, the finances comes from third party services. So fetch the finances from third party service and then pass it to db add/update 
            // Prepare report filters
            redirectUrlCallbackData = await FetchReportFiltersAsync(redirectUrlCallbackData, statementList);

            if (thirdPartyFinanceService.Name.Equals(StringConstant.Quickbooks, StringComparison.InvariantCultureIgnoreCase))
            {
                financialReportList = await GetQuickbooksReportsAsync(redirectUrlCallbackData, configurationList, thirdPartyFinanceService);

            }
            else if (thirdPartyFinanceService.Name.Equals(StringConstant.Xero, StringComparison.InvariantCultureIgnoreCase))
            {
                financialReportList = await GetXeroReportsAsync(redirectUrlCallbackData, configurationList, thirdPartyFinanceService);
            }


            // Check if for the entityId, whether respective financial statement exists or not.
            var allFinanceEntriesForEntityid = _dataRepository.Fetch<EntityFinance>(x => x.EntityId == redirectUrlCallbackData.EntityId);
            var latestEntityFinances = allFinanceEntriesForEntityid.Where(x => x.LoanApplicationId == null);
            var entityFinanceListInDb = await latestEntityFinances.Include(x => x.FinancialStatement).ToListAsync();
            if (!entityFinanceListInDb.Any())
            {
                entityFinanceListInDb = await latestEntityFinances.GetLatestVersionForLoan().Include(x => x.FinancialStatement).ToListAsync();
            }

            using (await _dataRepository.BeginTransactionAsync())
            {

                var newFinanceList = new List<EntityFinance>();
                var updateFinanceList = new List<EntityFinance>();
                _dataRepository.DisableChangeTracking();
                foreach (var statement in statementList)
                {
                    var currentStatement = financialStatementList.First(x => x.Name == statement);
                    var financeInDb = entityFinanceListInDb.FirstOrDefault(x => x.FinancialStatement.Name.Equals(statement, StringComparison.InvariantCultureIgnoreCase));

                    if (financeInDb == null)
                    {
                        // Add
                        var newFinances = new EntityFinance
                        {
                            CreatedByUserId = currentUser.Id,
                            CreatedOn = DateTime.UtcNow,

                            EntityId = redirectUrlCallbackData.EntityId,
                            FinancialInformationJson = !currentStatement.IsAutoCalculated ? financialReportList.First(x => x.ReportName.Equals(statement, StringComparison.InvariantCultureIgnoreCase)).ReportJson : null,
                            IntegratedServiceConfigurationId = thirdPartyFinanceService.Id,
                            FinancialStatementId = financialStatementList.First(x => x.Name.Equals(statement, StringComparison.InvariantCultureIgnoreCase)).Id,
                            ThirdPartyWiseCompanyName = financialReportList.FirstOrDefault()?.ThirdPartyWiseCompanyName,
                            IsDataEmpty = false
                        };

                        newFinanceList.Add(newFinances);

                    }
                    else
                    {
                        // Update
                        financeInDb.IntegratedServiceConfigurationId = thirdPartyFinanceService.Id;
                        financeInDb.UpdatedByUserId = currentUser.Id;
                        financeInDb.UpdatedOn = DateTime.UtcNow;
                        financeInDb.FinancialInformationJson = !currentStatement.IsAutoCalculated ? financialReportList.First(x => x.ReportName.Equals(statement, StringComparison.InvariantCultureIgnoreCase)).ReportJson : null;
                        financeInDb.ThirdPartyWiseCompanyName = financialReportList.FirstOrDefault()?.ThirdPartyWiseCompanyName;
                        financeInDb.IsDataEmpty = false;
                        updateFinanceList.Add(financeInDb);
                    }


                }
                await _dataRepository.AddRangeAsync(newFinanceList);
                _dataRepository.UpdateRange(updateFinanceList);
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Company, redirectUrlCallbackData.EntityId);

                _dataRepository.EnableChangeTracking();
                await _dataRepository.SaveChangesAsync(auditLog);
                _dataRepository.CommitTransaction();
            }
            // Trigger Aws SQS service to invoke background job of mapping and preparing standard finances
            var obj = new
            {
                EntityId = redirectUrlCallbackData.EntityId
            };
            if (_configuration.GetValue<string>("Environment") != "local")
                await _amazonServicesUtility.TriggerQueueAsync(JsonConvert.SerializeObject(obj));


        }

        /// <summary>
        /// Map standard chart of accounts
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public async Task MapToStandardChartOfAccountsAsync(Guid entityId)
        {
            // Fetch entity finances for given entityId
            var entityFinanceList = await _dataRepository.Fetch<EntityFinance>(x => x.EntityId == entityId && x.LoanApplicationId == null)
                .Include(x => x.Entity.Company)
                .Include(x => x.IntegratedServiceConfiguration)
                .Include(x => x.FinancialStatement)
                .ToListAsync();

            List<string> financialStatementList = new List<string>() { StringConstant.IncomeStatement, StringConstant.BalanceSheet };
            var entityFinanceFromDb = entityFinanceList.Where(x => financialStatementList.Contains(x.FinancialStatement.Name, StringComparer.InvariantCultureIgnoreCase)).ToList();

            var financialAccounts = new List<PeriodicFinancialAccountsAC>();

            foreach (var entityFinance in entityFinanceFromDb)
            {
                if (entityFinance.IntegratedServiceConfiguration.Name.Equals(StringConstant.Quickbooks, StringComparison.InvariantCultureIgnoreCase))
                {

                    FetchQuickbooksChartOfAccounts(entityFinance, financialAccounts);

                }
                if (entityFinance.IntegratedServiceConfiguration.Name.Equals(StringConstant.Xero, StringComparison.InvariantCultureIgnoreCase))
                {

                    await FetchXeroChartOfAccountsAsync(entityFinance, financialAccounts);

                }

            }
            // Get standard jamoon chart of account for respective statement from drools
            // Prepare HTTP request
            var inputObj = new
            {
                QboAccountsWithAmounts = financialAccounts
            };

            var dmnDecision = await _rulesUtility.ExecuteRuleForFetchingFinanceStandardAccountsAsync(inputObj);
            string dmnDecisionStatement;
            if (dmnDecision != null)
            {
                dmnDecisionStatement = dmnDecision.First()[StringConstant.Statement].ToString();
            }
            else
            {
                dmnDecisionStatement = null;
            }

            using (await _dataRepository.BeginTransactionAsync())
            {
                if (dmnDecision != null && dmnDecisionStatement != StringConstant.SquareBrackets)
                {
                    var entityFinanceIdList = entityFinanceList.Select(x => x.Id).Distinct().ToList();
                    var entityFinanceYearlyMappingListToRemove = await _dataRepository.Fetch<EntityFinanceYearlyMapping>(x => entityFinanceIdList.Contains(x.EntityFinanceId)).ToListAsync();
                    if (entityFinanceYearlyMappingListToRemove.Any())
                    {
                        // Delete those entries
                        _dataRepository.RemoveRange(entityFinanceYearlyMappingListToRemove);
                    }

                    foreach (var entityFinance in entityFinanceList)
                    {
                        entityFinance.IsDataEmpty = false;
                    }
                    var entityFinanceYearlyMappingListToAdd = new List<EntityFinanceYearlyMapping>();
                    foreach (var statement in dmnDecision)
                    {
                        var statementList = statement[StringConstant.Statement];

                        var periodicFinances = JsonConvert.DeserializeObject<List<PeriodicFinancialAccountsAC>>(JsonConvert.SerializeObject(statementList));

                        entityFinanceYearlyMappingListToAdd.AddRange(SaveEntityFinanceYearlyMapping(periodicFinances, entityFinanceList));
                    }
                    await _dataRepository.AddRangeAsync(entityFinanceYearlyMappingListToAdd);
                }
                else
                {
                    foreach (var entityFinance in entityFinanceList)
                    {
                        entityFinance.IsDataEmpty = true;
                    }
                }

                _dataRepository.UpdateRange(entityFinanceList);
                await _dataRepository.SaveChangesAsync();
                _dataRepository.CommitTransaction();
            }
        }

        #region Personal Finances

        /// <summary>
        /// Method to fetch personal finances of all shareholders' linked with given loan application.
        /// </summary>
        /// <param name="applicationId">Loan application id</param>
        /// <param name="scopeCsv">List of scopes of required data</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        public async Task<List<EntityAC>> FetchPersonalFinancesForApplicationAsync(Guid applicationId, string scopeCsv, CurrentUserAC currentUser)
        {
            var entities = new List<EntityAC>();
            //Check if the current user has access to the given loan application
            if (!await _globalRepository.CheckUserLoanAccessAsync(currentUser, applicationId, !currentUser.IsBankUser))
            {
                throw new InvalidResourceAccessException();
            }

            List<EntityFinance> entityFinancesOfShareholders;
            var loanApplication = await _dataRepository.Fetch<LoanApplication>(x => x.Id.Equals(applicationId))
                .Include(i => i.EntityLoanApplicationMappings).ThenInclude(i => i.Entity).ThenInclude(i => i.PrimaryEntityRelationships)
                .Include(i => i.EntityLoanApplicationMappings).ThenInclude(i => i.Entity.Company.CompanyStructure)
                .FirstOrDefaultAsync();

            //Check if the status of loan is Draft then fetch finances of shareholders where loan id is null else fetch finances for latest version of the loan
            if (loanApplication.Status.Equals(LoanApplicationStatusType.Draft))
            {
                var shareholderIds = new List<Guid>();
                loanApplication.EntityLoanApplicationMappings.ForEach(x => x.Entity.PrimaryEntityRelationships.ForEach(x => shareholderIds.Add(x.RelativeEntityId)));

                //Fetch finances of shareholders where loan id is null
                entityFinancesOfShareholders = await _dataRepository.Fetch<EntityFinance>(x => x.LoanApplicationId == null && shareholderIds.Contains(x.EntityId))
                                            .Include(i => i.Entity).ThenInclude(i => i.User).ToListAsync();
            }
            else
            {
                //Fetch all latest finances for the given application (where entity type is User to fetch the personal finances)
                entityFinancesOfShareholders = await _dataRepository.Fetch<EntityFinance>(x => x.LoanApplicationId == applicationId && x.Entity.Type.Equals(EntityType.User)).GetLatestVersionForLoan()
                                            .Include(i => i.Entity).ThenInclude(i => i.User)
                                            .ToListAsync();
            }


            if (loanApplication.EntityLoanApplicationMappings.Count == 1 && !loanApplication.EntityLoanApplicationMappings.Any(x => x.Entity.Company.CompanyStructure.Structure == StringConstant.Proprietorship) && !entityFinancesOfShareholders.Any())
            {
                throw new DataNotFoundException();
            }
            else if (entityFinancesOfShareholders.Any())
            {

                foreach (var entityFinance in entityFinancesOfShareholders)
                {
                    var entity = new EntityAC
                    {
                        Id = entityFinance.EntityId,
                        Type = entityFinance.Entity.Type,
                        User = _mapper.Map<UserAC>(entityFinance.Entity.User)
                    };

                    //If loan application is not in Draft status, then summary JSON would be available as JSON in EntityFinance table.
                    if (!loanApplication.Status.Equals(LoanApplicationStatusType.Draft) && scopeCsv.Equals(StringConstant.PersonalFinanceSummary))
                    {
                        entity.PersonalFinance = JsonConvert.DeserializeObject<PersonalFinanceAC>(entityFinance.FinancialInformationJson);
                    }
                    else
                    {
                        entity.PersonalFinance = await GetScopedPersonalFinancesAsync(scopeCsv.Split(",").ToList(), entityFinance);
                    }
                    entities.Add(entity);
                }

            }
            return entities;
        }

        /// <summary>
        /// Method to get the personal finaces
        /// </summary>
        /// <param name="entityId">User id</param>
        /// <param name="scopeCsv">Comma separated list of scopes of required data</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>PersonalFinanceAC object</returns>
        public async Task<PersonalFinanceAC> GetPersonalFinancesAsync(Guid entityId, string scopeCsv, CurrentUserAC currentUser)
        {
            //Check if current user is fetching the finances of self only and not of others.
            var userForGivenId = await _dataRepository.SingleOrDefaultAsync<DomainModel.Models.EntityInfo.User>(x => x.Id.Equals(entityId));
            if (!currentUser.IsBankUser && userForGivenId != null && !entityId.Equals(currentUser.Id))
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            //Check if valid scope/(s) given in csv
            List<string> scopes = string.IsNullOrEmpty(scopeCsv) ? new List<string>() : scopeCsv.Split(",").ToList();
            if (!scopes.Any() || (scopes.Any() && (!scopes.Contains(StringConstant.PersonalFinanceDetails) && !scopes.Contains(StringConstant.PersonalFinanceSummary))))
            {
                throw new InvalidParameterException(StringConstant.MissingScopesForPersonalFinancesData);
            }

            //Fetch personal finances for given entity
            var statement = await _dataRepository.SingleOrDefaultAsync<FinancialStatement>(x => x.Name.Equals(StringConstant.PersonalFinances));
            if (statement == null)
            {
                throw new DataNotFoundException(StringConstant.DataNotFound);
            }

            var personalFinance = new PersonalFinanceAC();

            //All objects that are queryable are intentionally kept queryable.
            //Fetch all personal entity finances for given entity id.
            var entityFinances = _dataRepository.Fetch<EntityFinance>(x => x.EntityId.Equals(entityId) && x.FinancialStatementId.Equals(statement.Id));

            //If no any personal finances are there for given entity then prepare blank object and return it.
            if (!entityFinances.Any())
            {
                personalFinance = await GetScopedPersonalFinancesAsync(scopes, null);

            }//If any record of personal finance is there then check below conditions.
            else
            {
                EntityFinance entityFinance;
                //If any record of personal finance is there with null loan application id then use it to map and return personal finance.
                if (entityFinances.Any(x => x.LoanApplicationId == null))
                {
                    entityFinance = entityFinances.Include(i => i.PersonalFinanceResponses).SingleOrDefault(x => x.LoanApplicationId == null);
                    personalFinance = await GetScopedPersonalFinancesAsync(scopes, entityFinance);

                }//If no any record found with loan id null then use the latest versioned record of personal finance of this entity.
                else
                {
                    // Disabling change tracking to improve performance
                    _dataRepository.DisableChangeTracking();

                    //Fetch the latest finances (saved for previous loan) and save a copy of it with loan id null in entity finance.
                    var latestFinances = entityFinances.GetLatestVersionForLoan().Include(i => i.PersonalFinanceResponses);
                    if (latestFinances.Any())
                    {
                        //Detach entity finance
                        var queryableFinance = _dataRepository.DetachEntities(latestFinances).Single();

                        //Fetch all personal finance responses as quearyable for given entity finance.
                        var personalFinanceResponseIdList = await latestFinances.SelectMany(x => x.PersonalFinanceResponses).Select(x => x.Id).ToListAsync();
                        var queryablePersonalFinanceResponseList = _dataRepository.Fetch<PersonalFinanceResponse>(x => personalFinanceResponseIdList.Contains(x.Id));

                        //Detach personal finance responses and assign new entityfinance id to each response
                        queryableFinance.PersonalFinanceResponses = _dataRepository.DetachEntities(queryablePersonalFinanceResponseList);
                        queryableFinance.PersonalFinanceResponses.ForEach(x => x.EntityFinanceId = queryableFinance.Id);

                        //Add the new entry of entity finance and its responses
                        await _dataRepository.AddAsync(queryableFinance);
                        _dataRepository.EnableChangeTracking();
                        await _dataRepository.SaveChangesAsync();

                        //Fetch newly added entity finance
                        var newlyAddedEntityFinance = await _dataRepository.Fetch<EntityFinance>(x => x.EntityId.Equals(entityId) && x.LoanApplicationId == null)
                                                        .Include(i => i.PersonalFinanceResponses).SingleAsync();

                        //Fetch scoped data of personal finance for newly added entity finance.
                        personalFinance = await GetScopedPersonalFinancesAsync(scopes, newlyAddedEntityFinance);
                    }
                }
            }
            //Map financial statement name in the object (Accounts and Summary are assigned in GetScopedPersonalFinance method)
            personalFinance.FinancialStatement = statement.Name;
            return personalFinance;
        }

        /// <summary>
        /// Method to add or update personal finances
        /// </summary>
        /// <param name="entityId">User id</param>
        /// <param name="categoryAC">Category object containing attributes and answers</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        public async Task SavePersonalFinancesAsync(Guid entityId, PersonalFinanceCategoryAC categoryAC, CurrentUserAC currentUser)
        {
            //Check if current user is saving the finances of self only and not of others.
            if (!entityId.Equals(currentUser.Id))
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            //Check if personal finance statement exists in DB or entityId is provided as empty Guid.
            var statement = await _dataRepository.SingleOrDefaultAsync<FinancialStatement>(x => x.Name.Equals(StringConstant.PersonalFinances));
            if (statement == null || entityId.Equals(Guid.Empty))
            {
                throw new InvalidParameterException(StringConstant.InvalidDataProvidedInRequest);
            }

            //Validate personal finance response.
            List<PersonalFinanceAttributeAC> attributes = await ValidatePersonalFinanceResponseAsync(categoryAC);

            //If no any attributes are there in request to add then no any operation will be performed.
            if (attributes.Any())
            {
                //Fetch all category-attribute mappings to map their Ids with responses to save them in DB.
                var categoryAttributeMappings = await _dataRepository.Fetch<PersonalFinanceAttributeCategoryMapping>(x => x.CategoryId.Equals(categoryAC.Id) && attributes.Select(x => x.Id).Distinct().Contains(x.AttributeId)).ToListAsync();

                var existingFinance = await _dataRepository.SingleOrDefaultAsync<EntityFinance>(x => x.EntityId.Equals(entityId) && x.LoanApplicationId == null);
                //If finance is not available in DB then Add, else check for add or update based on category
                if (existingFinance == null)
                {
                    //Add an entry in EntityFinance table for PersonalFinance
                    var entityFinance = new EntityFinance
                    {
                        EntityId = entityId,
                        FinancialStatementId = statement.Id,
                        IsDataEmpty = false,
                        CreatedByUserId = currentUser.Id,
                        CreatedOn = DateTime.UtcNow
                    };
                    await _dataRepository.AddAsync<EntityFinance>(entityFinance);

                    await AddPersonalFinanceAsync(entityFinance, categoryAC, categoryAttributeMappings, currentUser);
                }
                else
                {
                    //Fetch all the existing responses saved for given entity id.
                    var existingResponses = await _dataRepository.Fetch<PersonalFinanceResponse>(x => x.EntityFinanceId.Equals(existingFinance.Id))
                                                        .Include(i => i.PersonalFinanceAttributeCategoryMapping).ToListAsync();
                    var existingResponsesForThisCategory = existingResponses.Where(x => x.PersonalFinanceAttributeCategoryMapping.CategoryId.Equals(categoryAC.Id)).ToList();

                    //If no responses found for given category then add responses, otherwise update responses
                    if (!existingResponses.Any() || !existingResponsesForThisCategory.Any())
                    {
                        await AddPersonalFinanceAsync(existingFinance, categoryAC, categoryAttributeMappings, currentUser);
                    }
                    else
                    {
                        //Remove existing records and save the new ones
                        _dataRepository.RemoveRange<PersonalFinanceResponse>(existingResponsesForThisCategory);
                        await AddPersonalFinanceAsync(existingFinance, categoryAC, categoryAttributeMappings, currentUser);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Private methods

        #region Personal Finance

#nullable enable

        /// <summary>
        /// Method to fetch personal finances for the required scropes
        /// </summary>
        /// <param name="scopes">Requied scopes of data</param>
        /// <param name="entityFinance">Entity finance object</param>
        /// <returns>PersonalFinanceAC object</returns>
        private async Task<PersonalFinanceAC> GetScopedPersonalFinancesAsync(List<string> scopes, EntityFinance? entityFinance)
        {
#nullable disable

            PersonalFinanceAC personalFinance = new PersonalFinanceAC
            {
                Accounts = new List<PersonalFinanceAccountAC>()
            };

            var accounts = await _dataRepository.Fetch<PersonalFinanceAccount>(x => x.IsEnabled)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.PersonalFinanceAttributeCategoryMappings).ThenInclude(i => i.Attribute)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.PersonalFinanceAttributeCategoryMappings).ThenInclude(i => i.PersonalFinanceResponses).ThenInclude(i => i.PersonalFinanceAttributeCategoryMapping).ThenInclude(i => i.Attribute)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.PersonalFinanceAttributeCategoryMappings).ThenInclude(i => i.PersonalFinanceResponses).ThenInclude(i => i.PersonalFinanceAttributeCategoryMapping).ThenInclude(i => i.Category)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.PersonalFinanceAttributeCategoryMappings).ThenInclude(i => i.PersonalFinanceConstant)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.PersonalFinanceAttributeCategoryMappings).ThenInclude(i => i.ChildAttributeCategoryMappings).ThenInclude(i => i.Attribute)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.PersonalFinanceAttributeCategoryMappings).ThenInclude(i => i.ChildAttributeCategoryMappings).ThenInclude(i => i.PersonalFinanceConstant)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.MappedAsParentCategoryMappings).ThenInclude(i => i.ParentCategory)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.MappedAsParentCategoryMappings).ThenInclude(i => i.ChildCategory)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.MappedAsParentCategoryMappings).ThenInclude(i => i.ParentAttributeCategoryMapping).ThenInclude(i => i.Attribute)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.MappedAsParentCategoryMappings).ThenInclude(i => i.ParentAttributeCategoryMapping).ThenInclude(i => i.PersonalFinanceConstant)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.MappedAsChildCategoryMappings).ThenInclude(i => i.ParentCategory)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.MappedAsChildCategoryMappings).ThenInclude(i => i.ChildCategory)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.MappedAsChildCategoryMappings).ThenInclude(i => i.ParentAttributeCategoryMapping).ThenInclude(i => i.Attribute)
                                .Include(x => x.PersonalFinanceCategories).ThenInclude(x => x.MappedAsChildCategoryMappings).ThenInclude(i => i.ParentAttributeCategoryMapping).ThenInclude(i => i.PersonalFinanceConstant)
                                .ToListAsync();

            //Check which scope of data is required
            if (scopes.Contains(StringConstant.PersonalFinanceDetails))
            {
                //Fetch personal finance for an entity and if not found then return static data without answer.
                if (entityFinance == null)
                {
                    foreach (var account in accounts.Where(x => x.IsEnabled))
                    {
                        personalFinance.Accounts.Add(_mapper.Map<PersonalFinanceAccountAC>(account));
                        personalFinance.Accounts.First(x => x.Id.Equals(account.Id)).Categories = new List<PersonalFinanceCategoryAC>();
                        personalFinance.Accounts.First(x => x.Id.Equals(account.Id)).Categories.AddRange(MapCategories(account.PersonalFinanceCategories, null));
                    }
                }
                else
                {
                    if (!entityFinance.PersonalFinanceResponses.Any())
                    {
                        throw new DataNotFoundException(StringConstant.DataNotFound);
                    }

                    foreach (var account in accounts.Where(x => x.IsEnabled))
                    {
                        personalFinance.Accounts.Add(_mapper.Map<PersonalFinanceAccountAC>(account));
                        personalFinance.Accounts.First(x => x.Id.Equals(account.Id)).Categories = new List<PersonalFinanceCategoryAC>();
                        personalFinance.Accounts.First(x => x.Id.Equals(account.Id)).Categories.AddRange(MapCategories(account.PersonalFinanceCategories, entityFinance.Id));
                    }

                    //Add summary of finances if asked in scope
                    if (scopes.Contains(StringConstant.PersonalFinanceSummary))
                    {
                        personalFinance.Summary = CalculatePersonalFinanceSummary(accounts, entityFinance.Id);
                    }
                }
            } //If only summary of finances is needed
            else if (!scopes.Contains(StringConstant.PersonalFinanceDetails) && scopes.Contains(StringConstant.PersonalFinanceSummary))
            {
                if (entityFinance != null)
                {
                    if (entityFinance.PersonalFinanceResponses.Any())
                    {
                        personalFinance.Summary = CalculatePersonalFinanceSummary(accounts, entityFinance.Id);
                    }
                    else
                    {
                        throw new DataNotFoundException(StringConstant.DataNotFound);
                    }
                }
                else
                {
                    throw new DataNotFoundException(StringConstant.DataNotFound);
                }
            }
            return personalFinance;
        }

        /// <summary>
        /// Method to calculate summary of personal finances
        /// </summary>
        /// <param name="accounts">List of accounts</param>
        /// <param name="entityFinanceId">EntityFinance id</param>
        /// <returns>PersonalFinanceSummaryAC object</returns>
        private PersonalFinanceSummaryAC CalculatePersonalFinanceSummary(List<PersonalFinanceAccount> accounts, Guid entityFinanceId)
        {
            //Summary object having list of accounts
            var summary = new PersonalFinanceSummaryAC
            {
                Accounts = new List<PersonalFinanceAccountSummaryAC>()
            };

            foreach (var account in accounts)
            {
                //Check if account is enabled or not
                if (account.IsEnabled)
                {
                    //Map account to its AC and then its categories
                    var accountSummary = new PersonalFinanceAccountSummaryAC()
                    {
                        Name = account.Name,
                        Order = account.Order,
                        Categories = new List<PersonalFinanceCategorySummaryAC>()
                    };

                    foreach (var category in account.PersonalFinanceCategories)
                    {
                        //Check if category is enabled or not
                        if (category.IsEnabled)
                        {
                            var tempCategory = new PersonalFinanceCategorySummaryAC
                            {
                                Name = category.Name,
                                Order = category.Order
                            };

                            //Fetch attributes which can be used to calculate original amount and then calculate original amount
                            var originalValueResponses = category.PersonalFinanceAttributeCategoryMappings.Where(x => x.IsOriginal && x.Attribute.FieldType.Equals(PersonalFinanceAttributeFieldType.Currency))
                                                            .SelectMany(x => x.PersonalFinanceResponses.Where(y => y.EntityFinanceId.Equals(entityFinanceId))).ToList();
                            if (originalValueResponses.Any())
                            {
                                tempCategory.OriginalAmount = originalValueResponses.Where(x => !string.IsNullOrEmpty(x.Answer)).Sum(x => Convert.ToDecimal(x.Answer));
                            }

                            //Fetch attributes which can be used to calculate current amount and then calculate current amount
                            var currentValueResponses = category.PersonalFinanceAttributeCategoryMappings.Where(x => x.IsCurrent && x.Attribute.FieldType.Equals(PersonalFinanceAttributeFieldType.Currency))
                                                            .SelectMany(x => x.PersonalFinanceResponses.Where(y => y.EntityFinanceId.Equals(entityFinanceId))).ToList();
                            if (currentValueResponses.Any())
                            {
                                tempCategory.CurrentAmount = currentValueResponses.Where(x => !string.IsNullOrEmpty(x.Answer)).Sum(x => Convert.ToDecimal(x.Answer));
                            }
                            accountSummary.Categories.Add(tempCategory);
                        }
                    }
                    summary.Accounts.Add(accountSummary);
                }
            }
            return summary;
        }

        /// <summary>
        /// Method to map the categories upto n-level hierarchy.
        /// </summary>
        /// <param name="categories">List of categories</param>
        /// <param name="entityFinanceId">EntityFinance id</param>
        /// <returns></returns>
        private List<PersonalFinanceCategoryAC> MapCategories(List<PersonalFinanceCategory> categories, Guid? entityFinanceId)
        {
            var personalFinanceCategories = new List<PersonalFinanceCategoryAC>();
            //Run a loop for only enabled categories
            foreach (var category in categories.Where(x => x.IsEnabled).ToList())
            {
                //Map the category object to its AC and also its attributes
                var tempCategory = _mapper.Map<PersonalFinanceCategoryAC>(category);
                tempCategory.Attributes = new List<PersonalFinanceAttributeAC>();
                tempCategory.Attributes.AddRange(MapAttributes(category.PersonalFinanceAttributeCategoryMappings.Where(x => x.ParentAttributeCategoryMappingId == null).ToList(), 1, entityFinanceId));

                tempCategory = SetChildAttributeSet(tempCategory);

                //If this category is parent category of other child categories then add those child categories
                if (category.MappedAsParentCategoryMappings.Any())
                {
                    foreach (var mapping in category.MappedAsParentCategoryMappings)
                    {
                        //Map parent attribute if it is there.
                        if (mapping.ParentAttributeCategoryMappingId != null)
                        {
                            tempCategory.ParentAttribute = MapAttributes(new List<PersonalFinanceAttributeCategoryMapping>
                            {
                                mapping.ParentAttributeCategoryMapping
                            }, 1, entityFinanceId).First();
                        }

                        //Add all child categories
                        tempCategory.ChildCategories = new List<PersonalFinanceCategoryAC>();
                        tempCategory.ChildCategories.AddRange(MapCategories(new List<PersonalFinanceCategory> {
                            mapping.ChildCategory
                        }, entityFinanceId));
                    }
                }
                personalFinanceCategories.Add(tempCategory);
            }
            return personalFinanceCategories;
        }

        /// <summary>
        /// Method to set child attribute set.
        /// </summary>
        /// <param name="tempCategory"></param>
        /// <returns></returns>
        public PersonalFinanceCategoryAC SetChildAttributeSet(PersonalFinanceCategoryAC tempCategory)
        {
            foreach (var attribute in tempCategory.Attributes)
            {
                if (attribute.ChildAttributeSets.Any())
                {
                    foreach (var childAttributeSet in attribute.ChildAttributeSets)
                    {
                        if (childAttributeSet.ChildAttributes.Any(x => x.ChildAttributeSets.Any()))
                        {
                            foreach (var childParentQuestion in childAttributeSet.ChildAttributes)
                            {
                                childParentQuestion.ChildAttributeSets = childParentQuestion.ChildAttributeSets.Where(x => x.Order == childAttributeSet.Order).ToList();
                            }
                        }
                    }

                }
            }
            return tempCategory;
        }

        /// <summary>
        /// Method to map the attributes upto n-level hierarchy.
        /// </summary>
        /// <param name="mappings">Attribute-Category mappings</param>
        /// <param name="order">Order of an attribute whose mapping are given</param>
        /// <param name="entityFinanceId">EntityFinance id</param>
        /// <returns>List of attributes of type PersonalFinanceAttributeAC</returns>
        private List<PersonalFinanceAttributeAC> MapAttributes(List<PersonalFinanceAttributeCategoryMapping> mappings, int order, Guid? entityFinanceId)
        {
            var attributes = new List<PersonalFinanceAttributeAC>();
            //Run a loop for mappings whose attributes are enabled
            foreach (var mapping in mappings.Where(x => x.Attribute.IsEnabled).ToList())
            {
                //Map the attribute object to its AC
                var tempAttribute = new PersonalFinanceAttributeAC
                {
                    Id = mapping.AttributeId,
                    Text = mapping.Attribute.Text,
                    Order = mapping.Order,
                    Constant = new PersonalFinanceConstantAC(),
                    FieldType = mapping.Attribute.FieldType,
                    Answer = entityFinanceId == null ? null : mapping.PersonalFinanceResponses?.FirstOrDefault(x => x.Order == order && x.EntityFinanceId == entityFinanceId)?.Answer,
                    ChildAttributeSets = new List<PersonalFinanceOrderedAttributeAC>()
                };

                //If constant is linked with this attribute then map it to its AC
                if (mapping.PersonalFinanceConstantId != null)
                {
                    tempAttribute.Constant = _mapper.Map<PersonalFinanceConstantAC>(mapping.PersonalFinanceConstant);
                    tempAttribute.Constant.Options = new List<PersonalFinanceConstantOptionAC>();
                    tempAttribute.Constant.Options.AddRange(_mapper.Map<List<PersonalFinanceConstantOptionAC>>((JsonConvert.DeserializeObject<List<PersonalFinanceConstantOptionSeedDataAC>>(mapping.PersonalFinanceConstant.ValueJson)).Where(x => x.IsEnabled)));
                }


                //If this attribute has child attributes then map those as well using this same method via recursion
                if (entityFinanceId != null && mapping.ChildAttributeCategoryMappings.Any())
                {
                    if (mapping.ChildAttributeCategoryMappings.Any(x => x.PersonalFinanceResponses != null && x.PersonalFinanceResponses.Any(x => x.EntityFinanceId == entityFinanceId)))
                    {
                        var responseCount = mapping.ChildAttributeCategoryMappings.First().PersonalFinanceResponses.Count(x => x.EntityFinanceId == entityFinanceId);
                        int countOrder = 1;
                        while (countOrder <= responseCount)
                        {
                            tempAttribute.ChildAttributeSets.Add(new PersonalFinanceOrderedAttributeAC
                            {
                                Order = countOrder,
                                ChildAttributes = MapAttributes(mapping.ChildAttributeCategoryMappings, countOrder, entityFinanceId)
                            });
                            countOrder++;
                        }
                    }
                    else
                    {
                        tempAttribute.ChildAttributeSets.Add(new PersonalFinanceOrderedAttributeAC
                        {
                            Order = 1,
                            ChildAttributes = MapAttributes(mapping.ChildAttributeCategoryMappings, 1, entityFinanceId)
                        });
                    }
                }
                else
                {
                    tempAttribute.ChildAttributeSets.Add(new PersonalFinanceOrderedAttributeAC
                    {
                        Order = 1,
                        ChildAttributes = MapAttributes(mapping.ChildAttributeCategoryMappings, 1, entityFinanceId)
                    });
                }

                attributes.Add(tempAttribute);
            }
            return attributes;
        }

        /// <summary>
        /// Method to fetch all N level child attributes.
        /// </summary>
        /// <param name="attributes">List of attributes</param>
        /// <returns>List of all child attributes</returns>
        private List<PersonalFinanceAttributeAC> FetchNLevelChildrenAttributes(List<PersonalFinanceAttributeAC> attributes)
        {
            //Common list of attributes
            var allAttributes = new List<PersonalFinanceAttributeAC>();

            foreach (var attribute in attributes)
            {
                //Check if this attribute has any child attribute SET or not
                if (attribute.ChildAttributeSets.Any())
                {
                    foreach (var set in attribute.ChildAttributeSets)
                    {
                        //Check if this set has child attributes or not.
                        //If present then map all child attributes using same method via recursion.
                        if (set.ChildAttributes.Any())
                        {
                            allAttributes.AddRange(set.ChildAttributes);
                            allAttributes.AddRange(FetchNLevelChildrenAttributes(set.ChildAttributes));
                        }
                    }
                }
            }
            return allAttributes;
        }

        /// <summary>
        /// Method to map all N level child attributes of given attribute to response type objects.
        /// </summary>
        /// <param name="parentAttribute">Parent attribute object</param>
        /// <param name="categoryAC">CategoryAC object</param>
        /// <param name="categoryAttributeMappings">Attribute mappings with given category</param>
        /// <returns>List of response</returns>
        private async Task<List<PersonalFinanceResponse>> MapNLevelChildAttributesWithResponse(PersonalFinanceAttributeAC parentAttribute, PersonalFinanceCategoryAC categoryAC, List<PersonalFinanceAttributeCategoryMapping> categoryAttributeMappings)
        {
            //Common list of responses
            var responses = new List<PersonalFinanceResponse>();

            foreach (var set in parentAttribute.ChildAttributeSets)
            {
                //Check if this set has child attributes
                if (set.ChildAttributes.Any())
                {
                    foreach (var childAttribute in set.ChildAttributes)
                    {
                        //Map each child attribute in the set to response type object with its answer
                        var response = new PersonalFinanceResponse
                        {
                            Order = set.Order,
                            PersonalFinanceAttributeCategoryMappingId = categoryAttributeMappings.First(x => x.AttributeId.Equals(childAttribute.Id) && x.CategoryId.Equals(categoryAC.Id)).Id
                        };

                        //If answer field is of type address then check address validity and then save
                        if (childAttribute.FieldType.Equals(PersonalFinanceAttributeFieldType.Address) && !string.IsNullOrEmpty(childAttribute.Answer))
                        {
                            childAttribute.Answer = await ValidateAndSerializeValidAddressResponse(childAttribute.Answer);
                        }
                        response.Answer = childAttribute.Answer;
                        responses.Add(response);

                        //If this child attribute also has child attributes sets, then map those using this same method via recursion.
                        if (childAttribute.ChildAttributeSets.Any())
                        {
                            responses.AddRange(await MapNLevelChildAttributesWithResponse(childAttribute, categoryAC, categoryAttributeMappings));
                        }
                    }
                }
            }
            return responses;
        }

        /// <summary>
        /// Method to check all the required validations on personal finances' response related to relevant data existance in DB.
        /// </summary>
        /// <param name="categoryAC">Category object containing attributes</param>
        /// <returns>List of all attributes provided in the response</returns>
        private async Task<List<PersonalFinanceAttributeAC>> ValidatePersonalFinanceResponseAsync(PersonalFinanceCategoryAC categoryAC)
        {
            //Check if given category exists in DB
            var category = await _dataRepository.SingleOrDefaultAsync<PersonalFinanceCategory>(x => x.Id.Equals(categoryAC.Id));
            if (category == null)
            {
                throw new InvalidParameterException();
            }

            var attributes = new List<PersonalFinanceAttributeAC>();

            //If there are no any attributes to save for given category then no operations will be done.
            if (categoryAC.Attributes.Any())
            {
                //Fetch all the attributes(including child attributes) and check whether they all exist in DB or not.
                var attributeIds = new List<Guid>();

                //Fetch all attributes including their children and then their Ids
                foreach (var attribute in categoryAC.Attributes)
                {
                    attributes.Add(attribute);
                    attributes.AddRange(FetchNLevelChildrenAttributes(new List<PersonalFinanceAttributeAC> { attribute }));
                }
                attributeIds = attributes.Select(x => x.Id).Distinct().ToList();

                //Check if all the attributes provided in response, exist in DB
                var attributesInDB = await _dataRepository.Fetch<PersonalFinanceAttribute>(x => attributeIds.Contains(x.Id)).ToListAsync();
                if (!attributeIds.Count.Equals(attributesInDB.Count))
                {
                    throw new InvalidParameterException();
                }
            }
            return attributes;
        }

        /// <summary>
        /// Method to add personal finances
        /// </summary>
        /// <param name="entityFinance">Entity finance object</param>
        /// <param name="categoryAC">Category object with attributes</param>
        /// <param name="categoryAttributeMappings">List of existing category-attribute mappings</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        private async Task AddPersonalFinanceAsync(EntityFinance entityFinance, PersonalFinanceCategoryAC categoryAC, List<PersonalFinanceAttributeCategoryMapping> categoryAttributeMappings, CurrentUserAC currentUser)
        {
            //Common list for responses
            var personalFinanceResponses = new List<PersonalFinanceResponse>();
            foreach (var parentAttribute in categoryAC.Attributes)
            {
                //Map parent attribute to response type object
                var response = new PersonalFinanceResponse
                {
                    Order = parentAttribute.Order,
                    EntityFinanceId = entityFinance.Id,
                    PersonalFinanceAttributeCategoryMappingId = categoryAttributeMappings.First(x => x.AttributeId.Equals(parentAttribute.Id) && x.CategoryId.Equals(categoryAC.Id)).Id
                };

                //If answer field is of type address then check address validity and then save
                if (parentAttribute.FieldType.Equals(PersonalFinanceAttributeFieldType.Address) && !string.IsNullOrEmpty(parentAttribute.Answer))
                {
                    parentAttribute.Answer = await ValidateAndSerializeValidAddressResponse(parentAttribute.Answer);
                }
                response.Answer = parentAttribute.Answer;
                personalFinanceResponses.Add(response);

                //If this parent attribute has child attributes then map them as well.
                if (parentAttribute.ChildAttributeSets.Any())
                {
                    var responses = await MapNLevelChildAttributesWithResponse(parentAttribute, categoryAC, categoryAttributeMappings);
                    responses.ForEach(x => x.EntityFinanceId = entityFinance.Id);
                    personalFinanceResponses.AddRange(responses);
                }
            }
            await _dataRepository.AddRangeAsync<PersonalFinanceResponse>(personalFinanceResponses);

            // Prepare the Auditlog object to save the custom fields in the dbcontext.
            AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.User, entityFinance.EntityId);
            await _dataRepository.SaveChangesAsync(auditLog);
        }

        /// <summary>
        /// Method to check the validity of given address and return JSON of valid address as an answer
        /// </summary>
        /// <param name="answer">Address JSON given in request</param>
        /// <returns>Valid address as JSON</returns>
        private async Task<string> ValidateAndSerializeValidAddressResponse(string answer)
        {
            //Deserialize parent attribute address
            var address = JsonConvert.DeserializeObject<AddressAC>(answer);
            address.StreetLine = string.Format("{0} {1} {2} {3} {4}", address.PrimaryNumber, address.StreetLine, address.StreetSuffix, address.SecondaryNumber, address.SecondaryDesignator).Trim();
            //Check valid address using smartystreet utility method
            IntegratedServiceConfiguration smartyStreetsService = await _dataRepository.SingleOrDefaultAsync<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.SmartyStreets) && x.IsServiceEnabled);
            var validAddress = _smartyStreetsUtility.GetValidatedAddress(address, smartyStreetsService.ConfigurationJson);
            if (validAddress == null)
            {
                throw new ValidationException(StringConstant.InvalidAddress);
            }

            //If valid, then again serialize that validated address and assign to the answer property of parent attribute
            var validAddressAC = _mapper.Map<Candidate, AddressAC>(validAddress);
            validAddressAC.IntegratedServiceConfigurationId = smartyStreetsService.Id;
            validAddressAC.AddressJson = System.Text.Json.JsonSerializer.Serialize(validAddress.Components);

            DefaultContractResolver contractResolverCamelCase = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            return JsonConvert.SerializeObject(validAddressAC, new JsonSerializerSettings
            {
                ContractResolver = contractResolverCamelCase,
                Formatting = Formatting.Indented
            });
        }

        #endregion

        /// <summary>
        /// Get report data from Quickbooks
        /// </summary>
        /// <param name="redirectUrlCallbackData"></param>
        /// <param name="configurationList"></param>
        /// <param name="thirdPartyFinanceService"></param>
        /// <returns></returns>
        private async Task<List<FinancialStatementsAC>> GetQuickbooksReportsAsync(ThirdPartyServiceCallbackDataAC redirectUrlCallbackData, List<ThirdPartyConfigurationAC> configurationList, IntegratedServiceConfiguration thirdPartyFinanceService)
        {
            // Fetch and save quickbooks bearer token - Token has to be saved for future mappings that happens in background 
            var bearerToken = await _quickbooksUtility.FetchQuickbooksTokensAsync(redirectUrlCallbackData);
            var quickbooksToken = configurationList.FirstOrDefault(x => x.Key.Equals("Token", StringComparison.InvariantCultureIgnoreCase));
            if (quickbooksToken == null)
            {

                configurationList.Add(new Utils.ApplicationClass.ThirdPartyConfigurationAC
                {
                    Key = "Token",
                    Value = bearerToken,
                    Path = "Quickbooks:Token"
                });

                configurationList.Add(new Utils.ApplicationClass.ThirdPartyConfigurationAC
                {
                    Key = "RealmId",
                    Value = redirectUrlCallbackData.RealmId,
                    Path = "Quickbooks:RealmId"
                });

                redirectUrlCallbackData.BearerToken = bearerToken;

            }
            else
            {
                redirectUrlCallbackData.BearerToken = bearerToken;

                var qbToken = configurationList.First(x => x.Key == "Token");
                qbToken.Value = redirectUrlCallbackData.BearerToken;
                var qbRealmId = configurationList.Find(x => x.Key == "RealmId");
                qbRealmId.Value = redirectUrlCallbackData.RealmId;

            }

            thirdPartyFinanceService.ConfigurationJson = JsonConvert.SerializeObject(configurationList);
            _dataRepository.Update(thirdPartyFinanceService);
            await _dataRepository.SaveChangesAsync();
            // Call quickbooks utility and fetch the statement
            return _quickbooksUtility.FetchQuickbooksReport(redirectUrlCallbackData);
        }

        /// <summary>
        /// Get report data from Xero
        /// </summary>
        /// <param name="redirectUrlCallbackData"></param>
        /// <param name="configurationList"></param>
        /// <param name="thirdPartyFinanceService"></param>
        /// <returns></returns>
        private async Task<List<FinancialStatementsAC>> GetXeroReportsAsync(ThirdPartyServiceCallbackDataAC redirectUrlCallbackData, List<ThirdPartyConfigurationAC> configurationList, IntegratedServiceConfiguration thirdPartyFinanceService)
        {
            // For mapping: Need to reverse because periods are in ascending order and report header comes in the descending order.
            redirectUrlCallbackData.PeriodList.Reverse();

            // Fetch and save xero token - Token has to be saved for future mappings that happens in background 
            var bearerToken = await _xeroUtility.GetXeroTokenAsync(redirectUrlCallbackData);

            var xeroToken = configurationList.FirstOrDefault(x => x.Key.Equals("Token", StringComparison.InvariantCultureIgnoreCase));
            if (xeroToken == null)
            {

                configurationList.Add(new Utils.ApplicationClass.ThirdPartyConfigurationAC
                {
                    Key = "Token",
                    Value = bearerToken.AccessToken,
                    Path = "Xero:Token"
                });

                configurationList.Add(new Utils.ApplicationClass.ThirdPartyConfigurationAC
                {
                    Key = "TenantId",
                    Value = bearerToken.Tenants.Last().TenantId.ToString(),
                    Path = "Xero:TenantId"
                });

                redirectUrlCallbackData.BearerToken = bearerToken.AccessToken;
                redirectUrlCallbackData.TenantId = bearerToken.Tenants.Last().TenantId.ToString();
            }
            else
            {
                redirectUrlCallbackData.BearerToken = bearerToken.AccessToken;
                redirectUrlCallbackData.TenantId = bearerToken.Tenants.Last().TenantId.ToString();


                var qbToken = configurationList.First(x => x.Key == "Token");
                qbToken.Value = redirectUrlCallbackData.BearerToken;
                var qbRealmId = configurationList.Find(x => x.Key == "TenantId");
                qbRealmId.Value = bearerToken.Tenants.Last().TenantId.ToString();

            }
            thirdPartyFinanceService.ConfigurationJson = JsonConvert.SerializeObject(configurationList);
            _dataRepository.Update(thirdPartyFinanceService);
            await _dataRepository.SaveChangesAsync();
            // Call xero utility and fetch the statement
            return await _xeroUtility.GetFinancialInfoAsync(redirectUrlCallbackData);
        }

        /// <summary>
        /// Fetch Quickbooks configuration
        /// </summary>
        /// <param name="redirectUrlCallbackData"></param>
        /// <param name="statementList"></param>
        /// <returns></returns>
        private async Task<ThirdPartyServiceCallbackDataAC> FetchReportFiltersAsync(ThirdPartyServiceCallbackDataAC redirectUrlCallbackData, List<string> statementList)
        {
            // Prepare meta data and filter to fetch report
            redirectUrlCallbackData.Configuration = JsonConvert.DeserializeObject<List<ThirdPartyConfigurationAC>>(redirectUrlCallbackData.ConfigurationJson);
            Company entityCompany = await _dataRepository.SingleAsync<Company>(x => x.Id.Equals(redirectUrlCallbackData.EntityId));
            string financialYearStartMonth;
            string financialYearEndMonth;
            if (entityCompany.CompanyFiscalYearStartMonth != null)
            {
                financialYearStartMonth = _globalRepository.GetMonthNameFromMonthNumber(entityCompany.CompanyFiscalYearStartMonth.Value);
                financialYearEndMonth = _globalRepository.GetMonthNameFromMonthNumber(entityCompany.CompanyFiscalYearStartMonth.Value - 1);
            }
            else
            {
                financialYearStartMonth = _configuration.GetValue<string>("FinancialYear:StartMonth");
                financialYearEndMonth = _configuration.GetValue<string>("FinancialYear:EndMonth");
            }

            redirectUrlCallbackData.PeriodList = _globalRepository.GetListOfLastNFinancialYears(financialYearStartMonth, financialYearEndMonth, _configuration.GetValue<bool>("FinancialYear:IncludeYTD"));

            if (!statementList.Contains("Chart of Accounts"))
            {
                redirectUrlCallbackData.LastYears = _configuration.GetValue<int>("FinancialYear:Years");
                var lastNYears = DateTime.UtcNow.AddYears(-redirectUrlCallbackData.LastYears);
                redirectUrlCallbackData.StartDate = new DateTime(lastNYears.Year, DateTime.ParseExact(_configuration.GetValue<string>("FinancialYear:StartMonth"), "MMMM", CultureInfo.InvariantCulture).Month, 01);
                if (_configuration.GetValue<bool>("FinancialYear:IncludeYTD"))
                {
                    redirectUrlCallbackData.EndDate = DateTime.UtcNow.Date;
                }
                else
                {
                    var lastYear = redirectUrlCallbackData.PeriodList.Select(x => new string((new string(x.Reverse().ToArray()).Substring(0, 4)).Reverse().ToArray())).OrderByDescending(y => y).First();
                    redirectUrlCallbackData.EndDate = new DateTime(Convert.ToInt32(lastYear), 12, 31, 23, 59, 59);
                }

                redirectUrlCallbackData.ReportListToFetch = new List<FinancialStatementsAC>();
                foreach (var finance in statementList.Except(StringConstant.AutocalculatedReports).ToList())
                {
                    redirectUrlCallbackData.ReportListToFetch.Add(new FinancialStatementsAC
                    {
                        ThirdPartyWiseName = finance.Equals(StringConstant.IncomeStatement, StringComparison.InvariantCultureIgnoreCase) ? StringConstant.ProfitAndLossStatement : StringConstant.BalanceSheetStatement,
                        ReportName = finance

                    });
                }
            }


            return redirectUrlCallbackData;

        }


        /// <summary>
        /// Save mapped finances
        /// </summary>
        /// <param name="entityFinancialYearlyMappingAC"></param>
        /// <param name="entityFinanceDbList"></param>
        /// <returns></returns>
        private List<EntityFinanceYearlyMapping> SaveEntityFinanceYearlyMapping(List<PeriodicFinancialAccountsAC> entityFinancialYearlyMappingAC, List<EntityFinance> entityFinanceDbList)
        {

            var reportList = entityFinancialYearlyMappingAC.Select(x => x.ReportName).Distinct().ToList();

            var entityFinanceList = entityFinanceDbList.Where(x => reportList.Contains(x.FinancialStatement.Name)).ToList();


            // Add the latest entries
            var newYearlyMappingRecordList = new List<EntityFinanceYearlyMapping>();
            foreach (var entityFinance in entityFinanceList)
            {
                // Same report for different years
                var reportToSaveFor = entityFinancialYearlyMappingAC.Where(x => x.ReportName.Equals(entityFinance.FinancialStatement.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();


                foreach (var yearlyRecord in reportToSaveFor)
                {
                    var financialStandardAccounts = new List<EntityFinanceStandardAccount>();
                    var newYearlyMappingRecord = new EntityFinanceYearlyMapping
                    {
                        EntityFinance = entityFinance,
                        Period = yearlyRecord.Period.ToString(),
                        LastAddedDateTime = DateTime.UtcNow
                    };

                    if (yearlyRecord.FinancialAccountBalances != null)
                    {
                        foreach (var account in yearlyRecord.FinancialAccountBalances)
                        {
                            financialStandardAccounts.Add(new EntityFinanceStandardAccount
                            {
                                Amount = Math.Round(account.Amount ?? 0, 2),
                                Name = account.Account.Trim(),
                                Order = account.Id,
                                ParentId = account.ParentId,
                                ExpectedValue = account.ExpectedValue,
                                SourceJson = account.Source != null && account.Source.Any() ? JsonConvert.SerializeObject(account.Source,
                                new JsonSerializerSettings
                                {
                                    DefaultValueHandling = DefaultValueHandling.Ignore
                                }) : JsonConvert.SerializeObject(new List<FinancialAccountBalanceAC>())
                            });
                        }

                        newYearlyMappingRecord.EntityFinanceStandardAccounts = financialStandardAccounts;
                        newYearlyMappingRecordList.Add(newYearlyMappingRecord);
                    }

                }

            }

            return newYearlyMappingRecordList;



        }



        /// <summary>
        /// Method to fetch chart of accounts from Quickbooks
        /// </summary>
        /// <param name="entityFinance"></param>
        /// <param name="financialAccounts"></param>
        /// <returns></returns>
        private void FetchQuickbooksChartOfAccounts(EntityFinance entityFinance, List<PeriodicFinancialAccountsAC> financialAccounts)
        {
            var quickbooksReport = JsonConvert.DeserializeObject<Intuit.Ipp.Data.Report>(entityFinance.FinancialInformationJson);
            var reportRows = quickbooksReport.Rows;

            var reportColumnHeaders = quickbooksReport.Columns.Where(x => x.ColType == "Money").ToList();
            var periodList = new List<string>();
            foreach (var header in reportColumnHeaders)
            {
                periodList.Add(header.ColTitle);
            }

            var columnThatContainsTotal = periodList.LastOrDefault(x => x.Contains("Total"));
            if (columnThatContainsTotal != null)
                periodList.Remove(columnThatContainsTotal);

            // Prepare yearly list of each account with amounts
            List<QuickbooksAccountAC> quickbooksAccounts = new List<QuickbooksAccountAC>();
            quickbooksAccounts = _quickbooksUtility.GetAccountsWithAmountFromReportRows(reportRows.ToList(), quickbooksAccounts);
            quickbooksAccounts = quickbooksAccounts.Where(x => x.Amounts.Remove(x.Amounts.First())).ToList();
            int index = 0;

            var quickbooksAccountIdList = quickbooksAccounts.Select(x => x.Id).Distinct().ToList();
            // Fetch quickbooks account details list and map all account in quickbooksAccounts with subtype name

            var configurationList = JsonConvert.DeserializeObject<List<Utils.ApplicationClass.ThirdPartyConfigurationAC>>(entityFinance.IntegratedServiceConfiguration.ConfigurationJson);
            var configurationAC = new ThirdPartyServiceCallbackDataAC
            {
                BearerToken = configurationList.First(x => x.Key == "Token").Value,
                RealmId = configurationList.First(x => x.Key == "RealmId").Value,
                Configuration = configurationList
            };

            if (quickbooksAccountIdList.Count > 0)
            {
                var accountChart = _quickbooksUtility.FetchQuickbooksChartOfAccountsById(quickbooksAccountIdList, configurationAC);

                foreach (var year in periodList)
                {
                    var financialAccount = new PeriodicFinancialAccountsAC
                    {
                        Period = year,
                        ReportName = entityFinance.FinancialStatement.Name,
                        IsXero = false
                    };

                    var financialAccountBalances = new List<FinancialAccountBalanceAC>();
                    foreach (var account in quickbooksAccounts)
                    {
                        var accountById = accountChart.FirstOrDefault(x => x.Id == account.Id);
                        if (accountById != null)
                        {
                            financialAccountBalances.Add(new FinancialAccountBalanceAC
                            {
                                Account = accountById.AccountSubType,
                                Amount = account.Amounts[index],
                                Period = year

                            });
                        }


                    }
                    financialAccount.FinancialAccountBalances = financialAccountBalances;

                    financialAccounts.Add(financialAccount);
                    index++;
                }
            }

        }

        /// <summary>
        /// Method to fetch chart of accounts from Xero
        /// </summary>
        /// <param name="entityFinance"></param>
        /// <param name="financialAccounts"></param>
        /// <returns></returns>
        private async Task FetchXeroChartOfAccountsAsync(EntityFinance entityFinance, List<PeriodicFinancialAccountsAC> financialAccounts)
        {
            var xeroReports = JsonConvert.DeserializeObject<List<ReportWithRow>>(entityFinance.FinancialInformationJson);
            var xeroReport = xeroReports[0];

            var reportRows = xeroReport.Rows.Where(s => s.Rows != null);


            var periodList = xeroReport.Rows.First(s => s.RowType == RowType.Header).Cells
                .Where(s => !string.IsNullOrEmpty(s.Value))
                .Select(s => s.Value)
                .ToList();


            // Prepare yearly list of each account with amounts
            var xeroAccounts = _xeroUtility.GetAccountsWithAmountFromReportRows(reportRows.ToList());

            int index = 0;

            var xeroAccountIdList = xeroAccounts.Select(x => x.Id).Distinct().ToList();

            // Fetch quickbooks account details list and map all account in quickbooksAccounts with subtype name
            var configurationList = JsonConvert.DeserializeObject<List<Utils.ApplicationClass.ThirdPartyConfigurationAC>>(entityFinance.IntegratedServiceConfiguration.ConfigurationJson);
            var configurationAC = new ThirdPartyServiceCallbackDataAC
            {
                BearerToken = configurationList.First(x => x.Key == "Token").Value,
                TenantId = configurationList.First(x => x.Key == "TenantId").Value,
                Configuration = configurationList
            };
            var accountChart = await _xeroUtility.FetchXeroChartOfAccountsByIdAsync(xeroAccountIdList, configurationAC);

            foreach (var year in periodList)
            {
                var financialAccount = new PeriodicFinancialAccountsAC
                {
                    Period = year,
                    ReportName = entityFinance.FinancialStatement.Name,
                    IsXero = true
                };

                var financialAccountBalances = new List<FinancialAccountBalanceAC>();
                foreach (var account in xeroAccounts)
                {
                    var accountById = accountChart.FirstOrDefault(x => x.AccountID == Guid.Parse(account.Id));
                    if (accountById != null)
                    {
                        financialAccountBalances.Add(new FinancialAccountBalanceAC
                        {
                            Account = accountById.Name,
                            Amount = account.Amounts[index],
                            Period = year
                        });
                    }


                }
                financialAccount.FinancialAccountBalances = financialAccountBalances;

                financialAccounts.Add(financialAccount);
                index++;
            }
        }

        /// <summary>
        /// Method to fetch mapped account details
        /// </summary>
        /// <param name="entityFinanceList"></param>
        /// <param name="financeAcList"></param>
        private void FetchMappedAccountDetails(List<EntityFinance> entityFinanceList, List<CompanyFinanceAC> financeAcList)
        {
            foreach (var finance in entityFinanceList)
            {
                var mappedFinance = financeAcList.First(x => x.FinancialStatement.Equals(finance.FinancialStatement.Name, StringComparison.InvariantCultureIgnoreCase));

                mappedFinance.FinancialAccounts = mappedFinance.FinancialAccounts.OrderBy(x => x.Period).ToList();

                mappedFinance.StandardAccountList = new List<StandardAccountsAC>();

                var financialAccountBalanceACs = mappedFinance.FinancialAccounts.Select(x => x.FinancialAccountBalances).ToList();
                var accountList = financialAccountBalanceACs.First().OrderBy(x => x.Order).Select(x => x.Account).Distinct().ToList();
                foreach (var account in accountList)
                {
                    var standardAccount = new StandardAccountsAC
                    {
                        Account = account,
                        IsParent = financialAccountBalanceACs.First().Any(x => x.Account == account && (x.ParentId == null || x.ParentId == 0))
                    };

                    standardAccount.Amount = new List<decimal?>();

                    standardAccount.SourceList = new List<AccountChartAC>();
                    // Show finances divided by division factor (ex in thousands)
                    foreach (var amount in financialAccountBalanceACs)
                    {
                        var respectiveEntry = amount.FirstOrDefault(x => x.Account == account);
                        if (finance.FinancialStatement.Name.Equals(StringConstant.FinancialRatios))
                        {
                            standardAccount.Amount.Add(respectiveEntry.Amount);
                        }
                        else
                        {
                            standardAccount.Amount.Add(respectiveEntry.Amount / mappedFinance.DivisionFactor);
                        }

                        if (!string.IsNullOrEmpty(respectiveEntry.SourceJson) && respectiveEntry.SourceJson != "[]")
                        {
                            var sourceJsonList = JsonConvert.DeserializeObject<List<AccountChartAC>>(respectiveEntry.SourceJson);
                            foreach (var json in sourceJsonList)
                            {
                                json.Amount = json.Amount / mappedFinance.DivisionFactor;
                                json.Period = respectiveEntry.Period;
                            }
                            standardAccount.SourceList.AddRange(sourceJsonList);
                        }

                    }
                    mappedFinance.StandardAccountList.Add(standardAccount);

                }
                mappedFinance.IsChartOfAccountMapped = true;


                mappedFinance.StandardAccountList.RemoveAll(x => !x.IsParent && x.Amount.All(y => y == 0));

            }
        }

        /// <summary>
        /// Clone the entity finance (and all related tables) versioned entries and make a non-versioned entry
        /// </summary>
        /// <param name="financeEntriesForTheEntity"></param>
        /// <returns></returns>
        private async Task CloneEntityFinanceAndRelatedTablesAsync(IQueryable<EntityFinance> financeEntriesForTheEntity)
        {
            // Disabling change tracking to improve performance
            // All objects that are queryable are intentionally kept queryable
            _dataRepository.DisableChangeTracking();

            // Fetch the latest finances (saved for any previous loan), save a copy of it with loanId null
            var latestEntityFinance = financeEntriesForTheEntity.GetLatestVersionForLoan()
                .Include(x => x.FinancialStatement)
                .Include(x => x.EntityFinanceYearlyMappings).ThenInclude(x => x.EntityFinanceStandardAccounts);
            if (latestEntityFinance.Any())
            {
                // Detach EntityFinance
                var queryableFinance = _dataRepository.DetachEntities(latestEntityFinance);
                var entityFinanceYearlyMappingIdList = await latestEntityFinance.SelectMany(x => x.EntityFinanceYearlyMappings).Select(x => x.Id).Distinct().ToListAsync();
                var queryableEntityFinanceYearlyMappingList = _dataRepository.Fetch<EntityFinanceYearlyMapping>(x => entityFinanceYearlyMappingIdList.Contains(x.Id)).Include(x => x.EntityFinanceStandardAccounts);
                var entityFinanceStandardAccountIdList = await queryableEntityFinanceYearlyMappingList.SelectMany(x => x.EntityFinanceStandardAccounts).Select(x => x.Id).Distinct().ToListAsync();
                var entityFinanceStandardAccountList = _dataRepository.Fetch<EntityFinanceStandardAccount>(x => entityFinanceStandardAccountIdList.Contains(x.Id));

                // Prepare cloned object to save as new non-versioned entry
                foreach (var finance in queryableFinance)
                {
                    var latestFinance = await latestEntityFinance.FirstAsync(x => x.FinancialStatement.Name == finance.FinancialStatement.Name);
                    finance.FinancialStatement = null;
                    var entityFinanceYearlyMappingStandardAccountIdList = await queryableEntityFinanceYearlyMappingList.Where(x => x.EntityFinanceId == latestFinance.Id).SelectMany(x => x.EntityFinanceStandardAccounts).Select(x => x.Id).Distinct().ToListAsync();
                    finance.EntityFinanceYearlyMappings = _dataRepository.DetachEntities(queryableEntityFinanceYearlyMappingList.Where(x => x.EntityFinanceId == latestFinance.Id));
                    foreach (var yearlyFinance in finance.EntityFinanceYearlyMappings)
                    {
                        yearlyFinance.EntityFinanceId = finance.Id;
                        yearlyFinance.EntityFinanceStandardAccounts = _dataRepository.DetachEntities(entityFinanceStandardAccountList.Where(x => entityFinanceYearlyMappingStandardAccountIdList.Contains(x.Id)));
                        foreach (var standardFinance in yearlyFinance.EntityFinanceStandardAccounts)
                        {
                            standardFinance.EntityFinancialYearlyMappingId = yearlyFinance.Id;
                        }
                    }
                }

                await _dataRepository.AddRangeAsync(queryableFinance);
                _dataRepository.EnableChangeTracking();
                await _dataRepository.SaveChangesAsync();
            }
        }

        #endregion

    }
}
