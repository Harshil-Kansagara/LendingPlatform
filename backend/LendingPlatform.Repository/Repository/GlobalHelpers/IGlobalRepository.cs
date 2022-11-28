using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.AppSettings;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.ApplicationClass.Products;
using LendingPlatform.Utils.ApplicationClass;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.GlobalHelpers
{
    public interface IGlobalRepository
    {
        /// <summary>
        /// Method that verifies if the logged in user is related to (or has access to) the passed entityid
        /// 
        /// Returns true if relation exists, else false
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="isClient">Flag to check whether the current user is client or bank user</param>
        /// <returns></returns>
        Task<bool> CheckEntityRelationshipMappingAsync(Guid entityId, Guid currentUserId, bool isClient = true);

        /// <summary>
        /// Get financial statement from statement name
        /// </summary>
        /// <param name="reportName"></param>
        /// <returns></returns>
        Task<FinancialStatement> GetFinancialStatementFromNameAsync(string reportName);

        /// <summary>
        /// Check if CIN is in valid format for USA
        /// </summary>
        /// <param name="cin"></param>
        /// <returns></returns>
        bool IsValidCIN(string cin);

        /// <summary>
        /// Check if EIN is unique
        /// </summary>
        /// <param name="ein"></param>
        /// <returns></returns>
        Task<bool> IsUniqueEINAsync(string ein);

        /// <summary>
        /// Get last year period list based on app settings.
        /// </summary>
        /// <param name="financialYearStartMonth">Start month of financial year</param>
        /// <param name="financialYearEndMonth">Start month of financial year</param>
        /// <param name="currentYear">If want current year then pass true</param>
        /// <returns></returns>
        List<string> GetListOfLastNFinancialYears(string financialYearStartMonth, string financialYearEndMonth, bool currentYear = false);


        /// <summary>
        /// Method to update section name of a loan application on adding details in each component.
        /// </summary>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <param name="sectionName">Current section name</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>Updated section name</returns>
        Task<string> UpdateSectionNameAsync(Guid loanApplicationId, string sectionName, CurrentUserAC currentUser);

        /// <summary>
        /// Method to get uploaded files using document id.
        /// </summary>
        /// <param name="documentId">Uploaded document id.</param>
        /// <returns>Url.</returns>
        Task<string> GetPreSignedUrlAsync(Guid documentId);

        /// <summary>
        /// Method to get the path for key name bucket.
        /// </summary>
        /// <param name="entityId">Entity Id</param>
        /// <param name="entityTaxAccountAC">EntityTaxAccountAC object.</param>        
        /// <returns>return path for key name bucket.</returns>
        string GetPathForKeyNameBucket(Guid entityId, EntityTaxAccountAC entityTaxAccountAC);

        /// <summary>
        /// Method to get the path for key name bucket (for additional documents)
        /// </summary>
        /// <param name="entityId">Entity Id</param>
        /// <param name="additionalDocument">AdditionalDocumentAC object</param>
        /// <returns>Return path for key name bucket for additional document</returns>
        string GetPathForKeyNameBucketForAdditionalDocument(Guid entityId, AdditionalDocumentAC additionalDocument);

        /// <summary>
        /// Method to verify that loan application exists and in correct status to do the add/update operation.
        /// </summary>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <returns>Returns a boolean value</returns>
        Task<bool> IsAddOrUpdateAllowedAsync(Guid loanApplicationId);

        /// <summary>
        /// Method to update status of a loan application on consents are given by all the shareholders.
        /// </summary>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <param name="status">Status to update</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        Task UpdateStatusOfLoanApplicationAsync(Guid loanApplicationId, LoanApplicationStatusType status, CurrentUserAC currentUser);

        /// <summary>
        /// Check if user is accessing his own loan only
        /// </summary>
        /// <param name="currentUser">Current logged in user</param>
        /// <param name="loanApplicationId">Loan application id</param>
        /// <param name="isClient">Flag to check whether the current user is client or bank user</param>
        /// <returns></returns>
        Task<bool> CheckUserLoanAccessAsync(CurrentUserAC currentUser, Guid loanApplicationId, bool isClient = true);

        /// <summary>
        /// Fetch only those loan applications which are accessible by current user.
        /// </summary>
        /// <param name="currentUser">Current logged in user</param>
        /// <param name="loanApplicationList">List of ApplicationBasicDetailAC objects</param>
        /// <param name="isClient">Flag to check whether the current user is client or bank user</param>
        /// <returns>List of ApplicationBasicDetailAC objects</returns>
        Task<List<ApplicationBasicDetailAC>> CheckUserLoanAccessAsync(CurrentUserAC currentUser, List<ApplicationBasicDetailAC> loanApplicationList, bool isClient = true);

        /// <summary>
        /// Check if loan is readonly
        /// </summary>
        /// <param name="loanApplication"></param>
        /// <param name="isConsentSection"></param>
        /// <returns></returns>
        Task<bool> IsLoanReadOnlyAsync(LoanApplication loanApplication, bool isConsentSection = false);

        /// <summary>
        /// Check if phone number is unique
        /// </summary>
        /// <param name="phoneNumberList"></param>
        /// <returns></returns>
        Task IsUniquePhoneNumberAsync(List<string> phoneNumberList);

        /// <summary>
        /// Convert utc date to local date
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        DateTime ConvertUtcDateToLocalDate(DateTime utcDateTime);

        /// <summary>
        /// Method to convert xml to json and return JObject.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        JObject ConvertXmlToJson(string xml);

        #region New implemenation

        /// <summary>
        /// Fetch query select fields as per the fields parameter.
        /// </summary>
        /// <param name="fields">request parameter fields value</param>
        /// <param name="className">Class name</param>
        /// <returns>Returns query select fields</returns>
        string GetQuerySelectFieldsFromString(string fields, object className);

        /// <summary>
        /// Returns all the consent statements.
        /// </summary>
        /// <returns>List of ConsentStatementAC objects</returns>
        Task<List<ConsentStatementAC>> GetConsentStatementsAsync();

        /// <summary>
        /// Get the list of banks.
        /// </summary>
        /// <returns>List of BankAC objects</returns>
        Task<List<ApplicationClass.Others.BankAC>> GetAllBanksAsync();

        /// <summary>
        /// Method fetches all the purposes with its type for the loan application.
        /// </summary>
        /// <returns>List of LoanPurposeAC</returns>
        Task<List<ApplicationClass.Others.LoanPurposeAC>> GetLoanPurposeListAsync();

        /// <summary>
        /// Method fetches all the UI required configurations.
        /// </summary>
        /// <returns>ConfigurationAC object</returns>
        Task<ConfigurationAC> GetAllConfigurationsAsync();

        /// <summary>
        /// Retrieve list of Industry Group.
        /// </summary>
        /// <returns>Returns list of NAICSIndustryTypeAC object</returns>
        Task<List<NAICSIndustryTypeAC>> GetIndustryGroupListAsync();

        /// <summary>
        /// Get all list of Company size ranges
        /// </summary>
        /// <returns>List of company size object</returns>
        Task<List<CompanySizeAC>> GetCompanySizeListAsync();
        /// <summary>
        /// Get all list of company structure
        /// </summary>
        /// <returns>List of company structure object</returns>
        Task<List<CompanyStructureAC>> GetCompanyStructureListAsync();
        /// <summary>
        /// Get all list of business age ranges
        /// </summary>
        /// <returns>List of business age object</returns>
        Task<List<BusinessAgeAC>> GetBusinessAgeRangeListAsync();
        /// <summary>
        /// Get all list of Industry experience ranges
        /// </summary>
        /// <returns>List of Industry experience object</returns>
        Task<List<IndustryExperienceAC>> GetIndustryExperienceListAsync();
        /// <summary>
        /// Method to fetch the interest rate based on given self declared credit score.
        /// </summary>
        /// <param name="creditScore">Credit score</param>
        /// <returns>Interest rate</returns>
        decimal? GetInterestRateForGivenSelfDeclaredCreditScore(string creditScore);

        /// <summary>
        /// Method is to calculate the loan product detail.
        /// </summary>
        /// <param name="loanProductRangeTypeMappings">List of LoanProductRangeTypeMapping </param>
        /// <param name="currentCurrencySymbol">Currency symbol</param>
        /// <param name="loanApplication">Loan application</param>
        /// <returns>Object of loanProductDetailAC</returns>
        ProductDetailsAC CalculateLoanProductDetail(List<ProductRangeTypeMapping> loanProductRangeTypeMappings, string currentCurrencySymbol, LoanApplication loanApplication);


        /// <summary>
        /// Prepare Audit log object for custom fields.
        /// </summary>
        /// <param name="userAC">User object</param>
        /// <param name="logBlockName">Company or Loan</param>
        /// <param name="id">Contain CompanyId or LoanId</param>
        /// <returns>AuditLog object.</returns>
        AuditLog GetAuditLogForCustomFields(CurrentUserAC userAC, ResourceType logBlockName = ResourceType.Other, Guid? id = null);

        /// <summary>
        /// Fetch the datewise Audit logs of the company or loan application based on the Id.
        /// </summary>
        /// <param name="loggedInUser">LoggedIn user object.</param>
        /// <param name="auditLogFilter">Audit log filter object.</param>
        /// <returns>List of datewise Auditlogs.</returns>
        Task<List<AuditDateWiseLogsAC>> GetAuditLogsByLogBlockNameIdAsync(CurrentUserAC loggedInUser, AuditLogFilterAC auditLogFilter);

        /// <summary>
        /// Fetch the list of fields of the primary key which is in the old and new value.
        /// </summary>
        /// <param name="auditLogField">Field object.</param>
        /// <returns>Retrieve the list of fields of the log.</returns>
        Task<List<AuditLogFieldAC>> GetAuditLogByPkIdAsync(AuditLogFieldAC auditLogField);
        /// <summary>
        /// Method return string name of month with respective to provide number
        /// </summary>
        /// <param name="monthNumber">Number month</param>
        /// <returns></returns>
        string GetMonthNameFromMonthNumber(int monthNumber);

        /// <summary>
        /// Get upload pre signed URL
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        public AwsSettings GetUploadPreSignedURL(string fileName);

        /// <summary>
        /// Method fetches all the additional document types.
        /// </summary>
        /// <returns>List of AdditionalDocumentTypeAC</returns>
        Task<List<AdditionalDocumentTypeAC>> GetAdditionalDocumentTypesAsync();
        #endregion
    }
}
