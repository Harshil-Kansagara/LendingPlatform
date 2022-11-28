using Amazon.S3;
using Audit.EntityFramework;
using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.AppSettings;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.ApplicationClass.Products;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankAC = LendingPlatform.Repository.ApplicationClass.Others.BankAC;

namespace LendingPlatform.Repository.Repository.GlobalHelpers
{
    public class GlobalRepository : IGlobalRepository
    {
        #region Private Variables
        private readonly IDataRepository _dataRepository;
        private readonly IConfiguration _configuration;
        private readonly IAmazonServicesUtility _amazonS3Utility;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public GlobalRepository(IDataRepository dataRepository,
            IConfiguration configuration, IAmazonServicesUtility amazonS3Utility, IMapper mapper)
        {
            _dataRepository = dataRepository;
            _configuration = configuration;
            _amazonS3Utility = amazonS3Utility;
            _mapper = mapper;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Method to fetch the required configurations from appSettings.
        /// </summary>
        /// <returns>List of AppSettingAC objects</returns>
        private List<AppSettingAC> GetListOfAppSettings()
        {
            List<AppSettingAC> appSettings = new List<AppSettingAC>();
            List<IConfigurationSection> configurationSections = new List<IConfigurationSection>();

            var loanNeedsProperties = _configuration.GetSection("LoanNeeds").GetChildren().ToList();
            if (loanNeedsProperties.Any())
            {
                configurationSections.AddRange(loanNeedsProperties);
            }

            // Add UserSelfDeclarationExpectedResponse's properties.
            var userSelfDeclarationChildSections = _configuration.GetSection("UserSelfDeclarationExpectedResponse").GetChildren().ToList();
            if (userSelfDeclarationChildSections.Any())
            {
                configurationSections.AddRange(userSelfDeclarationChildSections);
            }

            // Add currency block's properties.
            var currency = _configuration.GetSection("Currency").GetChildren().ToList();
            if (currency.Any())
            {
                configurationSections.AddRange(currency);
            }

            // Add entity block's properties.
            var entity = _configuration.GetSection("Entity").GetChildren().ToList();
            if (entity.Any())
            {
                configurationSections.AddRange(entity);
            }

            // Add tax config properties
            var taxConfigSections = _configuration.GetSection("TaxConfig").GetChildren().ToList();
            if (taxConfigSections.Any())
            {
                configurationSections.AddRange(taxConfigSections);
            }

            // Add product block's properties
            var productConfigSections = _configuration.GetSection("Product").GetChildren().ToList();
            if (productConfigSections.Any())
            {
                configurationSections.AddRange(productConfigSections);
            }

            // Add user's configurations
            var userConfigurations = _configuration.GetSection("User").GetChildren().ToList();
            if (userConfigurations.Any())
            {
                configurationSections.AddRange(userConfigurations);
            }

            // Map all the properties into required object list type.
            if (configurationSections.Any())
            {
                appSettings = _mapper.Map<List<IConfigurationSection>, List<AppSettingAC>>(configurationSections);
            }
            return appSettings;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Method to convert string xml to json and return json of JObject type.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public JObject ConvertXmlToJson(string xml)
        {
            var json = JsonConvert.SerializeXNode(XDocument.Parse(xml));
            return JObject.Parse(json);
        }

        /// <summary>
        /// Check if CIN is in valid format for USA
        /// </summary>
        /// <param name="cin"></param>
        /// <returns></returns>
        public bool IsValidCIN(string cin)
        {
            Regex rgx = new Regex(@StringConstant.CINRegexUSA);
            return rgx.IsMatch(cin);
        }


        /// <summary>
        /// Check if EIN is unique
        /// </summary>
        /// <param name="ein"></param>
        /// <returns></returns>
        public async Task<bool> IsUniqueEINAsync(string ein)
        {
            Company checkCIN = await _dataRepository.FirstOrDefaultAsync<Company>(u => u.CIN.Equals(ein));
            if (checkCIN != null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if phone number is unique
        /// </summary>
        /// <param name="phoneNumberList"></param>
        /// <returns></returns>
        public async Task IsUniquePhoneNumberAsync(List<string> phoneNumberList)
        {
            var checkUserPhoneNumber = await _dataRepository.Fetch<User>(x => phoneNumberList.Contains(x.Phone)).ToListAsync();
            List<string> invalidPhoneList = checkUserPhoneNumber.GroupBy(x => x.Phone).Where(x => x.Count() == 1).Select(s => s.Key).ToList();
            if (invalidPhoneList.Any())
            {
                throw new ValidationException(String.Format(StringConstant.NotUniquePhoneNumber, invalidPhoneList.First()));
            }
        }

        /// <summary>
        /// Method that verifies if the logged in user is related to (or has access to) the passed entityid
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="isClient">Flag to check whether the current user is client or bank user</param>
        /// <returns></returns>
        public async Task<bool> CheckEntityRelationshipMappingAsync(Guid entityId, Guid currentUserId, bool isClient = true)
        {
            // Validate only for user with customer role
            if (isClient)
            {
                // Check any relative of logged in user is the passed entityId or not from all relations of the logged in user
                return await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId == currentUserId || x.RelativeEntityId == currentUserId)
                    .AnyAsync(x => (x.PrimaryEntityId == currentUserId && x.RelativeEntityId == entityId) || (x.PrimaryEntityId == entityId && x.RelativeEntityId == currentUserId));
            }
            return true;

        }

        /// <summary>
        /// Get financial statement from statement name
        /// </summary>
        /// <param name="reportName"></param>
        /// <returns></returns>
        public async Task<FinancialStatement> GetFinancialStatementFromNameAsync(string reportName)
        {
            var financialStatement = await _dataRepository.FirstOrDefaultAsync<FinancialStatement>(x => x.Name == reportName);

            if (financialStatement == null)
            {
                throw new DataNotFoundException(StringConstant.DataNotFound);
            }
            return financialStatement;
        }

        /// <summary>
        /// Get last year period list based on app settings.
        /// </summary>
        /// <param name="financialYearStartMonth">Start month of financial year</param>
        /// <param name="financialYearEndMonth">Start month of financial year</param>
        /// <param name="currentYear">If want current year then pass true</param>
        /// <returns></returns>
        public List<string> GetListOfLastNFinancialYears(string financialYearStartMonth, string financialYearEndMonth, bool currentYear = false)
        {

            var lastNYears = _configuration.GetValue<int>("FinancialYear:Years");
            string startMonth = financialYearStartMonth;
            string endMonth = financialYearEndMonth;
            var period = new List<string>();
            string financialYear;
            while (lastNYears > 0)
            {
                financialYear = string.Concat(DateTime.ParseExact(startMonth, "MMMM", CultureInfo.InvariantCulture).ToString("MMM"), " - "
                                                   , DateTime.ParseExact(endMonth, "MMMM", CultureInfo.InvariantCulture).ToString("MMM"), " "
                                                   , DateTime.UtcNow.AddYears(-lastNYears).Year);
                period.Add(financialYear);
                lastNYears--;
            }

            if (currentYear)
            {
                financialYear = string.Concat(DateTime.ParseExact(startMonth, "MMMM", CultureInfo.InvariantCulture).ToString("MMM"), " - "
                                                      , DateTime.UtcNow.ToString("MMM"), " "
                                                      , DateTime.UtcNow.AddYears(-lastNYears).Year);
                period.Add(financialYear);
            }

            return period;
        }


        /// <summary>
        /// Method to update section name of a loan application on adding details in each component.
        /// </summary>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <param name="sectionName">Current section name</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>Updated section name</returns>
        public async Task<string> UpdateSectionNameAsync(Guid loanApplicationId, string sectionName, CurrentUserAC currentUser)
        {
            LoanApplication loanApplication = await _dataRepository.Fetch<LoanApplication>(x => x.Id == loanApplicationId)
                .Include(y => y.UserLoanSectionMappings).ThenInclude(i => i.Section)
                .Include(y => y.EntityLoanApplicationMappings).ThenInclude(x => x.Entity).ThenInclude(x => x.RelativeEntityRelationships).SingleOrDefaultAsync();
            if (loanApplication == null)
            {
                throw new DataNotFoundException(StringConstant.LoanApplicationNotExistsForGivenId);
            }

            Section loanSection = loanApplication.UserLoanSectionMappings.Any(x => x.UserId.Equals(currentUser.Id))
                                    ? loanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(currentUser.Id)).Section
                                    : loanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(loanApplication.CreatedByUserId)).Section;
            if (!sectionName.Equals(loanSection.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidResourceAccessException(StringConstant.LoanSectionNameError);
            }

            List<Section> sections = await _dataRepository.Fetch<Section>(x => x.IsEnabled).OrderBy(x => x.Order).ToListAsync();

            //Check if the sections exist.
            if (!sections.Any())
            {
                throw new DataNotFoundException(StringConstant.SectionsNotFound);
            }

            //Update the section if the current section of the loan application is not having the last section.
            if (sections.Select(x => x.Name).ToList().Contains(loanSection.Name) && loanSection.Order < sections.Last().Order)
            {
                var section = sections.ElementAt(sections.Select(x => x.Name).ToList().IndexOf(loanSection.Name) + 1);
                if (sections.Any(x => x.ParentSectionId == section.Id))
                {
                    section = sections.Where(x => x.ParentSectionId == section.Id).OrderBy(x => x.Order).First();
                }
                if (sectionName == StringConstant.PersonalFinancesSection && !loanApplication.CreatedByUserId.Equals(currentUser.Id))
                {
                    section = sections.Single(x => x.Name.Equals(StringConstant.LoanConsentSection));
                    loanApplication.UserLoanSectionMappings.Single(x => x.UserId == currentUser.Id && x.LoanApplicationId == loanApplicationId).SectionId = section.Id;
                }
                else if (section.Order > 9)
                {
                    foreach (var userLoanSectionMapping in loanApplication.UserLoanSectionMappings)
                    {
                        userLoanSectionMapping.SectionId = section.Id;
                    }
                }
                else
                {
                    loanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(currentUser.Id)).SectionId = section.Id;
                }

                loanApplication.UpdatedOn = DateTime.UtcNow;
                if (currentUser.IsBankUser)
                {
                    loanApplication.UpdatedByBankUserId = currentUser.Id;
                }
                else
                {
                    loanApplication.UpdatedByUserId = currentUser.Id;
                }
                _dataRepository.UpdateRange(loanApplication.UserLoanSectionMappings);
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = GetAuditLogForCustomFields(currentUser, ResourceType.Loan, loanApplication.Id);
                await _dataRepository.SaveChangesAsync(auditLog);
                return section.Name;
            }
            else
            {
                throw new InvalidOperationException(StringConstant.UpdateSectionNameError);
            }
        }

        /// <summary>
        /// Check if user is accessing his own loan only
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="loanApplicationId"></param>
        /// <param name="isClient">Flag to check whether the current user is client or bank user</param>
        /// <returns></returns>
        public async Task<bool> CheckUserLoanAccessAsync(CurrentUserAC currentUser, Guid loanApplicationId, bool isClient = true)
        {
            // Validate only for customer role
            if (isClient)
            {
                var loanApplication = await _dataRepository.SingleOrDefaultAsync<LoanApplication>(x => x.Id == loanApplicationId);
                // Check if the user has initiated this loan
                if (loanApplication.CreatedByUserId == currentUser.Id)
                {
                    return true;
                }

                // Check if the user's linked company(entity) has this loan
                var relatedBorrowerIdList = _dataRepository.Fetch<EntityLoanApplicationMapping>(x => x.LoanApplicationId == loanApplicationId).Select(x => x.EntityId).ToList();
                if (relatedBorrowerIdList.Contains(currentUser.Id))
                {
                    return true;
                }

                List<EntityLoanApplicationConsent> consents = await _dataRepository.Fetch<EntityLoanApplicationConsent>(x => x.LoanApplicationId == loanApplicationId).Include(x => x.LoanApplication).ToListAsync();
                // Only if loan consent is given by the loan initiator.
                if (!consents.Any(x => x.IsConsentGiven && x.ConsenteeId == loanApplication.CreatedByUserId))
                {
                    return false;
                }

                // Check if any of the borrower has relationship with the logged in user
                // Fetch all relations of the logged in user
                var borrowerRelatives = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId == currentUser.Id || x.RelativeEntityId == currentUser.Id)
                    .ToListAsync();

                // Check any relative of logged in user is the passed entityId or not
                return borrowerRelatives.Any(x => (x.PrimaryEntityId == currentUser.Id && relatedBorrowerIdList.Contains(x.RelativeEntityId)) || (relatedBorrowerIdList.Contains(x.PrimaryEntityId) && x.RelativeEntityId == currentUser.Id));
            }
            return true;
        }

        /// <summary>
        /// Fetch only those loan applications which are accessible by current user.
        /// </summary>
        /// <param name="currentUser">Current logged in user</param>
        /// <param name="loanApplicationList">List of ApplicationBasicDetailAC objects</param>
        /// <param name="isClient">Flag to check whether the current user is client or bank user</param>
        /// <returns>List of ApplicationBasicDetailAC objects</returns>
        public async Task<List<ApplicationBasicDetailAC>> CheckUserLoanAccessAsync(CurrentUserAC currentUser, List<ApplicationBasicDetailAC> loanApplicationList, bool isClient = true)
        {
            // Validate only for customer role
            if (isClient)
            {
                var loanApplicationDbList = await _dataRepository.Fetch<LoanApplication>(x => loanApplicationList.Select(y => y.Id).Contains(x.Id)).ToListAsync();
                var entityLoanApplicationMappingList = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => loanApplicationList.Select(y => y.Id).Contains(x.LoanApplicationId)).ToListAsync();
                var consentList = await _dataRepository.Fetch<EntityLoanApplicationConsent>(x => loanApplicationList.Select(y => y.Id).Contains(x.LoanApplicationId)).Include(x => x.LoanApplication).ToListAsync();
                var borrowerRelatives = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId == currentUser.Id || x.RelativeEntityId == currentUser.Id)
                    .ToListAsync();

                var loanApplicationsToAdd = new List<ApplicationBasicDetailAC>();
                foreach (var loanApplication in loanApplicationDbList)
                {
                    // Check if the user has initiated this loan
                    if (loanApplication.CreatedByUserId == currentUser.Id)
                    {
                        loanApplicationsToAdd.AddRange(loanApplicationList.Where(x => x.Id == loanApplication.Id));
                        continue;
                    }

                    // Check if the user's linked company(entity) has this loan
                    var relatedBorrowerIdList = entityLoanApplicationMappingList.Where(x => x.LoanApplicationId == loanApplication.Id).Select(x => x.EntityId).ToList();
                    if (relatedBorrowerIdList.Contains(currentUser.Id))
                    {
                        loanApplicationsToAdd.AddRange(loanApplicationList.Where(x => x.Id == loanApplication.Id));
                        continue;
                    }

                    List<EntityLoanApplicationConsent> consents = consentList.Where(x => x.LoanApplicationId == loanApplication.Id).ToList();
                    // Only if loan consent is given by the loan initiator.
                    if (!consents.Any(x => x.IsConsentGiven && x.ConsenteeId == loanApplication.CreatedByUserId))
                    {
                        continue;
                    }

                    if (borrowerRelatives.Any(x => (x.PrimaryEntityId == currentUser.Id && relatedBorrowerIdList.Contains(x.RelativeEntityId)) || (relatedBorrowerIdList.Contains(x.PrimaryEntityId) && x.RelativeEntityId == currentUser.Id)))
                    {
                        loanApplicationsToAdd.AddRange(loanApplicationList.Where(x => x.Id == loanApplication.Id));
                    }
                }
                return loanApplicationsToAdd;
            }
            return loanApplicationList;
        }

        /// <summary>
        /// Method to verify that loan application exists and in correct status to do the add/update operation.
        /// </summary>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <returns>Returns a boolean value</returns>
        public async Task<bool> IsAddOrUpdateAllowedAsync(Guid loanApplicationId)
        {
            LoanApplication loanApplication = await _dataRepository.Fetch<LoanApplication>(x => x.Id == loanApplicationId).Include(i => i.UserLoanSectionMappings).ThenInclude(x => x.Section).SingleOrDefaultAsync();
            if (loanApplication == null)
            {
                throw new DataNotFoundException(StringConstant.LoanApplicationNotExistsForGivenId);
            }

            // Check if loan is not in readonly mode. (If loan consent section then update should be allowed )
            return !(await IsLoanReadOnlyAsync(loanApplication, loanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(loanApplication.CreatedByUserId)).Section.Name == StringConstant.LoanConsentSection));
        }

        /// <summary>
        /// Check if loan is readonly.
        /// </summary>
        /// <param name="loanApplication"></param>
        /// <param name="isConsentSection"></param>
        /// <returns></returns>
        public async Task<bool> IsLoanReadOnlyAsync(LoanApplication loanApplication, bool isConsentSection = false)
        {
            if (!loanApplication.Status.Equals(LoanApplicationStatusType.Draft))
            {
                return true;
            }

            //If it is consent section then update operation will be allowed even though the initiator has given consent.
            if (!isConsentSection)
            {
                List<EntityLoanApplicationConsent> consents = await _dataRepository.Fetch<EntityLoanApplicationConsent>(x => x.LoanApplicationId == loanApplication.Id).ToListAsync();
                // Only if loan consent is given by the loan initiator.
                if (consents.Any() && consents.Any(x => x.IsConsentGiven && x.ConsenteeId == loanApplication.CreatedByUserId))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Method to update status of a loan application on consents are given by all the shareholders.
        /// </summary>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <param name="status">Status to update</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        public async Task UpdateStatusOfLoanApplicationAsync(Guid loanApplicationId, LoanApplicationStatusType status, CurrentUserAC currentUser)
        {
            LoanApplication loanApplication = await _dataRepository.SingleOrDefaultAsync<LoanApplication>(x => x.Id == loanApplicationId);
            if (loanApplication == null)
            {
                throw new DataNotFoundException(StringConstant.LoanApplicationNotExistsForGivenId);
            }

            loanApplication.Status = status;
            loanApplication.UpdatedOn = DateTime.Now;
            loanApplication.UpdatedByUserId = currentUser.Id;
            _dataRepository.Update<LoanApplication>(loanApplication);
            // Prepare the Auditlog object to save the custom fields in the dbcontext.
            AuditLog auditLog = GetAuditLogForCustomFields(currentUser, ResourceType.Loan, loanApplication.Id);
            await _dataRepository.SaveChangesAsync(auditLog);
        }

        /// <summary>
        /// Convert utc date to local date
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public DateTime ConvertUtcDateToLocalDate(DateTime utcDateTime)
        {
            var currentTimeZoneInfo = TimeZoneInfo.Local;
            var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, currentTimeZoneInfo);
            return convertedDateTime;
        }
        #endregion

        #region New implementation

        /// <summary>
        /// Method to get uploaded files using document id.
        /// </summary>
        /// <param name="documentId">Uploaded document id.</param>
        /// <returns>Url.</returns>
        public async Task<string> GetPreSignedUrlAsync(Guid documentId)
        {
            try
            {
                var objectKey = await _dataRepository.Fetch<Document>(x => x.Id == documentId).Select(x => x.Path).SingleOrDefaultAsync();

                return _amazonS3Utility.GetPreSignedURL(objectKey);
            }

            catch (AmazonS3Exception ex)
            {
                throw new AmazonS3Exception(ex.Message);
            }
        }

        /// <summary>
        /// Method to get the path for key name bucket 
        /// </summary>
        /// <param name="entityId">Entity Id</param>
        /// <param name="entityTaxAccountAC">EntityTaxAccountAC object</param>        
        /// <returns>Return path for key name bucket</returns>
        public string GetPathForKeyNameBucket(Guid entityId, EntityTaxAccountAC entityTaxAccountAC)
        {
            string filePath = $"{entityId}/{StringConstant.TaxReturns}/{entityTaxAccountAC.Period}/{entityTaxAccountAC.Document.Name}";
            return filePath;
        }

        /// <summary>
        /// Method to get the path for key name bucket (for additional documents)
        /// </summary>
        /// <param name="entityId">Entity Id</param>
        /// <param name="additionalDocument">AdditionalDocumentAC object</param>
        /// <returns>Return path for key name bucket for additional document</returns>
        public string GetPathForKeyNameBucketForAdditionalDocument(Guid entityId, AdditionalDocumentAC additionalDocument)
        {
            string fileName = $"{string.Join('.', additionalDocument.Document.Name.Split('.').SkipLast(1).ToList())}_{Guid.NewGuid()}.{additionalDocument.Document.Name.Split('.').Last()}";
            return $"{entityId}/{StringConstant.AdditionalDocuments}/{additionalDocument.DocumentType.Type}/{additionalDocument.DocumentType.DocumentTypeFor}/{fileName}";
        }

        /// <summary>
        /// Fetch query select fields as per the fields parameter.
        /// </summary>
        /// <param name="fields">request parameter fields value</param>
        /// <param name="className">Class name</param>
        /// <returns>Returns select fields from the query</returns>
        public string GetQuerySelectFieldsFromString(string fields, object className)
        {
            string selectFields = string.Empty;

            // Remove blank spaces in the fields and split with comma delimiter.
            var requestParameters = fields.Replace(" ", "").ToLower().Split(',');

            foreach (var result in requestParameters)
            {
                // Get the basic details property.
                if (result.Equals(StringConstant.RequestParameterBasicDeatail))
                {
                    var classBasicProperties = className.GetType().GetProperties().Where(s => !s.PropertyType.IsClass);
                    selectFields = string.Join(",", classBasicProperties.Select(s => s.Name));
                }

                // Get the DB field name from the parent split result.
                selectFields = GetDBFieldNameFromValue(result, selectFields);

                #region Borrowing entity
                Regex borrowingEntityRegex = new Regex(@"^borrowing-entity([\w]+)$");
                if (borrowingEntityRegex.IsMatch(result))
                {
                    // Remove "borrowing-entity(" sting in the pattern and also remove last ) braces.
                    var borrowingEntityValues = result.Substring(0, result.Length - 1).Replace("borrowing-entity(", "").Split(',');

                    foreach (var borrowingEntityValue in borrowingEntityValues)
                    {
                        // Get the DB field name from the borrowing entity split result.
                        selectFields = GetDBFieldNameFromValue(result, selectFields);
                    }
                }
                #endregion
            }
            return $"new ({selectFields})";
        }

        /// <summary>
        /// Get the request parameter class field name from the dictionary and the append in the existing fields.
        /// </summary>
        /// <param name="value">request paramter value</param>
        /// <param name="selectFields">existing select fields value</param>
        /// <returns>list of select</returns>
        private string GetDBFieldNameFromValue(string value, string selectFields)
        {
            string parameterDBValue;

            // Check the request parameter key value exist in the request parameter dictionary.
            if (StringConstant.RequestParameterDBFieldName.TryGetValue(value, out parameterDBValue))
            {
                selectFields = $"{(string.IsNullOrEmpty(selectFields) ? "" : $"{selectFields},")}{parameterDBValue}";
            }
            return selectFields;
        }

        /// <summary>
        /// Returns all the consent statements.
        /// </summary>
        /// <returns>List of ConsentStatementAC objects</returns>
        public async Task<List<ConsentStatementAC>> GetConsentStatementsAsync()
        {
            List<Consent> consents = await _dataRepository.GetAll<Consent>().ToListAsync();
            if (!consents.Any())
            {
                throw new DataNotFoundException(StringConstant.EmptyConsentTable);
            }
            return _mapper.Map<List<Consent>, List<ConsentStatementAC>>(consents);
        }

        /// <summary>
        /// Get the list of banks.
        /// </summary>
        /// <returns>List of BankAC objects</returns>
        public async Task<List<ApplicationClass.Others.BankAC>> GetAllBanksAsync()
        {
            List<Bank> banks = await _dataRepository.GetAll<Bank>().ToListAsync();
            if (!banks.Any())
            {
                throw new DataNotFoundException(StringConstant.BanksNotFound);
            }
            return _mapper.Map<List<Bank>, List<BankAC>>(banks);
        }

        /// <summary>
        /// Method fetches all the purposes with its type for the loan application.
        /// </summary>
        /// <returns>List of LoanPurposeAC</returns>
        public async Task<List<LoanPurposeAC>> GetLoanPurposeListAsync()
        {
            List<LoanPurpose> loanPurposes = await _dataRepository.GetAll<LoanPurpose>()
                .Include(i => i.LoanPurposeRangeTypeMappings).ThenInclude(i => i.LoanRangeType)
                .Include(i => i.SubLoanPurposes).ToListAsync();

            if (!loanPurposes.Any())
            {
                throw new DataNotFoundException(StringConstant.EmptyLoanPurposeTable);
            }
            return _mapper.Map<List<LoanPurposeAC>>(loanPurposes);
        }

        /// <summary>
        /// Method fetches all the UI required configurations.
        /// </summary>
        /// <returns>ConfigurationAC object</returns>
        public async Task<ConfigurationAC> GetAllConfigurationsAsync()
        {
            ConfigurationAC configuration = new ConfigurationAC();

            List<Section> sections = await _dataRepository.Fetch<Section>(x => x.IsEnabled).Include(x => x.ParentSection).ToListAsync();
            if (sections.Any())
            {
                var parentSections = sections.Where(x => x.ParentSectionId == null).ToList();
                configuration.Sections = _mapper.Map<List<SectionAC>>(parentSections);
                var childSections = sections.Where(x => x.ParentSectionId != null).ToList();
                foreach (var parent in configuration.Sections)
                {
                    parent.ChildSection = new List<SectionAC>();
                }
                foreach (var child in childSections)
                {
                    configuration.Sections.First(x => x.Id == child.ParentSectionId).ChildSection.Add(_mapper.Map<SectionAC>(child));
                }

                List<string> services = await _dataRepository.Fetch<IntegratedServiceConfiguration>(x => x.IsServiceEnabled).Select(x => x.Name).ToListAsync();
                configuration.ThirdPartyServices = services.Any() ? services : new List<string>();
                configuration.AppSettings = GetListOfAppSettings();

                return configuration;
            }
            else
            {
                throw new DataNotFoundException(StringConstant.SectionsNotFound);
            }
        }

        /// <summary>
        /// Get all list of industry groups
        /// </summary>
        /// <returns>List of NAICSIndustryTypeAC object</returns>
        public async Task<List<NAICSIndustryTypeAC>> GetIndustryGroupListAsync()
        {
            List<NAICSIndustryTypeAC> industrySectorList = await _dataRepository.Fetch<NAICSIndustryType>(x => x.NAICSParentSectorId != null).Select(s => new NAICSIndustryTypeAC { Id = s.Id, IndustryCode = s.IndustryCode, IndustryType = s.IndustryType }).ToListAsync();
            return industrySectorList;
        }

        /// <summary>
        /// Get all list of business age ranges
        /// </summary>
        /// <returns>List of business age object</returns>
        public async Task<List<BusinessAgeAC>> GetBusinessAgeRangeListAsync()
        {
            List<BusinessAgeAC> companyAgeList = await _dataRepository.GetAll<BusinessAge>().Select(s => new BusinessAgeAC { Id = s.Id, Age = s.Age, Order = s.Order }).ToListAsync();
            return companyAgeList;
        }

        /// <summary>
        /// Get all list of company structure
        /// </summary>
        /// <returns>List of company structure object</returns>
        public async Task<List<CompanyStructureAC>> GetCompanyStructureListAsync()
        {
            List<CompanyStructureAC> companyStructureList = await _dataRepository.GetAll<CompanyStructure>().Select(s => new CompanyStructureAC { Id = s.Id, Structure = s.Structure, Order = s.Order }).ToListAsync();
            return companyStructureList;
        }

        /// <summary>
        /// Get all list of Company size ranges
        /// </summary>
        /// <returns>List of company size object</returns>
        public async Task<List<CompanySizeAC>> GetCompanySizeListAsync()
        {
            List<CompanySizeAC> companySizeList = await _dataRepository.GetAll<CompanySize>().Select(s => new CompanySizeAC { Id = s.Id, Size = s.Size, Order = s.Order, IsEnabled = s.IsEnabled }).ToListAsync();
            return companySizeList;
        }

        /// <summary>
        /// Get all list of Industry experience ranges
        /// </summary>
        /// <returns>List of Industry experience object</returns>
        public async Task<List<IndustryExperienceAC>> GetIndustryExperienceListAsync()
        {
            List<IndustryExperienceAC> industryExperienceList = await _dataRepository.GetAll<IndustryExperience>().Select(s => new IndustryExperienceAC { Id = s.Id, Experience = s.Experience, Order = s.Order, IsEnabled = s.IsEnabled }).ToListAsync();
            return industryExperienceList;
        }

        /// <summary>
        /// Get description of enum value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(T value) where T : Enum
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Method to fetch the interest rate based on given self declared credit score.
        /// </summary>
        /// <param name="creditScore">Credit score</param>
        /// <returns>Interest rate</returns>
        public decimal? GetInterestRateForGivenSelfDeclaredCreditScore(string creditScore)
        {
            decimal? interestRate = null;

            if (string.IsNullOrEmpty(creditScore))
            {
                return null;
            }

            //Return the interest value based on the credit score given
            if (creditScore.Equals(StringConstant.ExcellentCreditScore))
            {
                interestRate = _configuration.GetValue<decimal?>("LoanNeeds:InterestRateForExcellentCreditScore");
            }
            else if (creditScore.Equals(StringConstant.GoodCreditScore))
            {
                interestRate = _configuration.GetValue<decimal?>("LoanNeeds:InterestRateForGoodCreditScore");
            }
            else if (creditScore.Equals(StringConstant.FairCreditScore))
            {
                interestRate = _configuration.GetValue<decimal?>("LoanNeeds:InterestRateForFairCreditScore");
            }
            else if (creditScore.Equals(StringConstant.AverageCreditScore))
            {
                interestRate = _configuration.GetValue<decimal?>("LoanNeeds:InterestRateForAverageCreditScore");
            }
            else if (creditScore.Equals(StringConstant.PoorCreditScore) || creditScore.Equals(StringConstant.notKnown))
            {
                interestRate = _configuration.GetValue<decimal?>("LoanNeeds:InterestRateForPoorCreditScore");
            }

            //If interest is null then throw an exception
            if (interestRate == null)
            {
                throw new ConfigurationNotFoundException(StringConstant.InterestRateNotFoundInConfigurations);
            }
            return interestRate;
        }

        /// <summary>
        /// Method is to calculate the loan product detail.
        /// </summary>
        /// <param name="loanProductRangeTypeMappings">List of LoanProductRangeTypeMapping </param>
        /// <param name="currentCurrencySymbol">Currency symbol</param>
        /// <param name="loanApplication">Loan application</param>
        /// <returns>Object of loanProductDetailAC</returns>
        public ProductDetailsAC CalculateLoanProductDetail(List<ProductRangeTypeMapping> loanProductRangeTypeMappings, string currentCurrencySymbol, LoanApplication loanApplication)
        {
            ProductDetailsAC productDetailAC = new ProductDetailsAC();

            if (string.IsNullOrEmpty(loanApplication.CreatedByUser.SelfDeclaredCreditScore))
            {
                throw new InvalidParameterException(StringConstant.LoanInitiatorOrSelfDeclaredCreditScoreNotFound);
            }

            if (!loanApplication.Status.Equals(LoanApplicationStatusType.Draft) && !loanApplication.Status.Equals(LoanApplicationStatusType.Unlocked))
            {
                productDetailAC.InterestRate = loanApplication.InterestRate ?? 0;
            }
            else
            {
                productDetailAC.InterestRate = GetInterestRateForGivenSelfDeclaredCreditScore(loanApplication.CreatedByUser.SelfDeclaredCreditScore).Value;
            }

            // Set product term
            productDetailAC.MinProductTenure = loanProductRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.Lifecycle).Minimum / 12;
            productDetailAC.MaxProductTenure = loanProductRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.Lifecycle).Maximum / 12;

            // Set product amount
            productDetailAC.MinProductAmount = loanProductRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.LoanAmount).Minimum;
            productDetailAC.MaxProductAmount = loanProductRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.LoanAmount).Maximum;

            // Calculate monthly payment
            productDetailAC.MonthlyPayment = CalculateEMI(loanApplication.LoanAmount, loanApplication.LoanPeriod * 12, productDetailAC.InterestRate);
            productDetailAC.MinMonthlyPayment = CalculateEMI(loanApplication.LoanAmount, productDetailAC.MaxProductTenure * 12, productDetailAC.InterestRate);
            productDetailAC.MaxMonthlyPayment = CalculateEMI(loanApplication.LoanAmount, productDetailAC.MinProductTenure * 12, productDetailAC.InterestRate);

            // Loan Amount
            productDetailAC.Amount = loanApplication.LoanAmount;

            // Loan Period 
            productDetailAC.Period = loanApplication.LoanPeriod;

            // Calculate total payment and total interest
            var totalPayment = Math.Round(productDetailAC.MonthlyPayment * loanApplication.LoanPeriod * 12, 2);
            var totalInterest = Math.Round(totalPayment - loanApplication.LoanAmount, 2);

            productDetailAC.TotalPayment = String.Format("{0} {1}", currentCurrencySymbol, ((double)(totalPayment)).ToString("C", CultureInfo.CreateSpecificCulture("en-US")).Split('$')[1]);
            productDetailAC.TotalInterest = String.Format("{0} {1}", currentCurrencySymbol, ((double)(totalInterest)).ToString("C", CultureInfo.CreateSpecificCulture("en-US")).Split('$')[1]);

            return productDetailAC;
        }

        /// <summary>
        /// Method is to calculate the loan EMI.
        /// </summary>
        /// <param name="amount">Loan amount</param>
        /// <param name="period">Loan period</param>
        /// <param name="yearlyRate">Interest rate</param>
        /// <returns>Monthky payment value</returns>
        private decimal CalculateEMI(decimal amount, decimal period, decimal yearlyRate)
        {
            var monthlyRate = yearlyRate / (12 * 100);
            return Math.Round((amount * monthlyRate * (decimal)Math.Pow(1 + Decimal.ToDouble(monthlyRate), Decimal.ToDouble(period))) / (decimal)(Math.Pow(1 + Decimal.ToDouble(monthlyRate), Decimal.ToDouble(period)) - 1), 2);
        }

        /// <summary>
        /// Prepare Audit log object for custom fields.
        /// </summary>
        /// <param name="userAC">User object</param>
        /// <param name="logBlockName">Company or Loan</param>
        /// <param name="id">Contain CompanyId or LoanId</param>
        /// <returns>AuditLog object.</returns>
        public AuditLog GetAuditLogForCustomFields(CurrentUserAC userAC, ResourceType logBlockName = ResourceType.Other, Guid? id = null)
        {
            return new AuditLog
            {
                LogBlockName = logBlockName,
                LogBlockNameId = id,
                CreatedByBankUserId = userAC.IsBankUser ? userAC.Id : (Guid?)null,
                CreatedByUserId = !userAC.IsBankUser ? userAC.Id : (Guid?)null,
                IpAddress = userAC.IpAddress
            };
        }
        /// <summary>
        /// Fetch the datewise Audit logs of the company or loan application based on the Id.
        /// </summary>
        /// <param name="loggedInUser">LoggedIn user object.</param>
        /// <param name="auditLogFilter">Audit log filter object.</param>
        /// <returns>List of datewise Auditlogs.</returns>
        public async Task<List<AuditDateWiseLogsAC>> GetAuditLogsByLogBlockNameIdAsync(CurrentUserAC loggedInUser, AuditLogFilterAC auditLogFilter)
        {
            // Check id is invalid.
            if (auditLogFilter.LogBlockNameId == Guid.Empty)
            {
                throw new InvalidParameterException();
            }

            // If user is not a bank user then thrown an exception.
            if (!loggedInUser.IsBankUser)
            {
                throw new InvalidResourceAccessException();
            }
            List<string> removeInsertTableNames = new List<string> { nameof(Address) };

            List<Guid> LinkedEntityIdList = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(auditLogFilter.LogBlockNameId)).Select(s => s.RelativeEntityId).ToListAsync();

            var auditLogs = await _dataRepository.Fetch<AuditLog>(x => LinkedEntityIdList.Contains(x.LogBlockNameId.Value) || x.LogBlockNameId == auditLogFilter.LogBlockNameId &&
                                                                  (auditLogFilter.StartDate == null || x.CreatedOn >= auditLogFilter.StartDate) &&
                                                                  (auditLogFilter.EndDate == null || x.CreatedOn <= auditLogFilter.EndDate) &&
                                                                  (x.Action != StringConstant.AuditLogInsertActionName ||
                                                                  (x.Action == StringConstant.AuditLogInsertActionName && !removeInsertTableNames.Contains(x.TableName))))
                                                                 .Include(y => y.CreatedByUser)
                                                                 .Include(b => b.CreatedByBankUser).OrderByDescending(s => s.CreatedOn).ToListAsync();

            var auditLogACs = _mapper.Map<List<AuditLogAC>>(auditLogs);
            foreach (var auditLog in auditLogACs)
            {
                // Fetch audit logs by action.
                await GetAuditLogFieldsByActionAsync(auditLog, auditLog.Id);
            }
            auditLogACs = MergeConsentDetailsByUser(auditLogACs);

            #region Datewise group the logs.
            // Remove audit log object if fields are empty.
            auditLogACs = auditLogACs.Where(s => s.AuditLogFields != null && s.AuditLogFields.Any()).ToList();
            var uniqueCreatedDates = auditLogACs.Select(s => s.CreatedOn.ToShortDateString()).Distinct();

            List<AuditDateWiseLogsAC> auditDateWiseLogs = new List<AuditDateWiseLogsAC>();
            foreach (var createdDate in uniqueCreatedDates)
            {
                auditDateWiseLogs.Add(new AuditDateWiseLogsAC()
                {
                    CreatedOn = Convert.ToDateTime(createdDate),
                    AuditLogs = auditLogACs.Where(s => s.CreatedOn.ToShortDateString().Equals(createdDate)).ToList()
                });
            }
            #endregion
            return auditDateWiseLogs;
        }
        /// <summary>
        /// Get audit log fields from the json.
        /// </summary>
        /// <param name="auditLog">Audit log object.</param>
        /// <param name="id">Audit log id.</param>
        /// <returns></returns>
        private async Task GetAuditLogFieldsByActionAsync(AuditLogAC auditLog, Guid? id = null)
        {
            var eventEntry = JsonConvert.DeserializeObject<EventEntry>(auditLog.AuditJson);

            #region Update
            if (auditLog.Action.Equals(StringConstant.AuditLogUpdateActionName))
            {
                auditLog.AuditLogFields = _mapper.Map<List<AuditLogFieldAC>>(eventEntry.Changes);
                auditLog.AuditLogFields = UpdateEnumValuesFromAuditLogFields(auditLog.AuditLogFields, eventEntry.Table);
                auditLog.AuditLogFields = await UpdateFieldValuesAsync(auditLog.AuditLogFields, eventEntry.Table);
                auditLog.AuditLogFields = PrepareFieldsByUx(auditLog.AuditLogFields, eventEntry.Table, id);
                if (auditLog.TableName != StringConstant.DocumentExtractedValueSnapshot)
                {
                    // Remove duplicate fields and null fields which hasn't add earlier and now.
                    auditLog.AuditLogFields = auditLog.AuditLogFields.Where(s => (s.NewValue != null && !s.NewValue.Equals(s.OriginalValue)) || (s.NewValue is null && s.OriginalValue != null)).ToList();
                }
            }
            #endregion

            #region Insert
            if (auditLog.Action.Equals(StringConstant.AuditLogInsertActionName))
            {
                auditLog.AuditLogFields = await PrepareFieldsFromColumnValuesAsync(eventEntry);
            }
            #endregion

            #region Delete
            if (auditLog.Action.Equals(StringConstant.AuditLogDeleteActionName))
            {
                auditLog.AuditLogFields = await PrepareFieldsFromColumnValuesAsync(eventEntry);
            }
            #endregion
        }
        /// <summary>
        /// Merge list of consent logs by user.
        /// </summary>
        /// <param name="auditLogACs">List of audit logs.</param>
        /// <returns>List of audit logs with merge consent logs by user.</returns>
        private List<AuditLogAC> MergeConsentDetailsByUser(List<AuditLogAC> auditLogACs)
        {
            var consentLogs = auditLogACs.Where(s => s.TableName == nameof(EntityLoanApplicationConsent)).ToList();
            if (consentLogs.Any())
            {
                var userIds = consentLogs.Select(s => s.CreatedByUserId).Distinct();
                foreach (var userId in userIds)
                {
                    var userConsentLogs = consentLogs.Where(s => s.CreatedByUserId.Equals(userId)).OrderByDescending(s => s.CreatedOn);
                    var userLastConsentLog = userConsentLogs.First();
                    var consentLogFields = userConsentLogs.SelectMany(s => s.AuditLogFields).ToList();
                    var consentIdFields = RemoveAuditLogUnwantedFields(consentLogFields, nameof(EntityLoanApplicationConsent));
                    var firstConsentField = consentIdFields.First();
                    firstConsentField.Ids = consentIdFields.Select(s => s.NewValue).ToList();
                    // Update userLastConsentLog with list of consentIds. 
                    auditLogACs.Where(s => s.Id.Equals(userLastConsentLog.Id)).ToList().ForEach(s => s.AuditLogFields = new List<AuditLogFieldAC> { firstConsentField });
                    // Remove all user consents except userLastConsentLog.  
                    auditLogACs.RemoveAll(s => s.TableName == nameof(EntityLoanApplicationConsent) && s.CreatedByUserId.Equals(userId) && !s.Id.Equals(userLastConsentLog.Id));
                }
            }
            return auditLogACs;

        }
        /// <summary>
        /// Prepare the list of AuditLogFieldAC from the column values in the EventEntry object.
        /// </summary>
        /// <param name="newEventEntry">new value </param>
        /// <param name="oldEventEntry"></param>
        /// <param name="logDate">Log date.</param>
        /// <returns>List of AuditLogFieldAC.</returns>
        private async Task<List<AuditLogFieldAC>> PrepareFieldsFromColumnValuesAsync(EventEntry newEventEntry, EventEntry oldEventEntry = null, DateTime? logDate = null)
        {
            Guid id;
            List<AuditLogFieldAC> auditLogFields = new List<AuditLogFieldAC>();
            if (newEventEntry != null)
            {
                foreach (var columnValue in newEventEntry.ColumnValues)
                {
                    auditLogFields.Add(new AuditLogFieldAC()
                    {
                        ColumnName = columnValue.Key,
                        NewValue = columnValue.Value,
                        OriginalValue = oldEventEntry?.ColumnValues.First(s => s.Key == columnValue.Key).Value,
                        IsGuid = Guid.TryParse(columnValue.Value?.ToString(), out id)
                    });
                }
                auditLogFields = UpdateEnumValuesFromAuditLogFields(auditLogFields, newEventEntry.Table);
                auditLogFields = await UpdateFieldValuesAsync(auditLogFields, newEventEntry.Table, logDate);
                auditLogFields = PrepareFieldsByUx(auditLogFields, newEventEntry.Table);
                auditLogFields = auditLogFields.Where(s => s.NewValue != null || s.OriginalValue != null).ToList();
            }
            return auditLogFields;
        }

        /// <summary>
        /// Update field enum number to string.
        /// </summary>
        /// <param name="auditLogFields">List of AuditLogFieldAC</param>
        /// <param name="tableName">Table name.</param>
        /// <returns>List of AuditLogFieldAC with updated enum string</returns>
        private List<AuditLogFieldAC> UpdateEnumValuesFromAuditLogFields(List<AuditLogFieldAC> auditLogFields, string tableName)
        {
            if (tableName.Equals(nameof(LoanApplication)))
            {
                auditLogFields.Where(s => s.ColumnName.Equals(nameof(LoanApplication.Status))).ToList()
                              .ForEach(s =>
                              {
                                  s.OriginalValue = s.OriginalValue != null ? ((LoanApplicationStatusType)Convert.ToInt16(s.OriginalValue)).ToString() : null;
                                  s.NewValue = s.NewValue != null ? ((LoanApplicationStatusType)Convert.ToInt16(s.NewValue)).ToString() : null;
                              });
            }
            else if (tableName.Equals(nameof(User)))
            {
                auditLogFields.Where(s => s.ColumnName.Equals(nameof(User.ResidencyStatus))).ToList()
                              .ForEach(s =>
                              {
                                  s.OriginalValue = s.OriginalValue != null ? ((ResidencyStatus)Convert.ToInt16(s.OriginalValue)).ToString() : null;
                                  s.NewValue = s.NewValue != null ? ((ResidencyStatus)Convert.ToInt16(s.NewValue)).ToString() : null;
                              });
            }
            return auditLogFields;
        }
        /// <summary>
        /// Fetch the list of fields of the primary key which is in the old and new value.
        /// </summary>
        /// <param name="auditLogField">Field object.</param>
        /// <returns>Retrieve the list of fields of the log.</returns>
        public async Task<List<AuditLogFieldAC>> GetAuditLogByPkIdAsync(AuditLogFieldAC auditLogField)
        {
            // Thrown exception when id is not exist.
            if (auditLogField.NewValue == null && auditLogField.OriginalValue == null)
            {
                throw new InvalidParameterException();
            }

            Guid originalValue, newValue;
            List<Guid> idList = new List<Guid>();
            if (auditLogField.Ids != null)
            {
                foreach (var id in auditLogField.Ids)
                {
                    Guid.TryParse(Convert.ToString(id), out newValue);
                    idList.Add(newValue);
                }
            }
            Guid.TryParse(Convert.ToString(auditLogField.OriginalValue), out originalValue);
            Guid.TryParse(Convert.ToString(auditLogField.NewValue), out newValue);
            string entityTableName = nameof(Entity);
            // Add this second because log date hasn't milisecond. 
            double addSecond = 2;

            var auditLogs = await _dataRepository.Fetch<AuditLog>(x => x.Id.Equals(newValue) || (x.TableName != entityTableName && x.CreatedOn <= auditLogField.LogDate.AddSeconds(addSecond) &&
                                                    ((auditLogField.OriginalValue != null && x.TablePk.Equals(originalValue))
                                                    || (auditLogField.NewValue != null && x.TablePk.Equals(newValue))
                                                    || (idList.Any() && idList.Contains(x.TablePk)))))
                                                 .OrderByDescending(s => s.CreatedOn).ToListAsync();

            // Thrown exception when data is not found
            if (!auditLogs.Any())
            {
                throw new DataNotFoundException();
            }

            List<AuditLogFieldAC> auditLogFields = new List<AuditLogFieldAC>();
            if (auditLogs.Any() && auditLogs.First().Id.Equals(newValue))
            {
                var auditLog = _mapper.Map<AuditLogAC>(auditLogs.First());
                await GetAuditLogFieldsByActionAsync(auditLog);
                auditLogFields = auditLog.AuditLogFields;
            }
            else
            {
                if (!idList.Any())
                {
                    auditLogField.Ids = new List<object> { auditLogField.NewValue };
                }
                foreach (var pkId in auditLogField.Ids)
                {
                    auditLogField.NewValue = pkId;
                    Guid.TryParse(Convert.ToString(auditLogField.NewValue), out newValue);

                    AuditLog oldValueAuditLog = auditLogs.FirstOrDefault(x => x.TablePk.Equals(originalValue));
                    AuditLog newValueAuditLog = auditLogs.FirstOrDefault(x => x.TablePk.Equals(newValue));
                    EventEntry oldValueEventEntry = oldValueAuditLog != null ? JsonConvert.DeserializeObject<EventEntry>(oldValueAuditLog.AuditJson) : null;
                    EventEntry newValueEventEntry = newValueAuditLog != null ? JsonConvert.DeserializeObject<EventEntry>(newValueAuditLog.AuditJson) : null;

                    var prepareFields = await PrepareFieldsFromColumnValuesAsync(newValueEventEntry, oldValueEventEntry, auditLogField.LogDate);
                    auditLogFields.AddRange(prepareFields);
                }
            }

            auditLogFields = RemoveAuditLogUnwantedFields(auditLogFields, auditLogs[0].TableName);

            auditLogFields = await UpdateOrRemoveAuditFieldsForUxAsync(auditLogFields, auditLogField);

            return auditLogFields;
        }

        /// <summary>
        ///  Update or remove audit field according to UX.
        /// </summary>
        /// <param name="auditLogFields"></param>
        /// <param name="auditLogField"></param>
        /// <returns></returns>
        private async Task<List<AuditLogFieldAC>> UpdateOrRemoveAuditFieldsForUxAsync(List<AuditLogFieldAC> auditLogFields, AuditLogFieldAC auditLogField)
        {
            if (auditLogFields.SingleOrDefault(x => x.ColumnName == StringConstant.AttributeId) != null)
            {
                auditLogFields[0].NewValue = (await _dataRepository.Fetch<PersonalFinanceAttributeCategoryMapping>(x => x.Id == Guid.Parse(auditLogField.NewValue.ToString())).Include(x => x.Attribute).SingleAsync())?.Attribute.Text;
            }
            auditLogFields.RemoveAll(x => x.ColumnName == StringConstant.LoanPurposeId || x.ColumnName == StringConstant.SubLoanPurposeId);
            return auditLogFields;
        }

        /// <summary>
        /// Prepare fields value as per the UX.
        /// </summary>
        /// <param name="auditLogFields">List of fields.</param>
        /// <param name="tableName">Table name.</param>
        /// <param name="id">Audit log id.</param>
        /// <returns>List of AuditLogFieldAC.</returns>
        private List<AuditLogFieldAC> PrepareFieldsByUx(List<AuditLogFieldAC> auditLogFields, string tableName, Guid? id = null)
        {
            if (tableName.Equals(nameof(Address)))
            {
                var primaryNumber = auditLogFields.First(s => s.ColumnName.Equals(nameof(Address.PrimaryNumber)));
                var streetSuffix = auditLogFields.First(s => s.ColumnName.Equals(nameof(Address.StreetSuffix)));
                var streetLine = auditLogFields.First(s => s.ColumnName.Equals(nameof(Address.StreetLine)));
                if (streetLine.OriginalValue != null)
                {
                    streetLine.OriginalValue = $"{primaryNumber.OriginalValue} {streetLine.OriginalValue} {streetSuffix.OriginalValue}";
                }
                if (streetLine.NewValue != null)
                {
                    streetLine.NewValue = $"{primaryNumber.NewValue} {streetLine.NewValue} {streetSuffix.NewValue}";
                }

                // If streetline update then display show more option in the frontend else display address separetly.
                if (id.HasValue && !streetLine.OriginalValue.Equals(streetLine.NewValue))
                {
                    AuditLogFieldAC auditLogField = new AuditLogFieldAC()
                    {
                        ColumnName = nameof(Entity.AddressId),
                        IsGuid = true,
                        NewValue = id,
                        // Add Guid.Empty to show a updated values in the frontend.
                        // In frontend "Updated" display when field has original and new value.
                        OriginalValue = Guid.Empty
                    };
                    auditLogFields = new List<AuditLogFieldAC>() { auditLogField };
                }

            }
            else if (tableName.Equals(nameof(EntityLoanApplicationMapping)))
            {
                var updateEntityId = auditLogFields.First(s => s.ColumnName.Equals(nameof(EntityLoanApplicationMapping.EntityId)));
                updateEntityId.ColumnName = $"{nameof(EntityLoanApplicationMapping)}_{nameof(EntityLoanApplicationMapping.EntityId)}";
            }
            return auditLogFields;
        }
        /// <summary>
        /// Update field values.
        /// </summary>
        /// <param name="auditLogFields">List of fields</param>
        /// <param name="tableName">Table name.</param>
        /// <param name="logDate">Log date.</param>
        /// <returns></returns>
        private async Task<List<AuditLogFieldAC>> UpdateFieldValuesAsync(List<AuditLogFieldAC> auditLogFields, string tableName, DateTime? logDate = null)
        {
            #region Display only date instead of display datetime in the DOB field.
            if (tableName.Equals(nameof(User)))
            {
                auditLogFields.Where(s => s.ColumnName.Equals(nameof(User.DOB))).ToList().ForEach(
                    s =>
                    {
                        s.OriginalValue = s.OriginalValue != null ? ((DateTime)s.OriginalValue).ToString(StringConstant.dashedDateFormat) : s.OriginalValue;
                        s.NewValue = s.NewValue != null ? ((DateTime)s.NewValue).ToString(StringConstant.dashedDateFormat) : s.NewValue;
                    });
            }
            #endregion

            #region Remove +1 prefix in the phone number.
            if (tableName.Equals(nameof(User)) || tableName.Equals(nameof(BankUser)))
            {
                auditLogFields.Where(s => s.ColumnName.Equals(nameof(User.Phone))).ToList().ForEach(
                    s =>
                    {
                        s.OriginalValue = s.OriginalValue != null && s.OriginalValue.ToString().Contains(StringConstant.USCountryCode) ? s.OriginalValue.ToString().Replace(StringConstant.USCountryCode, string.Empty) : s.OriginalValue;
                        s.NewValue = s.NewValue != null && s.NewValue.ToString().Contains(StringConstant.USCountryCode) ? s.NewValue.ToString().Replace(StringConstant.USCountryCode, string.Empty) : s.NewValue;
                    });
            }
            #endregion

            #region Company structure name.
            if (logDate != null && tableName.Equals(nameof(Company)))
            {
                var companyStructureField = auditLogFields.First(s => s.ColumnName.Equals(nameof(Company.CompanyStructureId)));
                companyStructureField.LogDate = logDate.Value;
                var fieldResults = await GetAuditLogByPkIdAsync(companyStructureField);
                var companyStructureName = fieldResults.FirstOrDefault(s => s.ColumnName.Equals(nameof(CompanyStructure.Structure)));
                if (companyStructureName != null)
                {
                    companyStructureField.OriginalValue = companyStructureName.OriginalValue;
                    companyStructureField.NewValue = companyStructureName.NewValue;
                }
            }
            #endregion
            return auditLogFields;
        }
        /// <summary>
        /// It removes unnecessary fields that is mention in the constant(AuditLogNecessaryTableFields) else it returns all fields.
        /// </summary>
        /// <param name="auditLogFields">List of fields</param>
        /// <param name="tableName">Table name.</param>
        /// <returns></returns>
        private List<AuditLogFieldAC> RemoveAuditLogUnwantedFields(List<AuditLogFieldAC> auditLogFields, string tableName)
        {
            var necessaryFields = StringConstant.AuditLogNecessaryTableFields.FirstOrDefault(a => a.Key.Equals(tableName));
            if (necessaryFields.Key != null)
            {
                auditLogFields = auditLogFields.Where(s => necessaryFields.Value.Contains(s.ColumnName)).ToList();
            }
            return auditLogFields;
        }
        /// <summary>
        /// Method return string name of month with respective to provide number
        /// </summary>
        /// <param name="monthNumber">Number month</param>
        /// <returns></returns>
        public string GetMonthNameFromMonthNumber(int monthNumber)
        {
            return monthNumber switch
            {
                1 => StringConstant.January,
                2 => StringConstant.February,
                3 => StringConstant.March,
                4 => StringConstant.April,
                5 => StringConstant.May,
                6 => StringConstant.June,
                7 => StringConstant.July,
                8 => StringConstant.August,
                9 => StringConstant.September,
                10 => StringConstant.October,
                11 => StringConstant.November,
                12 => StringConstant.December,
                0 => StringConstant.December,
                13 => StringConstant.January,
                _ => StringConstant.InvalidMonthNumber,
            };
        }

        /// <summary>
        /// Get upload pre signed URL
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        public AwsSettings GetUploadPreSignedURL(string fileName)
        {
            AwsSettings awsSettings = new AwsSettings();
            awsSettings.BaseUrl = String.Format("{0}{1}/{2}", StringConstant.FileTemp, Guid.NewGuid(), fileName);
            awsSettings.UploadPreSignedUrl = _amazonS3Utility.GetUploadPreSignedURL(awsSettings.BaseUrl);
            awsSettings.PreSignedUrl = _amazonS3Utility.GetPreSignedURL(awsSettings.BaseUrl);
            return awsSettings;
        }

        /// <summary>
        /// Method fetches all the additional document types.
        /// </summary>
        /// <returns>List of AdditionalDocumentTypeAC</returns>
        public async Task<List<AdditionalDocumentTypeAC>> GetAdditionalDocumentTypesAsync()
        {
            List<AdditionalDocumentType> additionalDocumentTypes = await _dataRepository.Fetch<AdditionalDocumentType>(x => x.IsEnabled).ToListAsync();

            if (!additionalDocumentTypes.Any())
            {
                throw new DataNotFoundException(StringConstant.AdditionalDocumentTypesNotFound);
            }
            return _mapper.Map<List<AdditionalDocumentTypeAC>>(additionalDocumentTypes);
        }

        #endregion
    }
}
