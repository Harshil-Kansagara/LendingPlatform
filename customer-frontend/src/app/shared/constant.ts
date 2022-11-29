export const Constant = {

  currency: 'USD',
  guidEmptyString: '00000000-0000-0000-0000-000000000000',
  //Caption for sidebar banner
  paymentCaption: `The monthly payment calculator helps you figure out your installments to give you the confidence to borrow and to help you plan your repayments with ease.`,
  //Redirect Url
  homeRedirectUrl: '/home',
  calculatorRedirectUrl: '/calculator',
  creditProfileRedirectUrl: '/profile',
  creditProfileSectionRedirectUrl: '/loan/profile',
  loanNeedsRedirectUrl: '/loan/needs',
  companyInfoRedirectUrl: '/loan/company/info',
  financesRedirectUrl: '/loan/company/finances/statements',
  invoicesRedirectUrl: '/loan/company/finances/invoices',
  transactionsRedirectUrl: '/loan/company/finances/transactions',
  taxesReturnsRedirectUrl: '/loan/company/finances/taxes',
  loanProductRedirectUrl: '/loan/products',
  loanProductDetailsRedirectUrl: '/loan/products/detail',
  loanConsentRedirectUrl: '/loan/consent',
  loanStatusRedirectUrl: '/loan/status',
  bankDetailsRedirectUrl: '/loan/bank-details',
  additionalDocumentsRedirectUrl: '/loan/additional-documents',
  loginRedirectUrl: '/login',
  routeRootUrl: '/',
  personalFinancesRedirectUrl: '/loan/personal/finances',

  //Components Title
  calculatorTitle: 'Monthly payment calculator',
  creditProfileTitle: 'Credit profile information',
  companyInfoTitle: `Please provide us with your company details!`,
  financialInfoTitle: `Let us add your company's financial information!`,
  taxesInfoTitle: `Let us add your company's tax information!`,
  productTitle: 'Based on your information, we recommend the below products',
  loanConsentTitle: 'Review and Consent',
  bankDetailsTitle: 'Give your bank information',
  //Section names
  loanNeeds: 'Your Needs',
  companyInfo: 'Company Information',
  financialInfo: 'Financial Information',
  finances: 'Financial Information',
  invoices: 'Invoices',
  transactions: 'Transactions',
  taxes: 'Tax Returns',
  loanProduct: 'Product Selection',
  productSelection: 'Product Selection',
  loanConsent: 'Review and Consent',
  loanStatus: 'Application Status',
  bankDetails: 'Bank Details',
  additionalDocuments: 'Additional Documents',
  company: 'Company',
  personal: 'Personal',

  //Coming Soon
  comingSoonTitile: 'Coming Soon',

  //Upload file error
  uploadAllYearFile: 'Upload files of all listed year',
  fileExtensionNotFound: 'Supported file extensions is PDF only',
  fileNotSupportedInAdditionalDocuments: 'Supported file types are : PDF/XLS/PNG/JPEG/CSV/DOCX',
  fileExceeded: 'Uploaded files exceeded. Total number of files allowed are ',
  fileSizeExceeded: 'Please upload file with size less than 20 MB',


  //Progress Bar
  loanNeedsProgressBar: 0,
  loanProductProgressBar: 20,
  companyInfoProgressBar: 40,
  financesProgressBar: 60,
  taxesProgressBar: 80,
  loanConsentProgressBar: 90,
  additionalDocumentsProgressBar: 80,
  loanStatusProgressBar: 100,

  //Company Structure
  companyStructureQuestion: 'What is your company structure?',
  proprietorship: 'Proprietorship',
  partnership: 'Partnership',
  limitedLiabilityCompany: 'Limited Liability Company (LLC)',
  sCorporation: 'S Corporation',
  cCorporation: 'C Corporation',

  //Pattern
  onlyNumberPattern: '([0-9])+',
  phoneNumberPattern: '[+]([1-9]){1}[0-9]{6,14}',
  onlyTenDigitPattern: '([0-9]){10}',
  onlyIntegerOrDecimalPattern: '[0-9]+([,.][0-9]+)?',
  dobPattern: '([1-9]{1,2})-([1-9]{1,2})-([1-9]{4})',
  zipCodePattern: '[0-9]{5}(-[0-9]{5}){0,1}',

  blur: 'blur',
  change: 'change',
  // Format
  dateFormat: 'dd - MM - yyyy',

  //Landing Page
  processSummary: [
    { iconPath: 'assets/images/quick-icon.svg', name: 'Simple, Quick and Easy Application Process' },
    { iconPath: 'assets/images/customize-icon.svg', name: 'Customized Funding Options to Fit Your Business Needs' }
  ],

  //OptionType of form fields
  textbox: 'textbox',
  searchbar: 'searchbar',
  typeahead: 'typeahead',
  radiobtn: 'radiobtn',

  //OptionName of form fields
  personalSSN: 'personalSSN',
  companyEIN: 'companyEIN',
  companyType: 'companyType',
  companyName: 'companyName',
  streetLine: 'streetLine',
  city: 'city',
  stateAbbreviation: 'stateAbbreviation',
  industryType: 'industryType',
  businessPeriod: 'businessPeriod',
  peopleCapacity: 'peopleCapacity',
  zipCode: 'zipCode',

  // Placeholder of company info
  personalSSNPlaceHolder: 'Personal SSN',
  companyEINPlaceHolder: 'Company EIN',
  companyNamePlaceHolder: 'Company Name',

  //Address fields' placeholders
  streetLinePlaceHolder: 'Search street line here...',
  cityPlaceHolder: 'City',
  statePlaceHolder: 'State',

  //Common error messages
  someThingWentWrong: 'Something went wrong',
  bankDataSavedSuccessfully: 'Bank details saved successfully',
  detailsSavedSuccessfully:'Details saved successfully',

  //Landing page loan steps
  request: 'Request',
  approval: 'Loan Decision',
  money: 'Money',
  //Landing page static info points
  requestInfo: 'Fast and easy to apply',
  approvalInfo: 'Instant decision in principle',
  moneyInfo: 'Quick access to funds',


  //Self as a company name in dropdown
  self: 'Self',

  //Loan application section's status
  approved: 'approved',
  pending: 'pending',
  processing: 'processing',

  //Loan needs questions
  howMuchMoneyDoYouNeed: 'How much money do you need?',
  timeRequiredToClear: 'Time required to clear debt',
  whatIsItFor: 'What is it for?',
  howLongDoYouNeedTheMoneyFor: 'What is your desired repayment term?',
  monthlyRepayment: 'This will be your monthly repayment.',
  monthlyPayment: 'Monthly payment',
  monthlyPaymentBasedOnCreditProfile: 'Monthly payment based on credit profile',
  whatTypeOfProperty: 'What type of property do you want to purchase?',
  whatTypeOfAssets: 'What type of asset do you want to purchase?',
  whatDoYouIntend: 'What do you intend use the funds for?',

  //Calculator questions
  whatIsThePrimaryPurpose: 'What is the primary purpose of these funds?',
  interestDisclaimer: `The interest rate quoted here is based on your self-assessed credit rating. 
    The final interest rate will depend on your financial and credit profile once you have submitted your completed application.`,
  interestDisclaimerWhereInterestNotQuoted: `The interest rate ({0}%) used to calculate your payment is based on your self-assessed credit rating.
    The final interest rate will depend on your financial and credit profile once you have submitted your completed application.`,

  //We are sorry
  failedRequestTitle: 'We are sorry, we will not be able to fulfill your request at this time. Someone from our support team will contact you within the next 24 hours.',

  //Loan needs form control names
  amountNeeded: 'amountNeeded',
  amountPeriod: 'amountPeriod',
  purpose: 'purpose',
  subLoanPurpose: 'subLoanPurpose',

  //Local forage keys
  currentProgress: 'currentProgress',
  currentSectionName: 'currentSectionName',
  currentLoanApplicationId: 'currentLoanApplicationId',
  currentLoanApplicationNumber: 'currentLoanApplicationNumber',
  currentCompanyDetails: 'currentCompanyDetails',
  currentCompanyId: 'currentCompanyId',
  currentUserDetails: 'currentUserDetails',
  isViewOnlyMode: 'isViewOnlyMode',
  currentLoanProduct: 'currentLoanProduct',
  currentLoanBankDetails: 'currentLoanBankDetails',
  currentLoanStatus: 'currentLoanStatus',
  sectionConfigurations: 'sectionConfigurations',
  appSettings: 'appSettings',
  thirdPartyServiceConfigurations: 'thirdPartyServiceConfigurations',
  loanNeedsValues: 'loanNeedsValues',
  isLinked: 'isLinked',
  isCreditOkay: 'isCreditOkay',
  lockedApplicationJson: 'lockedApplicationJson',

  // Financial Statement
  incomeStatement: 'Income Statement',
  balanceSheet: 'Balance Sheet',
  cashFlow: 'Cash Flow',
  financialRatio: 'Financial Ratios',
  keyRatios: 'Key Ratios',

  accountFirmHeading: 'We have a partnership with these accounting firms',
  availableSummaryHeading: 'Your financial summary',
  retrieveHeading: `Thank you for providing us access to your accounting package!`,
  retrieveInfo: `We are retrieving your financial information, which may take a few minutes.
You can continue with your application process and we will notify you when your financial information is ready for your review.`,
  // Invoices type
  paypal: 'PayPal',
  stripe: 'Stripe',
  square: 'Square',

  // Transactions Type
  yodlee: 'yodlee',
  plaid: 'plaid',

  // Http Error
  badRequest: 400,
  unauthorized: 401,
  notFound: 404,
  timeout: 408,
  conflict: 409,
  internalServerError: 500,
  forbidden: 403,
  noContent: 204,

  // Validation Messages
  //documentTaxFormNameEmpty: 'Add tax form name for each uploaded document',
  documentPeriodEmpty: 'Add tax year for each uploaded document',
  duplicateTaxReturnExists: 'Duplicate tax return exists for same tax year in uploaded document',
  tokenExpired: 'Session expired. Please re-login to continue',
  timedOut: 'Request timed out',
  serverError: 'Something went wrong, Please try again',
  forbiddenServerRefused: 'Request forbidden',
  noRecordFound: 'No records found.',
  noFinanceRecordFoundWithSuggestion: 'No records found. Please check the company or you may use the manual for add the finance data.',
  requiredField: 'Please enter required field.',
  invalidEIN: 'Please enter valid EIN.',
  invalidSSN: 'Please enter valid SSN.',
  invalidzipCode: 'Please enter valid Zip Code.',
  invalidEmail: 'Please enter valid email.',
  invalidPhone: 'Please enter valid phone number.',
  duplicateEmail: 'Duplicate email addresses',
  duplicatePhone: 'Duplicate phone numbers',
  requiredTitle: 'Please enter title.',
  noInvoicesAreSavedPleaseSave: 'No invoices found. Please select again any one of these services to fetch and save the invoices',
  noTransactionsAreSavedPleaseSave: 'No bank transactions are saved. Please save the bank transactions',
  onlyApprovedLoanApplicationsAllowed: 'Bank details can be added for approved Loan Applications only',
  singleLowPercentage: 'If only one partner is added then sholud have higher percentage',
  enterPhoneNumber: 'Please provide phone number',
  enterSSN: 'Please provide personal SSN',
  enterDob: 'Please provide DOB',
  addSharePercentageMessage: 'Please add share percentage',
  invalidTotalPercentage: 'Total share percentage should be 100',
  invalidPartnershipSingleUserPercentage: 'With partnership company structure, single partner can not own 100 percent share',
  zeroSharePercentageError: 'Share percentage can not be zero',
  allTaxFormValidationError: 'Please add all the tax forms for each years',
  atleastOneTaxFormValidationError: 'Please add atleast 1 tax form for previous year',
  notKnown: 'not known',
  documentTypeNotSelected: 'Select type for each uploaded document',

  loanRouteSectionMapping: [
    {
      Route: '/loan/profile',
      Section: 'Credit Profile',
      IsChild: false
    },
    {
      Route: '/loan/needs',
      Section: 'Your Needs',
      IsChild: false
    },
    {
      Route: '/loan/products',
      Section: 'Product Selection',
      IsChild: false
    },
    {
      Route: '/loan/products/detail',
      Section: 'Product Selection',
      IsChild: false
    },
    {
      Route: '/loan/company/info',
      Section: 'Company Information',
      IsChild: false
    },
    {
      Route: '/loan/company/finances/statements',
      Section: 'Financial Information',
      IsChild: false
    },
    {
      Route: '/loan/company/finances/invoices',
      Section: 'Invoices',
      IsChild: false
    },
    {
      Route: '/loan/company/finances/transactions',
      Section: 'Transactions',
      IsChild: false
    },
    {
      Route: '/loan/company/finances/taxes',
      Section: 'Tax Returns',
      IsChild: false
    },
    {
      Route: '/loan/consent',
      Section: 'Review and Consent',
      IsChild: false
    },
    {
      Route: '/loan/status',
      Section: 'Application Status',
      IsChild: false
    },
    {
      Route: '/loan/bank-details',
      Section: 'Bank Details',
      IsChild: false
    },
    {
      Route: '/loan/additional-documents',
      Section: 'Additional Documents',
      IsChild: false
    },
    {
      Route: '/loan/company/finances/statements',
      Section: 'Company',
      IsChild: true
    },
    {
      Route: '/loan/personal/finances',
      Section: 'Personal',
      IsChild: true
    },
  ],

  //Landing Page
  lendingFeatures: [
    {
      title: 'Flexibility',
      imageUrl: 'assets/images/flexibility.png',
      description: `Digital tools for borrowing and on-going account management`,
      cssClass: 'flexibility'
    },
    {
      title: 'Instant',
      imageUrl: 'assets/images/instant.png',
      description: `Simple, interactive online application`,
      cssClass: 'instant'
    },
    {
      title: 'Control & confidence',
      imageUrl: 'assets/images/control.png',
      description: `Affordability calculators and intelligent exchange of information for most suitable solutions`,
      cssClass: 'control'
    },
    {
      title: 'Transparency & Value',
      imageUrl: 'assets/images/transparency.png',
      description: `Competitive and fair offers, with no hidden costs`,
      cssClass: 'transparency'
    }
  ],

  // Loan product
  interestRate: ['Fixed', 'Variable', 'AnnualPercentage'],
  noApplicableFee: 'No Fee Applicable',

  // Tax table validation error
  taxValidationError: 'Please fill details for at least 1 year.',
  uploadingInProgressLeaveAnyway: `Uploading file already in progress, leave anyway?`,
  taxFileUploadInProgress: 'Tax file upload is in progress',

  //Additional document validation error
  additionalDocumentUploadInProgress: 'Additional document upload is in progress',

  //Content to show in consent modal while files are being uploaded. 
  fileUploadingHeading: 'File Upload In Progress',
  fileUploadingInfo: 'We are uploading your files, which may take a few minutes. We will notify you when the files are ready for your review.',
  
  //Loan consent validation error
  loanConsentValidationError: 'Please provide all the consents',
  fillAllRequiredFieldsError: 'Please fill up all the required fields',
  fillAtLeastOneInformation:'Please fill at least one information',

  //Authguard error message
  fillPreviousSteps: 'Fill previous steps',

  //Upload file title
  uploadTaxStatement: 'Upload Taxes Documents',
  uploadIncomeStatement: 'Upload Income Statement Documents',
  uploadBalancesheetStatement: 'Upload Balance Sheet Documents',

  //Loan consent user details questions
  yourFullName: 'Your Full Name Is',
  whatsYourSSN: 'What’s Your SSN?',
  whatsYourDateOfBirth: 'What’s Your Date of Birth?',
  yourEmailId: 'Your Email ID',
  yourPhoneNumber: 'Your Phone Number',
  whereDoYouLive: 'Where Do You Live?',

  //Loan consent form fields
  firstName: 'firstName',
  lastName: 'lastName',
  ssn: 'ssn',
  dob: 'dob',
  email: 'email',
  phone: 'phone',
  residencyStatus: 'residencyStatus',

  //Placeholders of loan consent user details fields
  namePlaceHolder: 'Name',
  ssnPlaceHolder: 'xxx-xx-xxxx',
  dobPlaceHolder: 'mm-dd-yyyy',
  emailPlaceHolder: 'abc@xyz.com',
  phonePlaceHolder: '+1(xxx)xxx-xxxx',

  //PayPal currency codes' symbols
  inr: '₹',
  usd: '$',
  aud: 'AU$',
  gbp: '£',

  //PayPal popup related URLs.
  localHostIpAddressUrl: '127.0.0.1:4200',
  payPalPopUpCloseLocalHostRedirectUrl: 'http://localhost:4200/loan/company/paypalredirect?code=',
  payPalStateParameterInRedirectUrl: '&state=',

  // Header component

  jamoonLogoImagePath: 'assets/images/jamoon_logo.png',

  //Credit score ranges
  poor: '<=560',
  average: '561-650',
  fair: '651-700',
  good: '701-750',
  excellent: '>750',

  //appsettings sections
  mojorityPercentage: 'Entity:MajoritySharePercentage',
  selectBank: 'Select Bank',

  //Months list
  months: [
    { name: 'January', number: 1, days: 31 },
    { name: 'February', number: 2, days: 28 },
    { name: 'March', number: 3, days: 31 },
    { name: 'April', number: 4, days: 30 },
    { name: 'May', number: 5, days: 31 },
    { name: 'June', number: 6, days: 30 },
    { name: 'July', number: 7, days: 31 },
    { name: 'August', number: 8, days: 31 },
    { name: 'September', number: 9, days: 30 },
    { name: 'October', number: 10, days: 31 },
    { name: 'November', number: 11, days: 30 },
    { name: 'December', number: 12, days: 31 }
  ],

  //Company Structure
  companyStructure: [
    { name: 'companystructure', optionName: 'Proprietorship', title: 'Proprietorship', imageUrl: 'assets/images/proprietorship.svg' },
    { name: 'companystructure', optionName: 'Partnership', title: 'Partnership', imageUrl: 'assets/images/partnership.svg' },
    { name: 'companystructure', optionName: 'LLC', title: 'Limited Liability Company (LLC)', imageUrl: 'assets/images/llc.svg' },
    { name: 'companystructure', optionName: 'sCorporation', title: 'S Corporation', imageUrl: 'assets/images/s_corporation.svg' },
    { name: 'companystructure', optionName: 'cCorporation', title: 'C Corporation', imageUrl: 'assets/images/c_corporation.svg' }
  ],

  //Company all questions
  companyAllQuestions: [

    {
      question1: `What is your social security number?`,
      question2: `What is your partnership's EIN?`,
      question3: `What is your company's EIN?`,
      optionType: 'ein',
      formControl: 'companyEIN'
    },
    {
      question1: `What is your business name?`,
      question2: `What is your partnership's name?`,
      question3: `What is your company's name?`,
      optionType: 'textbox',
      formControl: 'name',
      placeholder: 'Business Name',
      placeholder2: 'Partnership Name',
      placeholder3: 'Company Name'
    },
    {
      question1: `In which state is your partnership registered?`,
      question2: `In which state is your company registered?`,
      registeration: true,
      formControl: 'registeredState'
    },
    {
      question1: `What is the mailing address of your business?`,
      question2: `What is the mailing address of your partnership?`,
      question3: `What is the mailing address of your company?`,
      gridColumn: 'full',
      addressFields: [
        { optionType: 'typeahead' },
        { optionType: 'textbox', placeholder: 'City', formControl: 'city' },
        { optionType: 'textbox', placeholder: 'State', formControl: 'stateAbbreviation' },
        { optionType: 'textbox', placeholder: 'Zip Code', formControl: 'zipCode' }
      ],
      note: `We capture industry sector and group information based on North American Industry Classification System (NAICS):`
    },
    {
      question1: `What kind of industry do you operate in?`,
      displayOption: 'all',
      optionType: 'selectWithSearchIndustryGroup',
      formControl: 'industryGroup'
    },
    {
      question1: `How long has this business been in operation?`,
      question2: `How long has this partnership been in operation?`,
      question3: `How long has this company been in operation?`,
      displayOption: 'all',
      optionType: 'businessSelectList',
      gridColumn: 'full',
      formControl: 'businessAge'
    },
    {
      question1: `How many employees do you have at your business?`,
      question2: `How many employees do you have at your partnership?`,
      question3: `How many employees do you have at your company?`,
      displayOption: 'all',
      optionType: 'companySelectList',
      gridColumn: 'full',
      formControl: 'companySize'
    },
    {
      question1: `How long have you been working in this industry?`,
      question2: `How long have you been working in this industry?`,
      question3: `How long have you been working in this industry?`,
      displayOption: 'all',
      optionType: 'industryExperienceSelectList',
      gridColumn: 'full',
      formControl: 'industryExperience'
    }
  ],

  //Company IsFiscalYear questions
  companyIsFiscalYear: [
    {
      formControl: 'fiscalYear',
      question: `Is the company's fiscal year the same as calendar year?`,
      optionType: 'radioButton'
    },
    {
      formControl: 'fiscalMonth',
      question: `Start Month`,
      optionType: 'selectlist'
    }
  ],

  // Residency Status
  usPermanentResident: 'Permanent Resident',
  usCitizen: 'US Citizen',
  greenCardHolder: 'Green Card Holder',
  nonResident: 'Non Resident',

  //Option Name Company Structure
  proprietorshipOptionName: 'Proprietorship',
  partnershipOptionName: 'Partnership',
  llcOptionName: 'LLC',
  sCorporationOptionName: 'sCorporation',
  cCorporationOptionName: 'cCorporation',

  //Image Url of company structure
  proprietorshipImagePath: 'assets/images/proprietorship.svg',
  partnershipImagePath: 'assets/images/partnership.svg',
  llcImagePath: 'assets/images/llc.svg',
  scorporationImagePath: 'assets/images/s_corporation.svg',
  ccorporationImagePath: 'assets/images/c_corporation.svg',
  //Company Partner detail questions
  companyAddPartnerDeatils: [
    //{ question1: `What is your partner's first name?`, question2: `What is your first name?`, placeholder: 'First Name', optionType: 'textbox', formControl: 'firstname' },
    //{ question1: `What is your partner's last Name?`, question2: `What is your last name?`, placeholder: 'Last Name', optionType: 'textbox', formControl: 'lastname' },
    { question1: `What is your partner's email address?`, question2: `What is your email address?`, placeholder: 'Email', optionType: 'email', formControl: 'email' },
    //{ question1: `What is your partner's phone number?`, question2: `What is your phone number?`, placeholder: 'Phone Number', optionType: 'textbox', formControl: 'phone' },
    {
      question1: `What is your partner's share ownership in percent?`,
      question2: `What is your share ownership in percent?`,
      placeholder: 'Share Percentage',
      optionType: 'textbox',
      formControl: 'shares'
    }
    //{
    //  question1: `What is Your Partner Residency Status?`,
    //  question2: `What is Your Residency Status?`,
    //  optionType: 'radioButton',
    //  formControl: 'residencyStatus',
    //  gridColumn: 'full',
    //  optionsList: [{ name: ResidencyStatus.USPermanentResident }, { name: ResidencyStatus.USCitizen }, { name: ResidencyStatus.NonResident }]
    //}
  ],

  //Credit Profile Questions
  creditQuestions: [
    { question: 'Have you filed bankruptcy in the last 2 years?', name: 'hasBankruptcy' },
    { question: 'Have you had any judgements against you in the past 12 months?', name: 'hasAnyJudgements' }
  ],

  //Declaration Pending Content
  partnerDeclaration: `Your Partner's Declaration Is Pending.`,
  partnerProcess: `We need you partner's consent to proceed with your application.
      We will process your request and inform you when we have received this consent.
      In case more than one partner, please change Partner's to Partners', consent to consents
      and received this to received these`,

  // Loan Status Content
  ifloanProcess: 'Your loan application is under review',
  ifloanSuccess: 'Congratulations !! Conditionally Approved in Principle',
  ifloanFailed: 'We are sorry we cannot provide you the capital you requested at this time. To discuss this further please contact our support team.',
  ifloanEvaluation: 'We are evaluating your application.',
  ifloanEvaluationSubline: 'A member of our staff will contact you in the next 24 hours.',
  ifloanEvaluationLandingPage: 'We are evaluating your application and a member of our staff will contact you in the next 24 hours.',
  supportTitle: 'Contact our Support and Sales team',
  supportMail: 'support@jamoon.net',
  supportPhone: '+01 987 654 321',

  financeInProgress: 'Data Mapping is in progress.',
  loanIsLocked: 'Loan is locked',
  resetTimer: 3000,

  //Bank details section questions
  whereToDepositLoanAmount: 'Where Do You Want Us To Deposit The Loan Amount?',
  accountNumberField: 'Account Number',
  routingNumberField: 'Routing Number',

  //LoanNeeds Slider
  rangeTypeLoanAmount: 'Loan Amount',
  rangeTypeLoanPeriod: 'Lifecycle',

  addressSameAsBusinessText: 'Address same as Business address',
  years: 'years',
  dobRegex: /^(0[1-9]|1[0-2])\/(0[1-9]|1\d|2\d|3[01])\/(19|20)\d{2}$/,
  defaultTheme: 'Default',
  selectedTheme: 'selectedTheme',
  defaultTitle: 'Sample Bank',

  //Loan status names
  loanStatusDraftTitle: 'Complete Below Steps And Get An Amazing Loan Offer',
  loanStatusLockedTitle: 'Your Loan Application Is Under Review',
  loanStatusApprovedTitle: 'Your Loan Application Has Been Conditionally Approved!',
  loanStatusRejectedTitle: 'We are sorry we cannot provide you the capital you requested at this time',
  //loan needs names
  afterLoginProvide: 'After login you need to provide extra information',
  monthlyRepaymentText: 'Monthly repayment (P&I)',
  totalPayment: 'Total of Payments (based on 36-months term)',

  //Personal Finances Names
  extraThingsQuestion: 'Do you want to add personal finance information?',
  Yes: 'Yes',
  No: 'No',
  continue: 'Continue',
  CheckingQueOne: 'Do not have a checking account',
  CheckingBrokerageQue: 'What is the current value of your portfolio according to your latest brokerage statement? ',
  CheckingRetirementQue: 'What is the current value of your portfolio according to your latest retirement statement? ',
  CheckingQueNotAccount: 'I do not have a savings account',
  CheckingQueNotRetirement: 'I do not have a retirement account',
  CheckingQueNotAccountBrokerage: 'I do not have a brokerage account',
  addAnotherAccount: 'Add Another Account',
  addAnotherReceivable: 'Add Another Receivable',
  addAnotherProperty: 'Add Another Property',
  addAnotherAutomobile:'Add Another Automobile',
  addAnotherMortgageLoan: 'Add Another Mortgage',
  addAnotherInsurance: 'Add Another Insurance',
  AddAnotherCreditCard: 'Add Another Credit Card',
  addAnotherInstalmentLoan: 'Add Another Instalment Loan',
  addAnotherLoan: 'Add Another Loan',
  addAnotherTax: 'Add Another Obligation',
  WhatBalanceAccount: 'What is the balance according to your latest bank statement?',
  submit: 'Submit',
  nextSavings: 'Next - Savings',
  
  nextBrokerage: 'Next - Brokerage',
  nextRetirement: 'Next - Retirement',
  nextLifeInsurance: 'Next - Life Insurance',
  nextReceivables: 'Next - Receivables',
  nextRealEstate: 'Next - Real Estate',
  nextAutoMobile: 'Next - Automobile',
  nextPersonalProperty: 'Next - Personal Property',
  nextCreditCard: 'Next - Credit Card',
  nextMortgage: 'Next - Mortgage Loans',
  nextOtherLoan: 'Next - Other Loans',
  nextInstallmentLoans: 'Next - Installment Loans',
  nextUnpaidTaxes:'Next - Unpaid Taxes',

  brokerage: 'Brokerage',
 
  LoanedMoneyQue: 'Have you personally loaned money to someone? ',
  DebtorsNameQue: 'What is the debtor\'s name?',
  DebatorsAddress: 'What is the debtor\'s address? Integrate with Smarty Streets',
  DebatorRelatedTo: 'Is the debtor related to you?',
  DebatorDropdown: 'What is your relationship with the debtor?',
  DebatorBorrow: 'How much did the debtor borrow from you?',
  WhatSecurityDebator: 'What security has the debtor provided for this loan?',
  LoanedMoney: 'Have you loaned money to someone else? ',

  WhatCurrentValue: 'What is the current value?',
  PledgedBorrowMondeyQue: 'Have you pledged this property to borrow money?',
  LienHolderNameQue: 'What is the name of the lien holder?',
  LienHolderAddress: 'What is the address of the lien holder?',
  MoneyBorrowQue: 'How much money did you borrow?',
  CurrentBalanceLoan: 'What is the current balance of the loan?',
  CurrentStatusLoan: 'What is the current status of the loan?',
  PaymentFrequency: 'What is the payment frequency?',
  IsTheLoanSecured: 'Is the loan secured?',
  WhatPaymentQue: 'What is the [frequency] payment?',
  HowManyAutomobileOwn: 'How many automobile do you own?',
  realestate: 'Real Estate',
  CreditCards: 'Credit Cards',
  HowmanyCreditCards: 'How many active credit cards do you have?',
  WhatTotalLineCredit: 'What is the total line of credit?',
  WhatCurrentBalance: 'What is the current balance?',
  HowManyInstalmentsLoan: 'How many instalment loans do you have?',
  WhatAmountBorrowed: 'What is the total amount borrowed?',
  WhatBalanceLoan: 'What is the balance of this loan according the latest statement?',
  AnyOtherMortgages: 'Do you have any other mortgages?',



  file: 'file',
  contentTypePdf: 'application/pdf',
  contentTypeOctetStream: 'application/octet-stream',
  amazonAWS: 'amazonaws',
  information: 'Information',
  infoDesk: 'Please gather the below mention documents at the ' +
    'beginning of this journey as you will require to complete information' +
    ' about your personal assets, obligations and income during the application.',
  continueApply: 'Continue to Apply',
  openCalculator: 'openCalculator',
  state: 'state',
  code: 'code',
  realmId:'realmId',
  AWSEncryptionMethod: 'AES256',
  shared:'shared',
  applyNewLoan: 'Apply New Loan',

  uploadAdditionalDocument: 'Upload Additional Documents',
  businessRelatedDocument: 'Business Related Documents',
  shareHolderDocument: 'Shareholder Related Documents',
  allowedFileTypesForAdditionalDocuments: 'PDF, XLS, PNG, JPEG, CSV, DOCX',
  allowedCommaSeparatedFileExtensionsForAdditionalDocuments: '.pdf,.xls,.png,.jpeg,.csv,.docx',

  //File types
  pdf: 'pdf',
  xls: 'xls',
  png: 'png',
  jpeg: 'jpeg',
  csv: 'csv',
  docx: 'docx',

  summary: 'Summary',
  yourAssets: 'Your Assets',
  originalValue: 'Original Value',
  originalBalance:'Original Balance',
  currentValue: 'Current Value',
  checking: 'Checking',
  savings: 'Savings',
  brokerages: 'Brokerage',
  retirement: 'Retirement',
  lifeInsurance: 'Life Insurance',
  receivables: 'Receivables',
  realEstate: 'Real Estate',
  autoMobile: 'Automobiles',
  yourObligations: 'Your Obligations',
  creditCards: 'Credit Cards',
  mortgageLoans: 'Mortgage Loans',
  otherLoans: 'Other Loans',
  unpaidTaxes: 'Unpaid Taxes and Other Obligations',
  incomeSection: 'Income Section',
  incomeInformation: 'Income Information',
  installmentLoans: 'Installment Loans',
  currentBalance: 'Current Balance',
  currentNetWorth: 'Current Net Worth',
  totalAssets: 'Total Assets',
  totalObligations: 'Total Obligations',
  personalProperty:'Personal Property'
};



