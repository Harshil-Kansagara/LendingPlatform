using System.Collections.Generic;

namespace LendingPlatform.Utils.Constants
{
    public static class StringConstant
    {
        #region Connection String
        public static readonly string LendingPlatformConnection = "LendingPlatformConnection";
        #endregion

        #region Company Structure
        public static string Proprietorship => "Proprietorship";
        public static string Partnership => "Partnership";
        public static string LimitedLiabilityCompany => "Limited Liability Company (LLC)";
        public static string SCorporation => "S Corporation";
        public static string CCorporation => "C Corporation";
        #endregion

        #region Business Age
        public static string SixMonthToOneYear => "6 Months - 1 Year";
        public static string OneYearToThreeYear => "1 Year - 3 Year";
        public static string ThreeYearToFiveYear => "3 Year - 5 Year";
        public static string MoreThanFiveYear => "More than 5 Year";
        #endregion

        #region Company Size
        public static string ZeroToTen => "0 - 10";
        public static string AboveTen => "Above 10";
        #endregion

        #region Industry Experience
        public static string IndustryExperienceZeroToFiveYears => "0 - 5";
        public static string IndustryExperienceAboveFiveYears => "Above 5";
        #endregion

        #region Relationship
        public static string Partner => "Partner";
        public static string Proprietor => "Proprietor";
        public static string Shareholder => "Shareholder";
        #endregion

        #region API
        public static string SmartyStreets => "SmartyStreets";
        public static string EquifaxAPI => "Equifax";
        public static string ExperianAPI => "Experian";
        public static string TransunionAPI => "Transunion";

        public const string Quickbooks = "Quickbooks";
        public const string Xero = "Xero";
        #endregion

        #region Financial Statement
        public static string IncomeStatement => "Income Statement";
        public static string CompanyFinance => "Company Finances";
        public static string ProfitAndLossStatement => "ProfitAndLoss";
        public static string BalanceSheetStatement => "BalanceSheet";
        public static string BalanceSheet => "Balance Sheet";
        public static string CreditReport => "Credit Report";
        public static string Invoices => "Invoices";
        public static string CashFlow => "Cash Flow";
        public static string FinancialRatios => "Financial Ratios";
        public static string PersonalFinances => "Personal Finances";
        public static List<string> AutocalculatedReports => new List<string> { "Cash Flow", "Financial Ratios" };

        public const string FinancialReportCsv = "Income Statement,Balance Sheet,Cash Flow,Financial Ratios";
        public static string Statement => "Statement";
        public static string SquareBrackets => "[]";

        public const string PersonalFinanceSummary = "summary";
        public const string PersonalFinanceDetails = "details";
        public const string PersonalFinanceDetailsSummary = "details,summary";
        #endregion

        #region Loan Products

        public static string LoanAmount => "Loan Amount";
        public static string Lifecycle => "Lifecycle";
        #endregion

        #region Errors
        public static string AddressAlreadyLinked => "Entity is already linked with address.Please update address";
        public static string DataNotFound => "Data Not Found";
        public static string UnauthorizedResourceAccess => "The resource access request is unauthorized";
        public static string AllTaxFormValidationError => "Please add all the tax forms for each years";
        public static string AtleastOneTaxFormValidationError => "Please add atleast 1 tax form for previous year";
        public static string EmptyLoanPurposeTable => "LoanPurpose table is empty";
        public static string LoanApplicationNotExistsForGivenId => "No any loan application exists for given id";
        public static string BankCodeNotExistsInConfiguration => "Bank code is not found in configuration";
        public static string NotUniqueSSN => "SSN {0} is not unique";
        public static string InvalidCIN => "{0} CIN of {1} has invalid format";
        public static string InvalidSharePercentageRange => "Share percentage should be within range of 0 to 100";
        public static string NotUniqueEIN => "{0} is not unique";
        public static string NotUniquePhoneNumber => "{0} is not unique phone number";
        public static string InvalidSICIndustryCode => "Invalid code of sic industry type";
        public static string NoAddressForEntityId => "Address for supplied entity id not found";
        public static string EmptyConsentTable => "Consent table is empty";
        public static string AddOperationNotAllowed => "Add operation is not allowed";
        public static string UpdateOperationNotAllowed => "Update operation is not allowed";
        public static string SectionNameIsNull => "Section name is null";
        public static string LoanSectionNameError => "Loan doesn't belong to current section";
        public static string UpdateSectionNameError => "Cannot update section with provided section name";

        // Used to display error message while clicking on cancel button at the allow permission page in the xero.
        public static string LoanApplicationProductNotFound => "Loan product not found as per the loan application provided";
        public static string LoanApplicationOrProductNotFound => "Loan applicaiton or loan product not found while saving";
        public static string UnexpectedStatusCodePremierProfilesExperian => "Unexpected response code from experian premier profiles API";
        public static string UnexpectedStatusCodeConsumerCreditReportEquifax => "Unexpected response code from equifax consumer credit report API";
        public static string UnexpectedStatusCodeProvidersYodlee => "Unexpected response code from providers yodlee API";
        public static string CINRegexUSA => "^[0-9]{9}$";
        public static string InvalidAddress => "Address is invalid";
        public static string MoreThanOneShareHolderRequired => "Company with company type registered entity or partnership should have more than one shareholders";
        public static string UniquePhoneErrorMessage => "Phone number {0} is not unique";
        public static string EmailSendingException => "Exception in sending an email";
        public static string InvalidSelectedBureau => "Invalid selected bureau in Bank";
        public static string PayPalApiRequestError => "Error occured in PayPal api call";
        public static string InvalidTransunionRequest => "Invalid transunion Request = ";
        public static string InvalidResponse => "Response = ";
        public static string SectionsNotFound => "No sections are found in database.";
        public static string BanksNotFound => "No banks found in database.";
        public static string NoSnapShotFoundForLoanApplication => "No snapshot of details found for given loan application.";
        public static string NoSnapShotFoundForDocument => "No snapshot of details found for given document.";
        public const string InvalidZipCode = "Zip code invalid";
        public const string CharacterNotAllowed = "Characters are not allowed.";
        public const string NotValidPhone = "Not Valid Phone.";
        public static string InvalidEntityType => "Entity type is invalid";

        public const string SmartyStreetsConfigurationNotFound = "SmartyStreets Configuration Not Found";
        public const string NoUpdateAccess = "Logged In user is not allowed to do update operation";
        public static string ProprietorOnlyOneUser => "Proprietorship company structure cannot have more than one linked user with 100 percent";
        public static string InvalidCompanyStructure => "Invalid company structure";
        public static string MajorityShareholderAlreadyExist => "Majority Shareholder already added can not add more";
        public static string ShareholderAlreadyExist => "The Shareholder trying to add is already linked";
        public static string NoAccessToLinkLoan => "No access to link application to entity";
        public static string InvalidEntityRelationshipMappingId => "Invalid unique identifier for entity relationship mapping object";
        public static string SharePercentageNotHundred => "Total share percentage not 100";
        public static string NotMajoritySharePercentage => "Only one user is linked and not have majority share percentage";
        public static string NotAllowedToStartNewLoan => "Not allowed to link provided entity to new loan";
        public static string InvalidCompanyRegisteredState => "Invalid Company Registered State";
        public static string InvalidAccessEntity => "Not have access to demanded entity";
        public static string InvalidCompanyFiscalYearStartMonth => "Invalid Company fiscal year start month";
        public static string NoDocumentFound => "No document found per year";
        public static string DocumentPeriodEmpty => "Add tax year for each uploaded document";
        public static string NotAllowedToGiveConsent => "User is not allowed yet to give consent";
        public static string NoEntityLoanApplicationMappingFound => "No any mapping found between loan and entity";
        public static string PleaseFillTheseSectionsToGiveConsent => "Please fill up these sections to give consent: ";
        public static string FinancesSectionNotReadyYet => "Finances section is not ready yet";
        public static string AllLinkedEntitiesProvideConsent => "Every linked entity(user) of primary entity(company) should have provided consent in order to fetch credit report";
        public static string UserHasNotProvidedConsent => "User has to provide his consent for a loan in order to fetch his credit report";
        public static string NoAccessToEntity => "No Access to Entity";
        public static string LoanNotLinkedWithEntity => "Loan not linked with entity";
        public static string CurrentUserNotLinkedWithEntity => "Current User not part of entity";
        public static string NoRelativesFoundOrNoAnyRelativesConsentFound => "No relatives found or no any relative's consent found";
        public static string NotAllLinkedEntitiesHaveGivenConsent => "Not all the entities have given the consent";
        public static string InvalidIdProvided => "Invalid id provided in request";
        public static string InterestRateNotFoundInConfigurations => "Interest rate is not foung in configurations";
        public static string LoanInitiatorOrSelfDeclaredCreditScoreNotFound => "Loan initiator or its self declared credit score is not found";
        public static string InvalidDataProvidedInRequest => "Invalid data provided in request";
        public static string InvalidLoanPurposeProvidedInRequest => "Invalid loan purpose is provided in request";
        public static string MissingScopesForPersonalFinancesData => "Scopes for personal finance data are not provided";

        public const string UnhandledExceptionResponseContentType = "text/plain";
        public static string CompanyNotExist => "Company with supplied company id does not exist";
        public const string InvalidMonthNumber = "Invalid Month Number";
        public static string AdditionalDocumentTypesNotFound => "No additional document types are found";
        #endregion

        #region Role
        public const string ClientRole = "Client";
        public const string BankUserRole = "[BankUser]";
        public const string UserAllRoles = "[Client],[BankUser]";
        public const string RoleClaimName = "UserRole";
        #endregion

        #region Claims
        public static string PhoneNumber => "Phone";
        #endregion

        #region Section names // Added to use anywhere if needed.
        public static string LoanProductSection => "Product Selection";
        public static string FinancesSection => "Financial Information";
        public static string PersonalFinancesSection => "Personal";
        public static string TaxReturns => "Tax Returns";
        public static string LoanConsentSection => "Review and Consent";
        public static string LoanStatusSection => "Application Status";
        public static string AdditionalDocuments => "Additional Documents";
        #endregion

        #region Files
        public static string FileTemp => "temp_/";
        public static string FileExtensionNotFound => "Supported file extensions is PDF";
        public static string FileSizeExceeded => "Please upload file with size less than 20 MB";
        #endregion

        #region HttpHeader
        public const string HttpHeaderAcceptJsonType = "application/json";
        public static string HttpHeaderAcceptXmlType => "application/xml";
        public static string HttpHeaderFormUrlEncoded => "application/x-www-form-urlencoded";
        public const string HttpHeaderAcceptPdfType = "application/pdf";
        public const string HttpHeaderAcceptDefaultType = "application/octet-stream";
        public static string HttpHeaderAccept => "Accept";
        public static string HttpHeaderGrantType => "Grant_type";
        public static string HttpHeaderPassword => "password";
        public static string HttpHeaderAuthorization => "Authorization";
        public static string HttpHeaderAuthorizationToken => "Bearer {0}";
        public static string HttpHeaderAuthorizationBasicAuth => "Basic {0}";
        public static string HttpHeaderClientReferenceId => "clientReferenceId";
        public static string HttpHeaderSBMYSQL => "SBMYSQL";
        public static string HttpHeaderClientCredentials => "client_credentials";
        public static string Base64Authorization => "{0}:{1}";
        public static string HttpGrantTypeAuthorizationCode => "authorization_code";
        public static string HttpRequestUrlParameterValue => "{0}={1}";
        public static string HttpHeaderGzip => "gzip";
        public static string HttpHeaderCacheControlMaxAgeOneYear => "max-age-31536000";
        public static string HttpHeaderContentDispositionAttachment => "attachment";
        #endregion

        #region Yodlee
        public static string ApiVersion => "Api-Version";
        public static string LoginName => "loginName";
        public static string AuthUrl => "auth/token";
        public static string RegisterUrl => "user/register";
        public static string ProvidersUrl => "providers/";
        public static string AccountsUrl => "accounts?providerAccountId=";
        public static string TransactionsUrl => "transactions";
        #endregion

        #region HttpMethod
        public static string HttpMethodPost => "POST";
        public static string HttpMethodGet => "GET";
        #endregion

        #region Email templates
        public static Dictionary<int, List<string>> EmailTemplates => new Dictionary<int, List<string>>()
        {
            { 1, new List<string> { "Request To Provide Consent", "RequestToProvideConsent.html" } },
            { 2, new List<string> { "Reminder To Provide Consent", "ReminderToProvideConsent.html" } },
            { 3, new List<string> { "Loan Application Approved", "LoanApplicationApproved.html" } },
            { 4, new List<string> { "Loan Application Rejected", "LoanApplicationRejected.html" } }
        };
        #endregion

        #region Email templates' fields
        public static string TodaysDate => "todaysDate";
        public static string LenderInstituteName => "lenderInstituteName";
        public static string CustomerName => "customerName";
        public static string ApplicationNumber => "applicationNumber";
        public static string CompanyName => "companyName";
        public static string Relation => "relation";
        public static string RedirectUrl => "redirectUrl";
        #endregion

        #region Email view loan redirect url
        public static string LoanApplicationRedirectUrl => "https://customer-dev.jamoon.net/";
        #endregion

        #region Email templates' folder name
        public static string EmailTemplatesFolderName => "EmailTemplates";
        #endregion

        #region Automotive credit report

        public static string Demographics => "Geocode and Phone";
        public static string ModelIndicatorAE => "AE";
        public static string ModelIndicatorF3 => "F3";
        public static string ModelIndicatorFM => "FM";
        public static string ModelIndicatorW => "W";
        public static string ModelIndicatorQ => "Q";
        public static string FraudShield => "Y";

        #endregion

        #region PayPal api request parameters
        public static string PayPalInvoiceTotalRequired => "total_required";
        public static string PayPalInvoicePage => "page";
        public static string PayPalInvoicePageSize => "page_size";
        #endregion

        #region Patterns
        public static string DatePattern => "MMddyyyy";
        public static string DatePatternWithDash => "yyyy-MM-dd";
        #endregion

        #region Square API
        public static string SquareInvoiceSortDate => "INVOICE_SORT_DATE";
        #endregion

        #region Order
        public static string DescendingOrderShorthand => "DESC";
        public static string AscendingOrderShorthand => "ASC";
        #endregion

        #region Transunion
        public static string TransunionProductCode => "07000";
        public static string TransunionSubjectNumber => "1";
        public static string TransunionAddressStatus => "current";
        public static string TransunionInquiryECOADesignator => "individual";
        public static string TransunionRootNodeName => "creditBureau";
        public static string TransunionFileHitIndicatorNodeName => "fileHitIndicator";
        public static string TransunionScoreResultsNodeName => "results";
        public static string TransunionRegularNoHitInfo => "regularNoHit";
        public static string TransunionNamespace => "http://www.transunion.com/namespace";
        public static string TransunionDocument => "request";
        public static string TransunionPublicRecordNodeName => "publicRecord";
        #endregion

        #region DynamicLinq field parameter
        public static string RequestParameterBasicDeatail => "basic-detail";
        public static Dictionary<string, string> RequestParameterDBFieldName => new Dictionary<string, string>()
        {
            { "finances",  "EntityFinances"},
            { "transactions",  "Transactions" },
            { "credit-report",  "CreditReport" },
            { "consents",  "Consents" },
            { "selected-product", "Products" },
            { "bank-details","BankDetails"}
        };

        #endregion

        #region RegularExpression
        public const string ZipCodeRegularExpression = "^[0-9]{5}(-[0-9]{5}){0,1}$";
        public const string SSNRegularExpression = @"^[0-9]*$";
        public const string PhoneRegularExpression = @"^[+]([1-9]){1}[0-9]{6,14}$";
        #endregion

        #region Entity Type
        public static string CompanyEntityType => "Company";
        public static string PeopleEntityType => "People";
        public static string BankerEntityType => "Banker";
        #endregion

        #region Filter
        public static string FilterType => "type";
        public static string FilterEqualOperator => "=";
        public static string FilterId => "Id";

        #region Bank end loan list grid fields
        public const string FilterApplicationNumber = "applicationnumber";
        public const string FilterCreatedDate = "date";
        public const string FilterCompanyName = "companyname";
        public const string FilterLoanAmount = "loanamount";
        public const string FilterLoanPurpose = "loanpurpose";
        public const string FilterLoanStatus = "status";
        #endregion

        #endregion

        #region Allowed file types of additional documents
        public static string Pdf => "pdf";
        public static string Xls => "xls";
        public static string Xlsx => "xlsx";
        public static string Png => "png";
        public static string Gif => "gif";
        public static string Jpeg => "jpeg";
        public static string Csv => "csv";
        public static string Docx => "docx";
        #endregion

        public const string DocumentExtractedValueSnapshot = "DocumentExtractedValueSnapshot";
        public static string Current => "current";
        public static string Local => "local";

        public const string BankBearer = "bankbearer";
        public const string DefaultDateFormat = "dd/MM/yyyy";
        public const string dashedDateFormat = "dd-MM-yyyy";

        public const string MissingStatementsInParameter = "Statements are missing in parameter";
        public const string MissingParameters = "Missing Parameters";
        public const string InsufficientEvaluationData = "Insufficient Evaluation Data";

        #region Self declared credit scores' values
        public const string ExcellentCreditScore = ">750";
        public const string GoodCreditScore = "701-750";
        public const string FairCreditScore = "651-700";
        public const string AverageCreditScore = "561-650";
        public const string PoorCreditScore = "<=560";
        public const string notKnown = "not known";
        #endregion
        #region Audit log
        public const string AuditLogUpdateActionName = "Update";
        public const string AuditLogInsertActionName = "Insert";
        public const string AuditLogDeleteActionName = "Delete";
        public const string USCountryCode = "+1";
        public const string AttributeId = "AttributeId";
        public const string SubLoanPurposeId = "SubLoanPurposeId";
        public const string LoanPurposeId = "LoanPurposeId";

        public static Dictionary<string, List<string>> AuditLogNecessaryTableFields => new Dictionary<string, List<string>>()
        {
            { "LoanPurpose", new List<string>(){ "Name" } },
            { "Section", new List<string>(){ "Name" } },
            { "BusinessAge", new List<string>(){ "Age" } },
            { "CompanyStructure", new List<string>(){ "Structure" } },
            { "IndustryExprience", new List<string>(){ "Experience" } },
            { "NAICSIndustryType", new List<string>(){ "IndustryType" } },
            { "Product", new List<string>(){ "Name", "Description" } },
            { "CompanySize", new List<string>(){ "Size" } },
            { "Consent", new List<string>(){ "ConsentText" } },
            { "IndustryExperience", new List<string>(){ "Experience" } },
            { "Bank", new List<string>(){ "Name", "SWIFTCode" } },
            { "IntegratedServiceConfiguration", new List<string>(){ "Name" } },
            { "FinancialStatement", new List<string>(){ "Name" } },
            { "User", new List<string>(){ "FirstName", "MiddleName", "LastName", "Email", "SSN", "Phone", "DOB", "ResidencyStatus" } },
            { "Address", new List<string>(){ "StreetLine", "StateAbbreviation", "City", "ZipCode" } },
            { "Company", new List<string>(){ "Name", "CIN", "CompanyStructureId" } },
            { "EntityLoanApplicationConsent", new List<string>(){ "ConsentId" } },
            { "PersonalFinanceResponse", new List<string>(){ "Answer", "PersonalFinanceAttributeCategoryMappingId" } },
            { "PersonalFinanceAttributeCategoryMapping", new List<string>(){ "AttributeId" } },
            { "PersonalFinanceAttribute", new List<string>(){ "Text" } },
            { "AdditionalDocumentType", new List<string>(){ "Type" } },
            { "EntityAdditionalDocument", new List<string>(){ "AdditionalDocumentTypeId" } }
        };
        #endregion
        public const string GuidValidationPatternRegex = @"^[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}$";
        public const string AuditLogCustomFieldName = "AuditLog";
        public const string IpAddressHeader = "X-Forwarded-For";

        #region Months
        public const string January = "January";
        public const string February = "February";
        public const string March = "March";
        public const string April = "April";
        public const string May = "May";
        public const string June = "June";
        public const string July = "July";
        public const string August = "August";
        public const string September = "September";
        public const string October = "October";
        public const string November = "November";
        public const string December = "December";
        #endregion

        #region Sub loan purpose
        public static string ConstructiveEquipment => "Constructive Equipment";
        public static string ConstructionEquipment => "Construction Equipment";
        #endregion

        public const string Version = "Version";
    }
}
