using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.Product;
using LendingPlatform.Utils.ApplicationClass.TaxForm;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using LendingPlatform.Utils.Utils.OCR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.Application
{
    public class ApplicationRepository : IApplicationRepository
    {
        #region Private variables
        private readonly IDataRepository _dataRepository;
        private readonly IGlobalRepository _globalRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ISimpleEmailServiceUtility _simpleEmailServiceUtility;
        private readonly IRulesUtility _rulesUtility;
        private readonly IAmazonServicesUtility _amazonServicesUtility;
        private readonly IOCRUtility _ocrUtility;
        #endregion

        #region Constructor
        public ApplicationRepository(IDataRepository dataRepository, IGlobalRepository globalRepository,
            IConfiguration configuration, IMapper mapper,
            ISimpleEmailServiceUtility simpleEmailServiceUtility, IRulesUtility rulesUtility,
            IOCRUtility ocrUtility, IAmazonServicesUtility amazonServicesUtility)
        {
            _dataRepository = dataRepository;
            _globalRepository = globalRepository;
            _configuration = configuration;
            _mapper = mapper;
            _simpleEmailServiceUtility = simpleEmailServiceUtility;
            _rulesUtility = rulesUtility;
            _ocrUtility = ocrUtility;
            _amazonServicesUtility = amazonServicesUtility;
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Method generates loan application number for each new loan application. 
        /// </summary>
        /// <returns>Returns unique loan application number of type string</returns>
        private string GenerateLoanApplicationNumber()
        {
            DateTime dateTime = DateTime.UtcNow;
            string bankCode = _configuration.GetValue<string>("Codes:BankCode");

            if (bankCode != null)
            {
                return bankCode + dateTime.ToString("dd") + dateTime.ToString("MM") + dateTime.ToString("yyyy")
                        + dateTime.ToString("HH") + dateTime.ToString("mm") + dateTime.ToString("ss") + dateTime.ToString("ff");
            }
            else
            {
                throw new ConfigurationNotFoundException(StringConstant.BankCodeNotExistsInConfiguration);
            }
        }

        /// <summary>
        /// Method to save the consent and credit report of given user for given loan application.
        /// </summary>
        /// <param name="applicationId">Loan application id</param>
        /// <param name="currentUser">User object.</param>
        /// <returns></returns>
        private async Task SaveConsentsOfGivenUserAsync(Guid applicationId, CurrentUserAC currentUser)
        {
            //Get all the consent text ids to link with the current user.
            var consents = await _dataRepository.GetAll<Consent>().ToListAsync();
            if (!consents.Any(x => x.IsEnabled))
            {
                throw new DataNotFoundException(StringConstant.EmptyConsentTable);
            }

            //Make the list of all consent and user id to add in application consent mapping table.
            var entityApplicationConsents = new List<EntityLoanApplicationConsent>();
            foreach (var id in consents.Where(x => x.IsEnabled).Select(x => x.Id).ToList())
            {
                entityApplicationConsents.Add(new EntityLoanApplicationConsent
                {
                    ConsenteeId = currentUser.Id,
                    LoanApplicationId = applicationId,
                    ConsentId = id,
                    IsConsentGiven = true,
                    CreatedByUserId = currentUser.Id,
                    CreatedOn = DateTime.UtcNow
                });
            }

            //Add the user consents in EntityLoanApplicationConsent table.
            await _dataRepository.AddRangeAsync<EntityLoanApplicationConsent>(entityApplicationConsents);
            // Prepare the Auditlog object to save the custom fields in the dbcontext.
            AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, applicationId);
            await _dataRepository.SaveChangesAsync(auditLog);
        }

        /// <summary>
        /// Method to filter the given loan list by given filter values.
        /// </summary>
        /// <param name="applications">List of applications</param>
        /// <param name="mappings">List of entity mappings for applications linked with entity</param>
        /// <param name="filters">Filtering parameters</param>
        /// <returns>Filtered list of applications</returns>
        private List<LoanApplication> FilterLoanList(List<LoanApplication> applications, List<EntityLoanApplicationMapping> mappings, List<FilterAC> filters)
        {
            foreach (var filter in filters)
            {
                if (filter.Field.Trim().ToLowerInvariant().Equals(StringConstant.FilterApplicationNumber) && filter.Operator.Trim().ToLowerInvariant().Equals(StringConstant.FilterEqualOperator))
                {
                    applications = applications.Where(x => x.LoanApplicationNumber.ToLowerInvariant().Contains(filter.Value.ToLowerInvariant())).ToList();
                }
                else if (filter.Field.Trim().ToLowerInvariant().Equals(StringConstant.FilterCreatedDate) && filter.Operator.Trim().ToLowerInvariant().Equals(StringConstant.FilterEqualOperator))
                {
                    applications = applications.Where(x => filter.Value.Contains('T') ? filter.Value.Split('T')[0].ToLowerInvariant().Equals(x.CreatedOn.ToString(StringConstant.dashedDateFormat))
                    : filter.Value.Split(' ')[0].ToLowerInvariant().Equals(x.CreatedOn.ToString(StringConstant.dashedDateFormat))).ToList();
                }
                else if (filter.Field.Trim().ToLowerInvariant().Equals(StringConstant.FilterCompanyName) && filter.Operator.Trim().ToLowerInvariant().Equals(StringConstant.FilterEqualOperator))
                {
                    applications = applications.Where(x => mappings.Where(y => y.Entity.Company.Name.ToLowerInvariant().Contains(filter.Value.ToLowerInvariant())).Select(z => z.LoanApplicationId).ToList().Contains(x.Id)).ToList();
                }
                else if (filter.Field.Trim().ToLowerInvariant().Equals(StringConstant.FilterLoanAmount) && filter.Operator.Trim().ToLowerInvariant().Equals(StringConstant.FilterEqualOperator))
                {
                    applications = applications.Where(x => x.LoanAmount.ToString().Contains(filter.Value.ToLowerInvariant())).ToList();
                }
                else if (filter.Field.Trim().ToLowerInvariant().Equals(StringConstant.FilterLoanPurpose) && filter.Operator.Trim().ToLowerInvariant().Equals(StringConstant.FilterEqualOperator))
                {
                    applications = applications.Where(x => x.LoanPurpose.Name.ToLowerInvariant().Contains(filter.Value.ToLowerInvariant())).ToList();
                }
                else if (filter.Field.Trim().ToLowerInvariant().Equals(StringConstant.FilterLoanStatus) && filter.Operator.Trim().ToLowerInvariant().Equals(StringConstant.FilterEqualOperator))
                {
                    var temp = Enum.GetValues(typeof(LoanApplicationStatusType));
                    foreach (var item in temp)
                    {
                        if (item.ToString().ToLowerInvariant().Contains(filter.Value.ToLowerInvariant()))
                        {
                            applications = applications.Where(x => x.Status.Equals((LoanApplicationStatusType)Enum.Parse(typeof(LoanApplicationStatusType), item.ToString()))).ToList();
                            break;
                        }
                    }
                }
            }
            return applications;
        }

        /// <summary>
        /// Method to paginate the applications by given record count and page number.
        /// </summary>
        /// <param name="applications">List of applications</param>
        /// <param name="pageNo">Required page number</param>
        /// <param name="recordPerPageCount">Record count per page</param>
        /// <returns>List of paginated applications</returns>
        private List<LoanApplication> PaginateLoanList(List<LoanApplication> applications, int pageNo, int recordPerPageCount)
        {
            int recordToSkip = (Math.Abs(pageNo) - 1) * Math.Abs(recordPerPageCount);
            return applications.Skip(recordToSkip).Take(Math.Abs(recordPerPageCount)).ToList();
        }

        /// <summary>
        /// Method to sort the list of application by given field and in given order.
        /// </summary>
        /// <param name="applications">List of applications</param>
        /// <param name="sortField">Name of field which is to be used in sorting</param>
        /// <param name="sortBy">Sorting order</param>
        /// <returns>Sorted list of applications</returns>
        private List<LoanApplication> SortLoanList(List<LoanApplication> applications, string sortField, string sortBy)
        {
            if (sortField.Equals(StringConstant.FilterCreatedDate))
            {
                return sortBy.Equals(StringConstant.AscendingOrderShorthand.ToLowerInvariant()) ?
                    applications.OrderBy(x => x.CreatedOn).ToList() : applications.OrderByDescending(x => x.CreatedOn).ToList();
            }
            return applications;
        }

        /// <summary>
        /// Method returns the selected product for the loan application
        /// </summary>
        /// <param name="loanApplication">Loan application object</param>
        /// <returns>Recommended Product object</returns>
        private async Task<RecommendedProductAC> GetRecommendedProductAsync(LoanApplication loanApplication)
        {
            var currentCurrencySymbol = _configuration.GetSection("Currency:Symbol").Value;

            var productRangeTypeMappingList = await _dataRepository.GetAll<ProductRangeTypeMapping>()
                .Include(x => x.LoanRangeType).ToListAsync();
            var productPurposeMappingList = await _dataRepository.GetAll<ProductSubPurposeMapping>()
                                                  .Include(x => x.SubLoanPurpose).ThenInclude(x => x.LoanPurpose).ToListAsync();

            var productRule = _mapper.Map<LoanApplication, ProductRuleAC>(loanApplication);
            productRule.LoanPeriod = productRule.LoanPeriod * 12;
            if (productPurposeMappingList.Any())
            {
                productRule.ProductSubPurposeMappings = _mapper.Map<List<ProductSubPurposeMapping>, List<ProductSubPurposeMappingAC>>(productPurposeMappingList);
            }

            if (productRangeTypeMappingList.Any())
            {
                productRule.ProductRangeTypeMappings = _mapper.Map<List<ProductRangeTypeMapping>, List<ProductRangeTypeMappingAC>>(productRangeTypeMappingList);
            }

            var productsPercentageSuitability = await _rulesUtility.ExecuteProductRules(productRule);

            var loanPurposeRangeTypeMappings = await _dataRepository.Fetch<LoanPurposeRangeTypeMapping>(x => x.LoanPurposeId == loanApplication.LoanPurposeId).Include(x => x.LoanRangeType).ToListAsync();

            var selectedProduct = _mapper.Map<RecommendedProductAC>(loanApplication.Product);
            selectedProduct.IsProductRecommended = true;

            if (productsPercentageSuitability.Any() && !productsPercentageSuitability.Any(x => x.ProductId == loanApplication.ProductId))
            {
                selectedProduct.IsPreviousProductMatched = false;
            }
            else
            {
                selectedProduct.IsPreviousProductMatched = true;
            }

            selectedProduct.ProductDetails = _globalRepository.CalculateLoanProductDetail(productRangeTypeMappingList.Where(x => x.ProductId == loanApplication.ProductId).ToList(), currentCurrencySymbol, loanApplication);
            selectedProduct.ProductDetails.TenureStepperCount = loanPurposeRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.Lifecycle).StepperAmount;
            selectedProduct.ProductDetails.AmountStepperCount = loanPurposeRangeTypeMappings.SingleOrDefault(x => x.LoanRangeType.Name == StringConstant.LoanAmount).StepperAmount;
            return selectedProduct;
        }

        #region Tax form value extraction
        /// <summary>
        /// Method is to add the extracted value of the tax return file in DB 
        /// </summary>
        /// <param name="loanId">Current Loan Id</param>
        /// <returns></returns>
        private async Task AddTaxReturnExtractedValueFromOCRAsync(Guid loanId)
        {
            var taxFormValueLabelMappings = new List<TaxFormValueLabelMapping>();
            var entityLoanMappings = await _dataRepository.Fetch<LoanApplication>(x => x.Id == loanId)
                .Include(x => x.EntityLoanApplicationMappings).ThenInclude(x => x.Entity).ThenInclude(x => x.EntityTaxForms)
                .Include(x => x.EntityLoanApplicationMappings).ThenInclude(x => x.Entity).ThenInclude(x => x.Company)
                .ToListAsync();
            if (entityLoanMappings.Select(y => y.EntityLoanApplicationMappings.Select(x => x.Entity.Company).SingleOrDefault()).SingleOrDefault() == null)
            {
                throw new InvalidParameterException(StringConstant.CompanyNotExist);
            }

            // Keep this comment. This code will be needed when different form will come as per the company structure
            // Get current company structure OCR Model data
            /*var currentCompanyStructureId = entityLoanMappings.Select(x => x.EntityLoanApplicationMappings.Select(y => y.Entity.Company.CompanyStructureId).Single()).Single();
            var currentCompanyStructureOCRModel = await _dataRepository.SingleOrDefaultAsync<OCRModelMapping>(x => x.CompanyStructureId == currentCompanyStructureId);*/

            // Get model detail as per the enter period for uploaded tax forms
            var entityTaxForms = entityLoanMappings.SelectMany(x => x.EntityLoanApplicationMappings.SelectMany(y => y.Entity.EntityTaxForms).ToList()).Where(x => x.LoanApplicationId.Equals(loanId)).OrderByDescending(o => o.SurrogateId).Take(1).ToList();
            var entityTaxYearlyMappings = await _dataRepository.Fetch<EntityTaxYearlyMapping>(x => entityTaxForms.Select(y => y.Id).Contains(x.EntityTaxFormId)).Include(x => x.UploadedDocument).Include(x => x.TaxFormValueLabelMappings).ToListAsync();
            var ocrModels = await _dataRepository.Fetch<OCRModelMapping>(x => entityTaxYearlyMappings.Select(y => y.Period).Contains(x.Year)).ToListAsync();

            // Keep this comment. This code will be needed when different form will come as per the company structure
            //var currentTaxformCompanyStructureMappings = await _dataRepository.Fetch<TaxFormCompanyStructureMapping>(x => x.CompanyStructureId == currentCompanyStructureId).Include(x => x.TaxForm).ToListAsync();

            var allTaxFormsExtractedValues = new List<TaxFormValueLabelMappingAC>();
            foreach (var entityTaxYearlyMapping in entityTaxYearlyMappings)
            {
                if (!entityTaxYearlyMapping.TaxFormValueLabelMappings.Any())
                {
                    var tempPreSignedUrl = _amazonServicesUtility.GetPreSignedURL(entityTaxYearlyMapping.UploadedDocument.Path);
                    // Keep this comment. This code will be needed when different form will come as per the company structure
                    /*var temp = await _ocrUtility.RecognizeContentModelAsync(currentCompanyStructureOCRModel.ModelId, tempPreSignedUrl);
                    if(!temp.All(x => currentTaxformCompanyStructureMappings.Select(y => y.TaxForm.Name.ToUpper()).Any(z => z.Contains(x.Value.ToUpper()) || x.Value.ToUpper().Contains(z))))
                    {
                        break;
                    }*/
                    if (ocrModels.Any(x => x.Year == entityTaxYearlyMapping.Period))
                    {
                        allTaxFormsExtractedValues.AddRange(_mapper.Map<List<OCRExtractedValueAC>, List<TaxFormValueLabelMappingAC>>(await ExtractValueFromTaxReturnAsync(ocrModels.Single(x => x.Year == entityTaxYearlyMapping.Period).ModelId, tempPreSignedUrl)));
                        allTaxFormsExtractedValues.ForEach(x => x.EntityTaxYearlyMappingId = entityTaxYearlyMapping.Id);
                    }
                }
                else
                {
                    taxFormValueLabelMappings.AddRange(entityTaxYearlyMapping.TaxFormValueLabelMappings);
                }
            }

            if (allTaxFormsExtractedValues.Any())
            {
                var allTaxFormLabelNameMappings = await _dataRepository.Fetch<TaxFormLabelNameMapping>(x => allTaxFormsExtractedValues.Select(y => y.Label).ToList().Contains(x.LabelFieldName)).ToListAsync();

                foreach (var taxFormExtractedValue in allTaxFormsExtractedValues)
                {
                    var taxFormValueLabelMapping = _mapper.Map<TaxFormValueLabelMapping>(taxFormExtractedValue);
                    taxFormValueLabelMapping.TaxformLabelNameMappingId = allTaxFormLabelNameMappings.SingleOrDefault(x => x.LabelFieldName == taxFormExtractedValue.Label).Id;
                    taxFormValueLabelMapping.TaxformLabelNameMapping = allTaxFormLabelNameMappings.SingleOrDefault(x => x.LabelFieldName == taxFormExtractedValue.Label);
                    taxFormValueLabelMappings.Add(taxFormValueLabelMapping);
                }

                using (await _dataRepository.BeginTransactionAsync())
                {
                    await _dataRepository.AddRangeAsync<TaxFormValueLabelMapping>(taxFormValueLabelMappings);
                    await _dataRepository.SaveChangesAsync();
                    _dataRepository.CommitTransaction();
                }
            }
        }

        /// <summary>
        /// Extract value of tax form
        /// </summary>
        /// <param name="modelId">Model Id</param>
        /// <param name="preSignedUrl">Presigned URL of document</param>
        /// <returns></returns>
        private async Task<List<OCRExtractedValueAC>> ExtractValueFromTaxReturnAsync(string modelId, string preSignedUrl)
        {
            var ocrExtractedValues = await _ocrUtility.RecognizeContentModelAsync(modelId, preSignedUrl);
            return ocrExtractedValues;
        }

        #endregion

        /// <summary>
        /// Method to check if all the required sections are filled up before giving consent
        /// </summary>
        /// <param name="entityMapping">EntityLoanApplicationMapping object</param>
        /// <returns></returns>
        private void ValidateAllSectionCompletion(EntityLoanApplicationMapping entityMapping)
        {
            string unfilledSections = null;

            if (entityMapping.Entity.EntityFinances == null || !entityMapping.Entity.EntityFinances.Any())
            {
                unfilledSections = string.Concat(unfilledSections, StringConstant.FinancesSection, ",");
            }
            else if (entityMapping.Entity.EntityFinances.Where(x => x.FinancialStatement.Name.Equals(StringConstant.IncomeStatement)).Any(x => (x.EntityFinanceYearlyMappings == null || !x.EntityFinanceYearlyMappings.Any())))
            {
                unfilledSections = string.Concat(unfilledSections, StringConstant.CompanyFinance, ",");
            }

            if (entityMapping.Entity.EntityTaxForms == null || !entityMapping.Entity.EntityTaxForms.Any())
            {
                unfilledSections = string.Concat(unfilledSections, StringConstant.TaxReturns, ",");
            }

            if (entityMapping.LoanApplication.ProductId == null)
            {
                unfilledSections = string.Concat(unfilledSections, StringConstant.LoanProductSection, ",");
            }

            if (!string.IsNullOrEmpty(unfilledSections))
            {
                //Remove last appended comma from the whole string.
                unfilledSections = unfilledSections.Remove(unfilledSections.Length - 1, 1);
                throw new ValidationException(string.Concat(StringConstant.PleaseFillTheseSectionsToGiveConsent, unfilledSections));
            }
        }

        #region Personal Finance
        /// <summary>
        /// Method to add summary of personal finances as Json in entity finance table
        /// </summary>
        /// <param name="entities">List of entities (shareholders)</param>
        /// <returns></returns>
        private async Task SavePersonalFinanceSummaryJsonAsync(List<EntityAC> entities)
        {
            //Fetch latest version of entityfinances for given loan
            var entityFinances = await _dataRepository.Fetch<EntityFinance>(x => entities.Select(y => y.Id).Contains(x.EntityId) && x.LoanApplicationId == null).ToListAsync();

            //Map financial info Json of each entity
            entityFinances.ForEach(x => x.FinancialInformationJson = JsonConvert.SerializeObject(entities.Single(y => y.Id.Equals(x.EntityId)).PersonalFinance));

            _dataRepository.UpdateRange<EntityFinance>(entityFinances);
            await _dataRepository.SaveChangesAsync();
        }
        #endregion

        #endregion

        #region Public methods

        #region Loan needs
        /// <summary>
        /// Method fetches all loan applications.
        /// </summary>
        /// <param name="filterModel"></param>
        /// <param name="currentUser">Logged in user</param>
        /// <returns>PagedLoanApplicationsAC object</returns>
        public async Task<PagedLoanApplicationsAC> GetAllLoanApplicationsAsync(FilterModelAC filterModel, CurrentUserAC currentUser)
        {
            //Only bank user is allowed to get all the loans.
            if (!currentUser.IsBankUser)
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            PagedLoanApplicationsAC pagedLoanApplications = new PagedLoanApplicationsAC();
            var authorizedEmailList = _configuration.GetSection("BankPreference:DbResetAuthorizedEmails").GetChildren().Select(x => x.Value).ToList();
            if (authorizedEmailList.Contains(currentUser.Email))
            {
                pagedLoanApplications.IsDeleteAuthorized = true;
            }
            //Fetch all the loan applications
            List<LoanApplication> applications = await _dataRepository.GetAll<LoanApplication>().Include(i => i.UserLoanSectionMappings).ThenInclude(s => s.Section)
                .Include(i => i.LoanPurpose).Include(i => i.EntityLoanApplicationMappings).ToListAsync();
            if (!applications.Any())
            {
                pagedLoanApplications.Applications = new List<ApplicationAC>();
                return pagedLoanApplications;
            }
            pagedLoanApplications.TotalApplicationsCount = applications.Count;

            //Fetch the entity details for all the loan which are linked with an entity
            var applicationsWithEntityLinked = applications.Where(x => x.EntityLoanApplicationMappings.Any()).ToList();
            List<EntityLoanApplicationMapping> mappings = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => applicationsWithEntityLinked.Select(y => y.Id).ToList().Contains(x.LoanApplicationId))
                .Include(i => i.Entity.Company).ToListAsync();

            //Check for the filtering parameters and filter the list according to them if any
            if (filterModel != null)
            {
                //Sequence should be this only (filtering => sorting => paging)
                if (filterModel.Filters != null && filterModel.Filters.Any())
                {
                    applications = FilterLoanList(applications, mappings, filterModel.Filters);
                    pagedLoanApplications.TotalApplicationsCount = applications.Count;
                }
                if (filterModel.SortField != null && filterModel.SortBy != null)
                {
                    applications = SortLoanList(applications, filterModel.SortField, filterModel.SortBy);
                    pagedLoanApplications.TotalApplicationsCount = applications.Count;
                }
                if (filterModel.PageNo != null && filterModel.PageRecordCount != null)
                {
                    applications = PaginateLoanList(applications, filterModel.PageNo.Value, filterModel.PageRecordCount.Value);
                }
            }

            //If no any loan found after filtering
            if (!applications.Any())
            {
                pagedLoanApplications.Applications = new List<ApplicationAC>();
                return pagedLoanApplications;
            }

            //Map each loan with ApplicationDetailsAC object and add in the list.
            List<ApplicationAC> applicationDetailsList = new List<ApplicationAC>();
            foreach (var application in applications)
            {
                var applicationDetailsAC = new ApplicationAC
                {
                    BasicDetails = _mapper.Map<ApplicationBasicDetailAC>(application)
                };

                if (application.EntityLoanApplicationMappings.Any())
                {
                    var mapping = mappings.Single(x => x.Id.Equals(application.EntityLoanApplicationMappings.First().Id));
                    if (mapping != null)
                    {
                        applicationDetailsAC.BorrowingEntities = new List<EntityAC>
                        {
                            new EntityAC
                            {
                                Id = mapping.Entity.Company?.Id,
                                Company = new CompanyAC
                                {
                                    Name = mapping.Entity.Company?.Name
                                }
                            }
                        };
                    }
                }
                applicationDetailsList.Add(applicationDetailsAC);
            }
            pagedLoanApplications.Applications = applicationDetailsList;
            return pagedLoanApplications;
        }

        /// <summary>
        /// Method fetches all details of a particular loan application.
        /// </summary>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <param name="currentUser">Logged in user</param>
        /// <returns>ApplicationDetailAC object</returns>
        public async Task<ApplicationAC> GetLoanApplicationDetailsByIdAsync(Guid loanApplicationId, CurrentUserAC currentUser)
        {
            if (!currentUser.IsBankUser && !await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplicationId))
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            LoanApplication loanApplication = await _dataRepository.Fetch<LoanApplication>(x => x.Id == loanApplicationId).Include(i => i.Product).ThenInclude(i => i.DescriptionPoints).SingleOrDefaultAsync();
            if (loanApplication == null)
            {
                throw new DataNotFoundException(StringConstant.LoanApplicationNotExistsForGivenId);
            }
            else
            {
                ApplicationAC applicationDetail = new ApplicationAC();

                // If loan has locked, approved, rejected, referral, evaluationfailure status
                if (!loanApplication.Status.Equals(LoanApplicationStatusType.Draft) && !loanApplication.Status.Equals(LoanApplicationStatusType.Unlocked))
                {
                    var lockedApplication = await _dataRepository.Fetch<LoanApplicationSnapshot>(x => x.LoanApplicationId == loanApplicationId)
                                                .Include(i => i.LoanApplication).ThenInclude(i => i.UserLoanSectionMappings).ThenInclude(s => s.Section)
                                                .Include(i => i.LoanApplication).ThenInclude(i => i.UpdatedByBankUser).SingleOrDefaultAsync();
                    if (lockedApplication == null)
                    {
                        throw new DataNotFoundException(StringConstant.NoSnapShotFoundForLoanApplication);
                    }
                    applicationDetail = JsonConvert.DeserializeObject<ApplicationAC>(lockedApplication.ApplicationDetailsJson);

                    //Update the basic details of snapshot with the current updated details.
                    applicationDetail.BasicDetails = _mapper.Map<LoanApplication, ApplicationBasicDetailAC>(lockedApplication.LoanApplication);
                    applicationDetail.BasicDetails.SectionName = lockedApplication.LoanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(lockedApplication.LoanApplication.CreatedByUserId)).Section.Name;
                    return applicationDetail;
                }
                else
                {
                    loanApplication = await _dataRepository.Fetch<LoanApplication>(x => x.Id == loanApplicationId).Include(i => i.UserLoanSectionMappings).ThenInclude(s => s.Section)
                        .Include(i => i.EntityLoanApplicationMappings).Include(i => i.CreatedByUser).SingleOrDefaultAsync();

                    var borrowingEntities = await _dataRepository.Fetch<EntityRelationshipMapping>(x => loanApplication.EntityLoanApplicationMappings.Select(y => y.EntityId).ToList().Contains(x.PrimaryEntityId))
                        .Include(i => i.PrimaryEntity).ThenInclude(i => i.Address)
                        .Include(i => i.PrimaryEntity).ThenInclude(i => i.Company).ThenInclude(i => i.CompanyStructure)
                        .Include(i => i.PrimaryEntity).ThenInclude(i => i.Company).ThenInclude(i => i.NAICSIndustryType)
                        .Include(i => i.PrimaryEntity).ThenInclude(i => i.Company).ThenInclude(i => i.CompanySize)
                        .Include(i => i.PrimaryEntity).ThenInclude(i => i.Company).ThenInclude(i => i.BusinessAge)
                        .Include(i => i.PrimaryEntity).ThenInclude(i => i.Company).ThenInclude(i => i.IndustryExperience)
                        .Include(i => i.PrimaryEntity).ThenInclude(i => i.User)
                        .Include(i => i.PrimaryEntity).ThenInclude(i => i.EntityConsents).ThenInclude(i => i.Consent)
                        .Include(i => i.RelativeEntity).ThenInclude(i => i.Address)
                        .Include(i => i.RelativeEntity).ThenInclude(i => i.User)
                        .Include(i => i.RelativeEntity).ThenInclude(i => i.EntityConsents).ThenInclude(i => i.Consent)
                        .Include(i => i.Relationship)
                        .ToListAsync();

                    var borrowingEntitiesCustom = borrowingEntities.GroupBy(s => s.PrimaryEntityId).Select(e => new
                    {
                        PrimaryEntity = e.Select(y => y.PrimaryEntity).First(),
                        RelativeEntities = e.Select(y => new
                        {
                            y.RelativeEntity,
                            y.Relationship.Relation,
                            y.SharePercentage
                        }).ToList(),
                        SharePercentage = e.Select(y => y.SharePercentage).First(),
                        Relation = e.Select(y => y.Relationship.Relation).First()
                    }).ToList();

                    applicationDetail.BasicDetails = _mapper.Map<LoanApplication, ApplicationBasicDetailAC>(loanApplication);
                    applicationDetail.BasicDetails.SectionName = loanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(loanApplication.CreatedByUserId)).Section.Name;
                    //Fetch taxes, credit report, finances here and then use them for mapping below.

                    List<EntityAC> primaryEntities = new List<EntityAC>();
                    foreach (var borrowingEntity in borrowingEntitiesCustom)
                    {
                        List<EntityAC> relatives = new List<EntityAC>();
                        foreach (var relativeEntity in borrowingEntity.RelativeEntities)
                        {
                            relatives.Add(new EntityAC
                            {
                                Id = relativeEntity.RelativeEntity.Id,
                                Address = _mapper.Map<Address, AddressAC>(relativeEntity.RelativeEntity.Address),
                                Consents = _mapper.Map<List<EntityLoanApplicationConsent>, List<ConsentAC>>(relativeEntity.RelativeEntity.EntityConsents.Where(a => a.LoanApplicationId == loanApplication.Id).ToList()),
                                User = _mapper.Map<User, UserAC>(relativeEntity.RelativeEntity.User),
                                Type = relativeEntity.RelativeEntity.Type,
                                RelationMapping = new EntityRelationMappingAC
                                {
                                    Relation = new RelationshipAC
                                    {
                                        Name = relativeEntity.Relation
                                    },
                                    SharePercentage = relativeEntity.SharePercentage
                                }
                            });
                        }

                        primaryEntities.Add(new EntityAC
                        {
                            Id = borrowingEntity.PrimaryEntity.Id,
                            Address = _mapper.Map<Address, AddressAC>(borrowingEntity.PrimaryEntity.Address),
                            Consents = _mapper.Map<List<EntityLoanApplicationConsent>, List<ConsentAC>>(borrowingEntity.PrimaryEntity.EntityConsents.Where(a => a.LoanApplicationId == loanApplication.Id).ToList()),
                            Company = new CompanyAC
                            {
                                Name = borrowingEntity.PrimaryEntity.Company.Name,
                                BusinessAge = new BusinessAgeAC
                                {
                                    Id = borrowingEntity.PrimaryEntity.Company.BusinessAge.Id,
                                    Age = borrowingEntity.PrimaryEntity.Company.BusinessAge.Age,
                                    Order = borrowingEntity.PrimaryEntity.Company.BusinessAge.Order
                                },
                                CIN = borrowingEntity.PrimaryEntity.Company.CIN,
                                IndustryType = new IndustryTypeAC
                                {
                                    Id = borrowingEntity.PrimaryEntity.Company.NAICSIndustryTypeId,
                                    IndustryType = borrowingEntity.PrimaryEntity.Company.NAICSIndustryType.IndustryType,
                                    IndustryCode = borrowingEntity.PrimaryEntity.Company.NAICSIndustryType.IndustryCode,
                                    IndustrySectorId = borrowingEntity.PrimaryEntity.Company.NAICSIndustryType.NAICSParentSectorId
                                },
                                CompanySize = new CompanySizeAC
                                {
                                    Id = borrowingEntity.PrimaryEntity.Company.CompanySize.Id,
                                    Size = borrowingEntity.PrimaryEntity.Company.CompanySize.Size,
                                    Order = borrowingEntity.PrimaryEntity.Company.CompanySize.Order
                                },
                                CompanyStructure = new CompanyStructureAC
                                {
                                    Id = borrowingEntity.PrimaryEntity.Company.CompanyStructure.Id,
                                    Structure = borrowingEntity.PrimaryEntity.Company.CompanyStructure.Structure,
                                    Order = borrowingEntity.PrimaryEntity.Company.CompanyStructure.Order
                                },
                                IndustryExperience = new IndustryExperienceAC
                                {
                                    Id = borrowingEntity.PrimaryEntity.Company.IndustryExperience.Id,
                                    Experience = borrowingEntity.PrimaryEntity.Company.IndustryExperience.Experience,
                                    Order = borrowingEntity.PrimaryEntity.Company.IndustryExperience.Order,
                                    IsEnabled = borrowingEntity.PrimaryEntity.Company.IndustryExperience.IsEnabled
                                },
                                CompanyFiscalYearStartMonth = borrowingEntity.PrimaryEntity.Company.CompanyFiscalYearStartMonth,
                                CompanyRegisteredState = borrowingEntity.PrimaryEntity.Company.CompanyRegisteredState
                            },
                            User = _mapper.Map<User, UserAC>(borrowingEntity.PrimaryEntity.User),
                            Type = borrowingEntity.PrimaryEntity.Type,
                            RelationMapping = new EntityRelationMappingAC
                            {
                                Relation = new RelationshipAC
                                {
                                    Name = borrowingEntity.Relation
                                },
                                SharePercentage = borrowingEntity.SharePercentage
                            },
                            LinkedEntities = relatives,
                        });
                    }

                    // Add product in application detail
                    if (loanApplication.ProductId != Guid.Empty && loanApplication.ProductId != null)
                    {
                        applicationDetail.SelectedProduct = await GetRecommendedProductAsync(loanApplication);
                    }

                    applicationDetail.BorrowingEntities = primaryEntities;
                    return applicationDetail;
                }
            }
        }

        /// <summary>
        /// Method adds or updates loan application in database.
        /// </summary>
        /// <param name="loanApplicationBasicDetailAC">LoanApplicationBasicDetailAC object</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <returns>Returns updated object of LoanApplicationBasicDetailAC</returns>
        public async Task<ApplicationBasicDetailAC> SaveLoanApplicationAsync(ApplicationBasicDetailAC loanApplicationBasicDetailAC, CurrentUserAC currentUser, Guid? loanApplicationId)
        {
            // Check if the required fields are not null.
            if (loanApplicationBasicDetailAC.LoanAmount == 0 || loanApplicationBasicDetailAC.LoanPeriod == 0 || loanApplicationBasicDetailAC.LoanPurposeId == Guid.Empty)
            {
                throw new InvalidParameterException(StringConstant.InvalidDataProvidedInRequest);
            }

            // Check if the loan purpose is enabled or not.
            if (!(await _dataRepository.SingleAsync<LoanPurpose>(x => x.Id.Equals(loanApplicationBasicDetailAC.LoanPurposeId))).IsEnabled)
            {
                throw new InvalidParameterException(StringConstant.InvalidLoanPurposeProvidedInRequest);
            }

            // If Id is null then add new application else update the existing application.
            if (loanApplicationId == null)
            {
                loanApplicationBasicDetailAC.Status = LoanApplicationStatusType.Draft;

                // Private method call to get loan application number.
                loanApplicationBasicDetailAC.LoanApplicationNumber = GenerateLoanApplicationNumber();

                // Assign current user id.
                loanApplicationBasicDetailAC.EntityId = currentUser.Id;

                LoanApplication loanApplication = _mapper.Map<ApplicationBasicDetailAC, LoanApplication>(loanApplicationBasicDetailAC);

                // Add CreatedBy user id and CreatedOn date.
                loanApplication.CreatedByUserId = currentUser.Id;
                loanApplication.CreatedOn = DateTime.UtcNow;

                await _dataRepository.AddAsync<LoanApplication>(loanApplication);

                // Fetch the section id from Section table.
                Section nextSection = await _dataRepository.SingleAsync<Section>(x => x.Name.Equals(StringConstant.LoanProductSection));
                var userLoanSectionMapping = new UserLoanSectionMapping
                {
                    UserId = currentUser.Id,
                    SectionId = nextSection.Id,
                    LoanApplicationId = loanApplication.Id
                };
                await _dataRepository.AddAsync<UserLoanSectionMapping>(userLoanSectionMapping);

                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, loanApplication.Id);
                await _dataRepository.SaveChangesAsync(auditLog);

                // Reverse mapping to return updated loanApplicationBasicDetailAC.
                loanApplicationBasicDetailAC = _mapper.Map<LoanApplication, ApplicationBasicDetailAC>(loanApplication);
                loanApplicationBasicDetailAC.SectionName = nextSection.Name;
                return loanApplicationBasicDetailAC;
            }
            else
            {
                // Assign the application id, that comes in route, in object's id.
                loanApplicationBasicDetailAC.Id = loanApplicationId;

                if (!await _globalRepository.CheckUserLoanAccessAsync(currentUser, loanApplicationBasicDetailAC.Id.Value))
                {
                    throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
                }

                if (!await _globalRepository.IsAddOrUpdateAllowedAsync(loanApplicationBasicDetailAC.Id.Value))
                {
                    throw new InvalidResourceAccessException(StringConstant.UpdateOperationNotAllowed);
                }

                // Check if the required fields are null or not.
                if (loanApplicationBasicDetailAC.Id == Guid.Empty || loanApplicationBasicDetailAC.LoanApplicationNumber == null)
                {
                    throw new InvalidParameterException(StringConstant.InvalidDataProvidedInRequest);
                }

                LoanApplication existingLoanApplication = await _dataRepository.Fetch<LoanApplication>(x => x.Id.Equals(loanApplicationBasicDetailAC.Id)).Include(x => x.UserLoanSectionMappings).ThenInclude(s => s.Section).FirstOrDefaultAsync();

                // If application exists for given application id then only update it.
                if (existingLoanApplication == null)
                {
                    throw new InvalidParameterException(StringConstant.LoanApplicationNotExistsForGivenId);
                }

                LoanApplication loanApplication = _mapper.Map<ApplicationBasicDetailAC, LoanApplication>(loanApplicationBasicDetailAC, existingLoanApplication);

                // Update UpdatedBy user id and UpdatedOn date.
                loanApplication.UpdatedByUserId = currentUser != null ? currentUser.Id : Guid.Empty;
                loanApplication.UpdatedOn = DateTime.UtcNow;
                loanApplication.CreatedByUserId = existingLoanApplication.CreatedByUser.Id;
                _dataRepository.Update<LoanApplication>(loanApplication);
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, loanApplication.Id);
                await _dataRepository.SaveChangesAsync(auditLog);

                // Reverse mapping to return updated loanApplicationBasicDetailAC.
                loanApplicationBasicDetailAC = _mapper.Map<LoanApplication, ApplicationBasicDetailAC>(loanApplication);

                return loanApplicationBasicDetailAC;
            }
        }

        /// <summary>
        /// Delete loan applications by authorized bank users
        /// </summary>
        /// <param name="currentUser"></param>
        public async Task DeleteLoanApplicationsAsync(CurrentUserAC currentUser)
        {
            var authorizedEmailList = _configuration.GetSection("BankPreference:DbResetAuthorizedEmails").GetChildren().Select(x => x.Value).ToList();
            if (authorizedEmailList.Contains(currentUser.Email))
            {
                // Deep include all references of loan application that also will need to be deleted
                var loanToDeleteWithCascadedReferences = _dataRepository.GetAll<LoanApplication>()
                    .Include(x => x.EntityFinances).ThenInclude(x => x.EntityFinanceYearlyMappings).ThenInclude(x => x.EntityFinanceStandardAccounts)
                    .Include(x => x.EntityFinances).ThenInclude(x => x.PersonalFinanceResponses)
                    .Include(x => x.EntityLoanApplicationMappings)
                    .Include(x => x.AdditionalDocuments)
                    .Include(x => x.EntityTaxForms)
                    .Include(x => x.UserLoanSectionMappings);
                using (await _dataRepository.BeginTransactionAsync())
                {
                    _dataRepository.RemoveRange(loanToDeleteWithCascadedReferences);
                    await _dataRepository.SaveChangesAsync();
                    _dataRepository.CommitTransaction();
                }

            }
            else
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }
        }
        #endregion

        #region Bank details

        /// <summary>
        /// Fetch the bank details by loan id.
        /// </summary>
        /// <param name="applicationId">Loan application id</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>Bank details in LoanEntityBankDetailsAC object</returns>
        public async Task<LoanEntityBankDetailsAC> GetBankDetailsByLoanIdAsync(Guid applicationId, CurrentUserAC currentUser)
        {
            if (applicationId == Guid.Empty)
            {
                throw new InvalidParameterException(StringConstant.InvalidIdProvided);
            }

            //If the current user is bank user then access check is not needed
            if (!currentUser.IsBankUser && !await _globalRepository.CheckUserLoanAccessAsync(currentUser, applicationId))
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            LoanApplication loanApplication = await _dataRepository.Fetch<LoanApplication>(x => x.Id == applicationId)
                                                .Include(i => i.LoanAmountDepositeeBank).ThenInclude(r => r.Bank)
                                                .SingleOrDefaultAsync();

            //If the loan application is not found then throw an exception
            if (loanApplication == null)
            {
                throw new DataNotFoundException(StringConstant.LoanApplicationNotExistsForGivenId);
            }
            else
            {
                return _mapper.Map<LoanEntityBankDetailsAC>(loanApplication);
            }
        }

        /// <summary>
        /// Add or update bank details for approved loan.
        /// </summary>
        /// <param name="entityBankDetailsAC">EntityBankDetailsAC object</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        public async Task AddOrUpdateBankDetailsAsync(LoanEntityBankDetailsAC entityBankDetailsAC, CurrentUserAC currentUser)
        {
            //If the application id is empty then throw an exception
            if (entityBankDetailsAC.LoanApplicationId == Guid.Empty)
            {
                throw new InvalidParameterException(StringConstant.InvalidIdProvided);
            }

            //If loan application is not exist for given id then throw an exception
            var loanApplication = await _dataRepository.SingleOrDefaultAsync<LoanApplication>(x => x.Id.Equals(entityBankDetailsAC.LoanApplicationId));
            if (loanApplication == null)
            {
                throw new InvalidParameterException(StringConstant.LoanApplicationNotExistsForGivenId);
            }

            using (_dataRepository.BeginTransactionAsync())
            {
                //If the bank information id is not available then only add them.
                if (loanApplication.LoanAmountDepositeeBankId == null)
                {
                    //If loan is approved then only allow adding the bank details
                    if (loanApplication.Status != LoanApplicationStatusType.Approved)
                    {
                        throw new InvalidResourceAccessException(StringConstant.AddOperationNotAllowed);
                    }

                    var loanAmountDepositeeBankDetails = new EntityBankDetail
                    {
                        AccountNumber = entityBankDetailsAC.LoanAmountDepositeeBank.AccountNumber,
                        BankId = entityBankDetailsAC.LoanAmountDepositeeBank.BankId,
                        CreatedByUserId = currentUser.Id,
                        CreatedOn = DateTime.UtcNow
                    };
                    await _dataRepository.AddAsync(loanAmountDepositeeBankDetails);
                    loanApplication.LoanAmountDepositeeBankId = loanAmountDepositeeBankDetails.Id;
                    _dataRepository.Update(loanApplication);
                }
                else
                {
                    //Fetch the existing bank details and throw an exception if it is null.
                    var bankDetails = await _dataRepository.SingleOrDefaultAsync<EntityBankDetail>(x => x.Id.Equals(loanApplication.LoanAmountDepositeeBankId));
                    if (bankDetails == null)
                    {
                        throw new InvalidResourceAccessException(StringConstant.UpdateOperationNotAllowed);
                    }

                    //Update the bank details
                    bankDetails.UpdatedByUserId = currentUser.Id;
                    bankDetails.UpdatedOn = DateTime.UtcNow;
                    bankDetails.AccountNumber = entityBankDetailsAC.LoanAmountDepositeeBank.AccountNumber;
                    bankDetails.BankId = entityBankDetailsAC.LoanAmountDepositeeBank.BankId;
                    _dataRepository.Update(bankDetails);
                }
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, loanApplication.Id);
                await _dataRepository.SaveChangesAsync(auditLog);
                _dataRepository.CommitTransaction();
            }
        }
        #endregion

        #region Entity
        /// <summary>
        /// Link Loan application with borrowing entity
        /// </summary>
        /// <param name="applicationId">Unique identifier of application object</param>
        /// <param name="borrowingEntityId">Unique identifier of entity object</param>
        /// <param name="currentUser">CurrentUserAC object</param>
        /// <returns></returns>
        public async Task LinkApplicationWithEntityAsync(Guid applicationId, Guid borrowingEntityId, CurrentUserAC currentUser)
        {
            //Check if current user has access to loan application
            bool checkLoanAccess = await _dataRepository.Fetch<LoanApplication>(x => x.Id.Equals(applicationId) && x.CreatedByUserId == currentUser.Id).AnyAsync();
            if (!checkLoanAccess)
            {
                throw new InvalidResourceAccessException(StringConstant.NoAccessToLinkLoan);
            }

            Entity entity = await _dataRepository.SingleAsync<Entity>(x => x.Id.Equals(borrowingEntityId));

            if (entity.Type == EntityType.Company)
            {
                await LinkApplicationWithCompanyAsync(applicationId, borrowingEntityId, currentUser);
            }
        }

        /// <summary>
        /// Link Loan application with borrowing company
        /// </summary>
        /// <param name="applicationId">Unique identifier of application object</param>
        /// <param name="borrowingEntityId">Unique identifier of entity object</param>
        /// <param name="currentUser">CurrentUserAC object</param>
        /// <returns></returns>
        private async Task LinkApplicationWithCompanyAsync(Guid applicationId, Guid borrowingEntityId, CurrentUserAC currentUser)
        {
            //Check if current user has access to company
            bool checkCompanyAccess = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(borrowingEntityId) && x.RelativeEntityId.Equals(currentUser.Id)).AnyAsync();

            if (!checkCompanyAccess)
            {
                throw new InvalidResourceAccessException(StringConstant.NoAccessToLinkLoan);
            }

            List<EntityRelationshipMapping> entityRelationshipMappingList = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(borrowingEntityId))
                .Include(x => x.PrimaryEntity.Company).ToListAsync();

            List<CompanyStructure> companyStructureList = await _dataRepository.GetAll<CompanyStructure>().ToListAsync();
            Guid proprietorshipId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.Proprietorship)).Select(s => s.Id).Single();
            Guid partnershipId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.Partnership)).Select(s => s.Id).Single();
            Guid limitedLiabilityCompanyId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Select(s => s.Id).Single();
            Guid cCorporationId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.CCorporation)).Select(s => s.Id).Single();
            Guid sCorporationId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.SCorporation)).Select(s => s.Id).Single();

            decimal TotalSharePercentage = entityRelationshipMappingList.Select(s => s.SharePercentage).Sum().Value;

            if (entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyStructureId == proprietorshipId && (entityRelationshipMappingList.Count != 1 || TotalSharePercentage != 100))
            {
                throw new InvalidParameterException(StringConstant.ProprietorOnlyOneUser);
            }
            else if (entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyStructureId == partnershipId && ((entityRelationshipMappingList.Select(s => s.SharePercentage).Sum() != 100) || entityRelationshipMappingList.Count == 1))
            {
                throw new InvalidParameterException(StringConstant.MoreThanOneShareHolderRequired);
            }
            else if (entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyStructureId == limitedLiabilityCompanyId || entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyStructureId == cCorporationId || entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyStructureId == sCorporationId)
            {
                if (entityRelationshipMappingList.Count == 1 && TotalSharePercentage < _configuration.GetValue<decimal>("Entity:MajoritySharePercentage"))
                {
                    throw new InvalidParameterException(StringConstant.NotMajoritySharePercentage);
                }
                else if (entityRelationshipMappingList.Count > 1 && TotalSharePercentage != 100)
                {
                    throw new InvalidParameterException(StringConstant.SharePercentageNotHundred);
                }
            }

            List<EntityLoanApplicationMapping> checkEntityLoanApplicationMappings = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => x.EntityId.Equals(borrowingEntityId)).Include(x => x.LoanApplication).ToListAsync();

            if (!checkEntityLoanApplicationMappings.Any() || (checkEntityLoanApplicationMappings.SingleOrDefault(x => x.LoanApplicationId.Equals(applicationId)) == null && checkEntityLoanApplicationMappings.Any(x => x.LoanApplication.Status != LoanApplicationStatusType.Draft)))
            {
                EntityLoanApplicationMapping entityLoanApplicationMapping = new EntityLoanApplicationMapping()
                {
                    EntityId = borrowingEntityId,
                    LoanApplicationId = applicationId
                };
                await _dataRepository.AddAsync<EntityLoanApplicationMapping>(entityLoanApplicationMapping);
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, applicationId);
                await _dataRepository.SaveChangesAsync(auditLog);
            }
            else
            {
                throw new InvalidParameterException(StringConstant.NotAllowedToStartNewLoan);
            }
        }
        #endregion

        #region Consent

        /// <summary>
        /// Method saves the consent of a user for a given loan application.
        /// </summary>
        /// <param name="applicationId">Loan application id for which the user's consent is to be saved</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        public async Task SaveLoanConsentOfUserAsync(Guid applicationId, CurrentUserAC currentUser)
        {
            if (!await _globalRepository.CheckUserLoanAccessAsync(currentUser, applicationId))
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            if (!await _globalRepository.IsAddOrUpdateAllowedAsync(applicationId))
            {
                throw new InvalidResourceAccessException(StringConstant.UpdateOperationNotAllowed);
            }

            //Check if the user has not already given the consent for the given application.
            List<EntityLoanApplicationConsent> existingConsents = await _dataRepository.Fetch<EntityLoanApplicationConsent>(x => x.LoanApplicationId == applicationId).ToListAsync();

            if (!existingConsents.Any(x => x.ConsenteeId.Equals(currentUser.Id)))
            {
                //Get the relatives if any, for the given loan application.
                var entityMappingList = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => x.LoanApplicationId == applicationId)
                    .Include(i => i.LoanApplication).ThenInclude(i => i.CreatedByUser)
                    .Include(i => i.Entity).ThenInclude(i => i.EntityFinances).ThenInclude(i => i.FinancialStatement)
                    .Include(i => i.Entity).ThenInclude(i => i.EntityFinances).ThenInclude(i => i.EntityFinanceYearlyMappings)
                    .Include(i => i.Entity).ThenInclude(i => i.EntityTaxForms).ToListAsync();

                if (!entityMappingList.Any() || (entityMappingList.Any() && (entityMappingList.Any(x => x.LoanApplication == null) || entityMappingList.Any(x => x.Entity == null))))
                {
                    throw new DataNotFoundException(StringConstant.NoEntityLoanApplicationMappingFound);
                }

                //Fetch all the relatives of all the linked entities.
                var relativesIds = await _dataRepository.Fetch<EntityRelationshipMapping>(x => entityMappingList.Select(x => x.EntityId).Contains(x.PrimaryEntityId)).Select(x => x.RelativeEntityId).ToListAsync();

                //If current user is initiator.
                var entityMapping = entityMappingList.FirstOrDefault(x => x.LoanApplication.CreatedByUserId == currentUser.Id);
                if (entityMapping != null)
                {
                    //Check if all the sections are filled up.
                    ValidateAllSectionCompletion(entityMapping);

                    using (await _dataRepository.BeginTransactionAsync())
                    {
                        //Save the interest rate of loan intiator if not null.
                        if (entityMapping.LoanApplication.CreatedByUser == null || string.IsNullOrEmpty(entityMapping.LoanApplication.CreatedByUser.SelfDeclaredCreditScore))
                        {
                            throw new InvalidParameterException(StringConstant.LoanInitiatorOrSelfDeclaredCreditScoreNotFound);
                        }

                        entityMapping.LoanApplication.InterestRate = _globalRepository.GetInterestRateForGivenSelfDeclaredCreditScore(entityMapping.LoanApplication.CreatedByUser.SelfDeclaredCreditScore).Value;
                        _dataRepository.Update<LoanApplication>(entityMapping.LoanApplication);

                        //Add other shareholders section as personal finances.
                        List<UserLoanSectionMapping> userLoanSectionMappings = new List<UserLoanSectionMapping>();
                        Guid personalFinanceSectionId = (await _dataRepository.Fetch<Section>(x => x.Name.Equals(StringConstant.PersonalFinancesSection)).SingleAsync()).Id;
                        foreach (var relativesId in relativesIds.Where(x => !x.Equals(currentUser.Id)))
                        {
                            UserLoanSectionMapping userLoanSectionMapping = new UserLoanSectionMapping
                            {
                                LoanApplicationId = applicationId,
                                UserId = relativesId,
                                SectionId = personalFinanceSectionId
                            };
                            userLoanSectionMappings.Add(userLoanSectionMapping);
                        }

                        await _dataRepository.AddRangeAsync(userLoanSectionMappings);

                        // Prepare the Auditlog object to save the custom fields in the dbcontext.
                        AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, applicationId);
                        await _dataRepository.SaveChangesAsync(auditLog);

                        //Save the consent
                        await SaveConsentsOfGivenUserAsync(applicationId, currentUser);

                        _dataRepository.CommitTransaction();
                    }

                    //If loan initiator has given a consent and email service is set enable then only send mails to shareholders if any.
                    if (relativesIds.Count > 1 && _configuration.GetValue<bool>("EmailService:IsEmailServiceEnabled"))
                    {
                        await SendReminderEmailToShareHoldersAsync(applicationId);
                    }
                }
                //If current user is not loan initiator then check whether he is last or not.
                else
                {
                    //If initiator has not given consent and another shareholder is trying to give consent then throw error.
                    if (!existingConsents.Any(x => x.ConsenteeId.Equals(entityMappingList.First().LoanApplication.CreatedByUserId)))
                    {
                        throw new InvalidOperationException(StringConstant.NotAllowedToGiveConsent);
                    }

                    //Save the consent
                    await SaveConsentsOfGivenUserAsync(applicationId, currentUser);
                }
            }
        }

        /// <summary>
        /// Method sends a reminder email to shareholders who have not given consent for loan application yet.
        /// </summary>
        /// <param name="loanApplicationId">Nullable loan application id</param>
        /// <returns></returns>
        public async Task SendReminderEmailToShareHoldersAsync(Guid? loanApplicationId)
        {
            // If loan application id is given in argument then fetch those records only.
            List<EntityLoanApplicationConsent> usersNotGivenConsent = loanApplicationId != null && loanApplicationId != Guid.Empty ?
                await _dataRepository.Fetch<EntityLoanApplicationConsent>(x => x.LoanApplicationId.Equals(loanApplicationId.Value))
                    .Include(x => x.LoanApplication).ToListAsync() : await _dataRepository.GetAll<EntityLoanApplicationConsent>()
                    .Include(x => x.LoanApplication).ToListAsync();

            if (!usersNotGivenConsent.Any())
            {
                return;
            }

            var conseteeApplicationIdList = usersNotGivenConsent.Select(x => new
            {
                x.ConsenteeId,
                x.LoanApplicationId
            }).Distinct().ToList();

            var mappings = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => conseteeApplicationIdList.Select(x => x.LoanApplicationId).Contains(x.LoanApplicationId)).ToListAsync();
            if (!mappings.Any())
            {
                return;
            }
            var relatives = await _dataRepository.Fetch<EntityRelationshipMapping>(x => mappings.Select(x => x.EntityId).Contains(x.PrimaryEntityId)).ToListAsync();
            if (!relatives.Any())
            {
                return;
            }

            foreach (var applicationId in conseteeApplicationIdList.Select(x => x.LoanApplicationId).Distinct().ToList())
            {
                var allRelatives = relatives.Where(x => mappings.Where(y => y.LoanApplicationId == applicationId).Select(y => y.EntityId).ToList().Contains(x.PrimaryEntityId)).Select(x => x.RelativeEntityId).ToList();
                if (conseteeApplicationIdList.Count(x => x.LoanApplicationId == applicationId) < allRelatives.Count)
                {
                    foreach (var relativeId in allRelatives.Where(x => !conseteeApplicationIdList.Where(y => y.LoanApplicationId == applicationId).Select(z => z.ConsenteeId).ToList().Contains(x)))
                    {
                        usersNotGivenConsent.Add(new EntityLoanApplicationConsent
                        {
                            ConsenteeId = relativeId,
                            LoanApplicationId = applicationId
                        });
                    }
                }
            }

            if (!usersNotGivenConsent.Any())
            {
                return;
            }

            // Fetch the entities related to loan applications.
            var entityLoanMappings = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => usersNotGivenConsent.Select(y => y.LoanApplicationId).ToList().Contains(x.LoanApplicationId))
                            .Include(i => i.Entity.Company).Include(i => i.LoanApplication)
                            .Select(x => new
                            {
                                x.EntityId,
                                CompanyName = x.Entity.Company.Name,
                                x.LoanApplication.LoanApplicationNumber
                            }).ToListAsync();

            if (!entityLoanMappings.Any())
            {
                return;
            }

            // Fetch all the users with relation for filtered entity.
            var entityRelationshipMappings = await _dataRepository.Fetch<EntityRelationshipMapping>(x => entityLoanMappings.Select(y => y.EntityId).ToList().Contains(x.PrimaryEntityId)
                                        && usersNotGivenConsent.Select(z => z.ConsenteeId).ToList().Contains(x.RelativeEntityId))
                                .Include(x => x.RelativeEntity.User).Include(x => x.Relationship)
                                .Select(x => new
                                {
                                    x.PrimaryEntityId,
                                    UserName = string.Format("{0} {1}", x.RelativeEntity.User.FirstName, x.RelativeEntity.User.LastName),
                                    UserEmail = x.RelativeEntity.User.Email,
                                    UserRelation = x.Relationship.Relation
                                }).ToListAsync();

            if (!entityRelationshipMappings.Any())
            {
                return;
            }

            List<EmailLoanDetailsAC> emailLoanDetailsACList = new List<EmailLoanDetailsAC>();
            foreach (var entityMapping in entityLoanMappings)
            {
                EmailLoanDetailsAC emailLoanDetails = new EmailLoanDetailsAC
                {
                    LoanApplicationNumber = entityMapping.LoanApplicationNumber,
                    CompanyName = entityMapping.CompanyName,
                    Subject = loanApplicationId != Guid.Empty && loanApplicationId != null ? StringConstant.EmailTemplates.Single(x => x.Key == 1).Value.First() : StringConstant.EmailTemplates.Single(x => x.Key == 2).Value.First(),
                    TemplateFile = loanApplicationId != Guid.Empty && loanApplicationId != null ? StringConstant.EmailTemplates.Single(x => x.Key == 1).Value.Last() : StringConstant.EmailTemplates.Single(x => x.Key == 2).Value.Last(),
                    LoanApplicationRedirectUrl = StringConstant.LoanApplicationRedirectUrl
                };

                List<BasicUserDetailsAC> shareHolders = new List<BasicUserDetailsAC>();
                var filteredMappings = entityRelationshipMappings.Where(x => x.PrimaryEntityId.Equals(entityMapping.EntityId)).ToList();
                foreach (var filteredMapping in filteredMappings)
                {
                    BasicUserDetailsAC user = new BasicUserDetailsAC
                    {
                        Email = filteredMapping.UserEmail,
                        Name = filteredMapping.UserName,
                        Relationship = filteredMapping.UserRelation
                    };
                    shareHolders.Add(user);
                }
                emailLoanDetails.ShareHoldersDetails = shareHolders;
                emailLoanDetailsACList.Add(emailLoanDetails);
            }
            await _simpleEmailServiceUtility.SendEmailToShareHoldersAsync(emailLoanDetailsACList);
        }

        /// <summary>
        /// Method locks the loan application with its current details.
        /// </summary>
        /// <param name="applicationId">Id of the loan application which needs to locked</param>
        /// <param name="entities">List of entities(shareholders) with their finance summary JSON</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        public async Task LockLoanApplicationByIdAsync(Guid applicationId, List<EntityAC> entities, CurrentUserAC currentUser)
        {
            if (!await _globalRepository.CheckUserLoanAccessAsync(currentUser, applicationId))
            {
                throw new UnauthorizedAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            if (!await _globalRepository.IsAddOrUpdateAllowedAsync(applicationId))
            {
                throw new InvalidOperationException(StringConstant.UpdateOperationNotAllowed);
            }

            var loanApplication = await _dataRepository.SingleOrDefaultAsync<LoanApplication>(x => x.Id == applicationId);
            if (loanApplication == null)
            {
                throw new DataNotFoundException(StringConstant.LoanApplicationNotExistsForGivenId);
            }

            //If an entity linked with the appliction is not found then throws an exception.
            var entityMappingList = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => x.LoanApplicationId == applicationId)
                .Include(i => i.LoanApplication)
                .Include(x => x.Entity.EntityFinances)
                .Include(x => x.Entity.PrimaryEntityRelationships).ThenInclude(x => x.RelativeEntity.EntityFinances)
                .Include(x => x.Entity.EntityTaxForms)
                .Include(x => x.Entity.AdditionalDocuments)
                .ToListAsync();

            if (!entityMappingList.Any() || (entityMappingList.Any() && entityMappingList.Any(x => x.LoanApplication == null)))
            {
                throw new DataNotFoundException(StringConstant.NoEntityLoanApplicationMappingFound);
            }

            var relativesIds = await _dataRepository.Fetch<EntityRelationshipMapping>(x => entityMappingList.Select(x => x.EntityId).Contains(x.PrimaryEntityId)).Select(x => x.RelativeEntityId).ToListAsync();
            var existingConsents = await _dataRepository.Fetch<EntityLoanApplicationConsent>(x => x.LoanApplicationId == applicationId).ToListAsync();

            //If no relatives or consent of any relative not found 
            if (!relativesIds.Any() || !existingConsents.Any())
            {
                throw new DataNotFoundException(StringConstant.NoRelativesFoundOrNoAnyRelativesConsentFound);
            }

            //If all the relatives have not given consent then do not lock the application.
            if (existingConsents.Select(x => x.ConsenteeId).Distinct().ToList().Count != relativesIds.Count)
            {
                throw new InvalidOperationException(StringConstant.NotAllLinkedEntitiesHaveGivenConsent);
            }

            //If cosents of all the users have been received then update the section and status.
            using (await _dataRepository.BeginTransactionAsync())
            {
                //Make the JSON of all the data at current state and save it.
                await _dataRepository.AddAsync<LoanApplicationSnapshot>(new LoanApplicationSnapshot
                {
                    LoanApplicationId = applicationId,
                    ApplicationDetailsJson = JsonConvert.SerializeObject(await GetLoanApplicationDetailsByIdAsync(applicationId, currentUser)),
                    CreatedByUserId = currentUser.Id,
                    CreatedOn = DateTime.UtcNow
                });

                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, applicationId);
                await _dataRepository.SaveChangesAsync(auditLog);

                //Udpate the section and status.
                await _globalRepository.UpdateSectionNameAsync(applicationId, StringConstant.LoanConsentSection, currentUser);
                await _globalRepository.UpdateStatusOfLoanApplicationAsync(applicationId, LoanApplicationStatusType.Locked, currentUser);

                if (entities.Any())
                {
                    //Add personal finance Json
                    await SavePersonalFinanceSummaryJsonAsync(entities);
                }


                AddVersion(entityMappingList.SelectMany(x => x.Entity.EntityFinances).Where(x => x.LoanApplicationId == null).AsQueryable(), applicationId);
                AddVersion(entityMappingList.SelectMany(x => x.Entity.EntityTaxForms).Where(x => x.LoanApplicationId == null).AsQueryable(), applicationId);
                AddVersion(entityMappingList.SelectMany(x => x.Entity.PrimaryEntityRelationships.SelectMany(x => x.RelativeEntity.EntityFinances)).Where(x => x.LoanApplicationId == null).AsQueryable(), applicationId);
                //If additional documents are saved for an entity then only they will exist in DB and then only need to version them.
                if (entityMappingList.SelectMany(x => x.Entity.AdditionalDocuments).Any())
                {
                    AddVersion(entityMappingList.SelectMany(x => x.Entity.AdditionalDocuments).Where(x => x.LoanApplicationId == null).AsQueryable(), applicationId);
                }
                await _dataRepository.SaveChangesAsync();
                _dataRepository.CommitTransaction();
            }
        }

        /// <summary>
        /// Versioning of taxes and company finances
        /// </summary>
        /// <returns></returns>
        private void AddVersion<T>(IQueryable<T> collectionToVersion, Guid applicationId) where T : class
        {
            _dataRepository.UpdateRange(collectionToVersion.VersionThisQueryable(applicationId));
        }

        #endregion

        #region Loan Evaluation and Status update
        /// <summary>
        /// Evaluate Loan and get loan status
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<ApplicationBasicDetailAC> EvaluateLoanAsync(Guid loanId, CurrentUserAC currentUser)
        {
            if (loanId == Guid.Empty)
            {
                throw new InvalidParameterException();
            }

            var entityLoanMapping = await _dataRepository.Fetch<LoanApplication>(x => x.Id == loanId)
                .Include(x => x.EntityLoanApplicationMappings).ThenInclude(x => x.Entity).ThenInclude(x => x.EntityFinances).ThenInclude(x => x.EntityFinanceYearlyMappings)
                .ThenInclude(x => x.EntityFinanceStandardAccounts)
                .Include(x => x.EntityLoanApplicationMappings).ThenInclude(x => x.Entity).ThenInclude(x => x.EntityFinances).ThenInclude(x => x.FinancialStatement)
                .ToListAsync();
            if (entityLoanMapping.All(x => x.Status == LoanApplicationStatusType.Draft || x.Status == LoanApplicationStatusType.Unlocked))
            {
                throw new InvalidResourceAccessException();
            }
            else if (entityLoanMapping.All(x => x.Status == LoanApplicationStatusType.Approved || x.Status == LoanApplicationStatusType.Referral || x.Status == LoanApplicationStatusType.Rejected))
            {
                return new ApplicationBasicDetailAC
                {
                    Id = loanId,
                    EvaluationComments = entityLoanMapping.First().EvaluationComments,
                    Status = entityLoanMapping.First().Status
                };
            }
            var financialAccountBalanceList = new List<FinancialAccountBalanceAC>();
            var entityLoanApplicationMappingList = entityLoanMapping.Select(x => x.EntityLoanApplicationMappings).ToList();
            foreach (var entity in entityLoanApplicationMappingList)
            {
                var entityFinances = entity.Select(x => x.Entity.EntityFinances).ToList();
                foreach (var finance in entityFinances)
                {
                    var yearlyMapping = finance.Where(x => x.FinancialStatement.Name.Equals(StringConstant.FinancialRatios, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.EntityFinanceYearlyMappings).ToList();
                    foreach (var yearlyValueList in yearlyMapping)
                    {
                        foreach (var yearlyValue in yearlyValueList)
                        {
                            foreach (var financeAccount in yearlyValue.EntityFinanceStandardAccounts)
                            {
                                var financeAccountAC = _mapper.Map<FinancialAccountBalanceAC>(financeAccount);
                                financeAccountAC.Period = yearlyValue.Period;
                                financialAccountBalanceList.Add(financeAccountAC);
                            }
                        }
                    }
                }
            }

            financialAccountBalanceList = financialAccountBalanceList.Where(x => x.ExpectedValue != null).ToList();
            ApplicationBasicDetailAC applicationBasicDetails = new ApplicationBasicDetailAC();
            applicationBasicDetails.Id = loanId;

            var updatedApplicationBasicDetailAC = await UpdateLoanApplicationStatusAsync(await PrepareApplicationStatusObjectAsync(financialAccountBalanceList, applicationBasicDetails), currentUser);

            await AddTaxReturnExtractedValueFromOCRAsync(loanId);

            return updatedApplicationBasicDetailAC;
        }

        /// <summary>
        /// Update loan application status
        /// </summary>
        /// <param name="loanApplication"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<ApplicationBasicDetailAC> UpdateLoanApplicationStatusAsync(ApplicationBasicDetailAC loanApplication, CurrentUserAC currentUser)
        {
            var loan = await _dataRepository.Fetch<LoanApplication>(x => x.Id == loanApplication.Id).SingleOrDefaultAsync();

            if (loan != null && (loan.Status == LoanApplicationStatusType.Locked || loan.Status == LoanApplicationStatusType.Referral || loan.Status == LoanApplicationStatusType.EvaluationFailure))
            {
                using (await _dataRepository.BeginTransactionAsync())
                {
                    loan.Status = loanApplication.Status;
                    loan.UpdatedOn = DateTime.UtcNow;
                    if (currentUser.IsBankUser)
                    {
                        loan.UpdatedByBankUserId = currentUser.Id;
                    }

                    if (loan.Status == LoanApplicationStatusType.Approved)
                        await _globalRepository.UpdateSectionNameAsync(loan.Id, StringConstant.LoanStatusSection, currentUser);

                    if (!string.IsNullOrEmpty(loanApplication.EvaluationComments))
                    {
                        loan.EvaluationComments = loanApplication.EvaluationComments;
                    }
                    _dataRepository.Update(loan);
                    // Prepare the Auditlog object to save the custom fields in the dbcontext.
                    AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, loanApplication.Id);
                    await _dataRepository.SaveChangesAsync(auditLog);
                    _dataRepository.CommitTransaction();
                }
                return loanApplication;
            }
            else
            {
                throw new InvalidParameterException();
            }
        }

        /// <summary>
        /// Method to prepare application status object
        /// </summary>
        /// <param name="financialAccountBalanceList"></param>
        /// <param name="applicationBasicDetails"></param>
        /// <returns></returns>
        private async Task<ApplicationBasicDetailAC> PrepareApplicationStatusObjectAsync(List<FinancialAccountBalanceAC> financialAccountBalanceList, ApplicationBasicDetailAC applicationBasicDetails)
        {
            if (financialAccountBalanceList.Any())
            {
                var inputObj = new
                {
                    FinancialAccountBalance = financialAccountBalanceList
                };


                var dmnDecision = await _rulesUtility.ExecuteRuleForEvaluatingLoanAsync(inputObj);
                if (dmnDecision != null)
                {
                    var approvedList = JsonConvert.DeserializeObject<List<FinancialAccountBalanceAC>>(JsonConvert.SerializeObject(dmnDecision["ApprovedList"]));
                    var rejectedList = JsonConvert.DeserializeObject<List<FinancialAccountBalanceAC>>(JsonConvert.SerializeObject(dmnDecision["RejectedList"]));
                    var approvedPeriodicList = approvedList.GroupBy(x => x.Period).Select(x => new PeriodicFinancialAccountsAC
                    {
                        Period = x.Key,
                        FinancialAccountBalances = x.ToList()
                    }).ToList();

                    var rejectedPeriodicList = rejectedList.GroupBy(x => x.Period).Select(x => new PeriodicFinancialAccountsAC
                    {
                        Period = x.Key,
                        FinancialAccountBalances = x.ToList()
                    }).ToList();

                    if (rejectedPeriodicList.Any())
                    {
                        applicationBasicDetails.Status = LoanApplicationStatusType.Rejected;
                        applicationBasicDetails.EvaluationComments = JsonConvert.SerializeObject(rejectedPeriodicList);
                    }
                    else if (approvedPeriodicList.Any())
                    {
                        applicationBasicDetails.Status = LoanApplicationStatusType.Approved;
                        applicationBasicDetails.EvaluationComments = JsonConvert.SerializeObject(approvedPeriodicList);
                    }
                    else
                    {
                        applicationBasicDetails.Status = LoanApplicationStatusType.Referral;
                        applicationBasicDetails.EvaluationComments = StringConstant.InsufficientEvaluationData;
                    }
                }
                else
                {
                    applicationBasicDetails.Status = LoanApplicationStatusType.EvaluationFailure;
                }
            }
            else
            {
                applicationBasicDetails.Status = LoanApplicationStatusType.Referral;
                applicationBasicDetails.EvaluationComments = StringConstant.InsufficientEvaluationData;
            }

            // For this mvp keep the status referral irrespective of any condition
            applicationBasicDetails.Status = LoanApplicationStatusType.Referral;
            return applicationBasicDetails;
        }
        #endregion

        #endregion
    }
}

