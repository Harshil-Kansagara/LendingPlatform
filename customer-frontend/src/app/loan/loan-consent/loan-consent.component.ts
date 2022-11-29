import { Component, OnInit, TemplateRef, Pipe, PipeTransform, ElementRef, ViewChildren, QueryList, ChangeDetectorRef } from '@angular/core';
import { AppService } from '../../services/app.service';
import { Constant } from '../../shared/constant';
import { UserAC, EntityAC, FilterAC,
  ConsentAC, LoanPurposeAC, AddressAC, EntityService, ApplicationService, GlobalService, ApplicationAC, ResidencyStatus,
  TaxAC, ApplicationBasicDetailAC, LoanApplicationStatusType, ConsentStatementAC, ProblemDetails, CompanyFinanceAC,
  RecommendedProductAC, ResourceType, AdditionalDocumentAC, DocumentAC, PersonalFinanceAC
} from '../../utils/serviceNew';
import { Observable, Observer, noop, of } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { FormBuilder, Validators } from '@angular/forms';
import { switchMap, map, tap } from 'rxjs/operators';
import { SmartyStreetsService } from '../entity/company-info/smartyStreets.service';
import { TypeaheadMatch } from 'ngx-bootstrap/typeahead/typeahead-match.class';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-loan-consent',
  templateUrl: './loan-consent.component.html',
  styleUrls: ['./loan-consent.component.scss']
})
export class LoanConsentComponent implements OnInit {
  personalFinances: PersonalFinanceAC;
  partnerDeclaration = Constant.partnerDeclaration;
  partnerProcess = Constant.partnerProcess;
  retrieveHeading = Constant.retrieveHeading;
  retrieveInfo = Constant.retrieveInfo;
  pdfIcon = '../../assets/images/pdf.svg';
  xlsIcon = '../../assets/images/xls.svg';
  pngIcon = '../../assets/images/png.svg';
  jpegIcon = '../../assets/images/jpg.svg';
  csvIcon = '../../assets/images/csv.svg';
  docxIcon = '../../assets/images/doc.svg';

  //File types
  pdf = Constant.pdf;
  xls = Constant.xls;
  png = Constant.png;
  jpeg = Constant.jpeg;
  csv = Constant.csv;
  docx = Constant.docx;

  personalFinancesRedirectUrl = Constant.personalFinancesRedirectUrl;
  companyInfoRedirectUrl = Constant.companyInfoRedirectUrl;
  financeRedirectUrl = Constant.financesRedirectUrl;
  additionalDocumentsRedirectUrl = Constant.additionalDocumentsRedirectUrl;
  taxesRedirectUrl = Constant.taxesReturnsRedirectUrl;
  statusRedirectUrl = Constant.loanStatusRedirectUrl;
  businessRelatedDocument = Constant.businessRelatedDocument;
  shareHolderDocument = Constant.shareHolderDocument;
  additionalDocumentsText = Constant.additionalDocuments;
  summary = Constant.summary;
  loanSubmissionInProgress = false;
  statementCsv = `${Constant.incomeStatement},${Constant.balanceSheet},${Constant.cashFlow},${Constant.financialRatio}`;
  interestDisclaimer = Constant.interestDisclaimer;
  statement = Constant.balanceSheet;
  ssnCustomPattern = {
    X: { pattern: new RegExp('\\d'), symbol: 'X' },
    0: { pattern: new RegExp('\\d') }
  };
  maskSSN = true;
  loanConsentTitle = Constant.loanConsentTitle;
  userDetails = [
    { question:'What is your first name?', placeholder:'First name',formControl: 'firstName', optionType: 'textbox' },
    { question:'What is your last name?',  placeholder:'Last name',formControl: 'lastName', optionType: 'textbox' },
    {question:'What is your social security number?',formControl:'ssn',optionType:'ssn'},
    {question:'What is your date of birth?',formControl:'dob',optionType:'datepicker'},
    {question:'What is your email?',formControl:'email',optionType:'email'},
    {question:'What is your mobile phone number?',formControl:'phone',optionType:'number'},
    {question:'What is your residency status?',
      residencyList: [
        { name: ResidencyStatus.USCitizen },
        { name: ResidencyStatus.USPermanentResident },
        {name:ResidencyStatus.NonResident}
      ]},
    {question:'What is your mailing address? (No P.O. Box addresses)',
    addressFields:[      
      { optionType: 'typeahead', placeholder: 'Search Street Line Here...', formControl:'streetLine'},
      { optionType: 'textbox', placeholder: 'City', formControl: 'city' },
      { optionType: 'textbox', placeholder: 'State', formControl: 'stateAbbreviation' },
      { optionType: 'textbox', placeholder: 'Zipcode', formControl: 'zipCode' }
    ]}
  ];
  currentUserDetailsForm = this.fb.group({
    firstName: '',
    lastName: '',
    ssn:'',
    email:'',
    dob:'',
    phone: '',
    residencyStatus: '0',
    streetLine:'',
    city: '',
    stateAbbreviation: '',
    zipCode: '',
    id: null
  });
  maxDate = new Date();
  // Dummy data for statments
  newData=[];
  modalRef: BsModalRef;
  config = {
    ignoreBackdropClick: true,
    class: 'modal-dialog-large summary-modal'
  };
  isTaxUploading = false;
  isAdditionalDocumentUploading = false;

  async openModal2(template: TemplateRef<ElementRef>) {

    if (this.appService.taxFileUploadingInProgress) {
      this.isTaxUploading = true;
    } else {
      await this.fetchTaxBasedOnApplicationStatus();
    }

    if (this.appService.additionalDocumentUploadingInProgress) {
      this.isAdditionalDocumentUploading = true;
    } else {
      await this.fetchAdditionalDocumentsBasedOnApplicationStatus();
    }

    this.modalRef = this.modalService.show(template, this.config,);
  }

  fileUploadingHeading = Constant.fileUploadingHeading;
  fileUploadingInfo = Constant.fileUploadingInfo;

  async fetchTaxBasedOnApplicationStatus() {
    if (this.currentUserDetails && await this.appService.getCurrentLoanApplicationStatus() !== LoanApplicationStatusType.Draft) {
      await this.fetchTaxDocuments(ResourceType.Loan, this.loanApplicationId);
    } else {
      await this.fetchTaxDocuments(ResourceType.Company, await this.appService.getCurrentCompanyId());
    }
  }

  async fetchAdditionalDocumentsBasedOnApplicationStatus() {
    if (this.currentUserDetails && await this.appService.getCurrentLoanApplicationStatus() !== LoanApplicationStatusType.Draft) {
      await this.fetchAdditionalDocuments(ResourceType.Loan);
    } else {
      await this.fetchAdditionalDocuments(ResourceType.Company);
    }
  }

  constructor(private readonly modalService: BsModalService,
    readonly appService: AppService,
    private readonly applicationService: ApplicationService,
    private readonly globalService: GlobalService,
    private readonly toastrService: ToastrService,
    private readonly router: Router,
    private readonly fb: FormBuilder,
    private readonly smartyStreetsService: SmartyStreetsService,
    private readonly entityService: EntityService,
    private readonly datePipe: DatePipe,
    private readonly changeDetectorRef: ChangeDetectorRef) {
    this.taxes = [];
    this.appService.updateLoader(false);
    this.updateButtonRoutes();
    this.appService.pendingDeclaration.subscribe(val => this.declarationPending = val);
  }

  applicationDetails: ApplicationAC;
  addressSuggestions: Observable<any[]>;
  isViewOnlyMode = false;
  productName: string;
  loader: boolean;
  viewButtonLoader = false;
  quickbooks = false;
  xero = false;
  taxes: TaxAC[];
  declarationPending = false;
  loanApplicationId: string;
  shareHolders: UserAC[];
  relatives: EntityAC[];
  consentStatements = new Array<ConsentAC>();
  loanPurpose: LoanPurposeAC;
  subLoanPurposeName;
  incomeStatementName: string = Constant.incomeStatement;
  balanceSheetName: string = Constant.balanceSheet;
  taxesName: string = Constant.taxes;
  allDeclarationDone: boolean;
  currentUserDetails = new EntityAC();
  currentUserAddress: AddressAC = new AddressAC();
  isManual: boolean;
  uploadTitle = Constant.uploadIncomeStatement;
  additionalDocuments: AdditionalDocumentAC[] =[];
  resourceType: typeof ResourceType = ResourceType;
  businessRelatedGroupedDocuments = [];
  shareholderRelatedGroupedDocuments = [];

  //User details questions
  yourFullName: string = Constant.yourFullName;
  whatsYourSSN: string = Constant.whatsYourSSN;
  whatsYourDateOfBirth: string = Constant.whatsYourDateOfBirth;
  yourEmailId: string = Constant.yourEmailId;
  yourPhoneNumber: string = Constant.yourPhoneNumber;
  whereDoYouLive: string = Constant.whereDoYouLive;

  //User details form fields control name
  firstNameControl: string = Constant.firstName;
  lastNameControl: string = Constant.lastName;
  ssnControl: string = Constant.ssn;
  dobControl: string = Constant.dob;
  emailControl: string = Constant.email;
  phoneControl: string = Constant.phone;
  residencyStatusControl: string = Constant.residencyStatus; 
  streetLineControl: string = Constant.streetLine;
  cityControl: string = Constant.city;
  stateAbbreviationControl: string = Constant.stateAbbreviation;
  zipCodeControl: string = Constant.zipCode;

  //User details fields' placeholders
  namePlaceHolder: string = Constant.namePlaceHolder;
  ssnPlaceHolder: string = Constant.ssnPlaceHolder;
  dobPlaceHolder: string = Constant.dobPlaceHolder;
  emailPlaceHolder: string = Constant.emailPlaceHolder;
  phonePlaceHolder: string = Constant.phonePlaceHolder;
  streetLinePlaceHolder: string = Constant.streetLinePlaceHolder;
  cityPlaceHolder: string = Constant.cityPlaceHolder;
  stateAbbreviationPlaceHolder: string = Constant.statePlaceHolder;
  addressSameAsBusinessText: string = Constant.addressSameAsBusinessText;

  //Validation messages
  invalidEmail: string = Constant.invalidEmail;
  invalidPhone: string = Constant.invalidPhone;
  invalidSSN: string = Constant.invalidSSN;
  requiredField: string = Constant.requiredField;
  invalidZipCode = Constant.invalidzipCode;

  isValidAddress = false;
  isUserDetailsFetched = false;
  isAddressSameAsBusinessAddress = false;
  isBusinessTypeCCorp = true;
  minAgeYears: number;

  
  applicationStatus: string;
  loanApplicationStatusEnum = LoanApplicationStatusType;

  currencySymbol: string;
  periodUnit: string;
  incomeStatementData: [];
  balanceSheetData: [];
  cashFlowData: [];
  ratioData: [];
  periodList: [];
  allConsentStatements: Array<ConsentStatementAC>;

  //To set the view only mode of locked application.
  @ViewChildren('locked') lockMode: QueryList<ElementRef>;
  async ngAfterViewInit() {
    const currentStatus = LoanApplicationStatusType[await this.appService.getCurrentLoanApplicationStatus()];
    if (currentStatus && LoanApplicationStatusType[currentStatus] !== LoanApplicationStatusType.Draft) {
      this.lockMode.forEach(el => el.nativeElement.classList.add('locked-state'));
    }
  }

  async ngOnInit() {
    this.loader = true;
    // Check tab accessibility for consent separately as footer changes for this section.
    this.currentUserDetails = await this.appService.getCurrentUserDetailsNew();
    await this.setAppSettingsValues();
    this.getFinancialReports();
    
    this.isViewOnlyMode = await this.appService.isViewOnlyMode();
    this.loanApplicationId = await this.appService.getCurrentLoanApplicationId();
    
    if (this.loanApplicationId !== null) {

      this.globalService.getLoanPurposeList().subscribe(
        async purposes => {
          this.globalService.getConsentStatements().subscribe(
            async statements => {
              this.allConsentStatements = statements;
              // If the current loan application is locked then use JSON of its details
              // otherwise make the backend call to get the details.

              // Get the locked application object from JSON stored in localForage. If the JSON not found in localForage
              // then only make the backend call to get the application details.
              const lockedApplication = await this.appService.getLockedApplicationJsonAsObject();
              if (lockedApplication && this.currentUserDetails && await this.appService.getCurrentLoanApplicationStatus() !== LoanApplicationStatusType.Draft) {
                this.applicationService.getPersonalFinances(this.loanApplicationId, 'summary').subscribe(entity => {
                  this.personalFinances = entity.filter(x => x.id === this.currentUserDetails.id)[0].personalFinance;
                });
                await this.setCurrentUserDetailsForm(lockedApplication.borrowingEntities[0].linkedEntities.filter(x => x.id === this.currentUserDetails.id)[0]);
                await this.setLoanApplicationDetails(lockedApplication, purposes);
                this.setCompanyId(lockedApplication.borrowingEntities[0].id);
                this.loader = false;
              } else {
                //Fetch and set the current user details in form.
                const filterAC = new FilterAC();
                filterAC.field = 'type';
                filterAC.operator = '=';
                filterAC.value = 'people';
                const filters = new Array<FilterAC>();
                filters.push(filterAC);
                this.entityService.getEntityList(null, null, null, JSON.stringify(filters), null, null).subscribe(
                  async res => {
                    await this.setCurrentUserDetailsForm(res[0]);
                    this.entityService.getPersonalFinances(res[0].id, 'summary').subscribe(finances => {
                      this.personalFinances = finances;
                    });
                    //Fetch and set the loan application details.
                    this.applicationService.getLoanApplicationDetailsById(this.loanApplicationId).subscribe(
                      async updatedDetails => {
                        await this.setLoanApplicationDetails(updatedDetails, purposes);
                        this.setCompanyId(updatedDetails.borrowingEntities[0].id);

                        //Fetch the selected product details for current application.
                        this.applicationService.getSelectedProduct(this.loanApplicationId).subscribe(
                          product => {
                            this.assignProductInVariable(product);
                            this.loader = false;
                          });
                      },
                      err => {
                      });
                  },
                  err => {
                    this.isUserDetailsFetched = true;
                  });
              }
            },
            err => {
            }
          );
        },
        err => {
        });
    }

    //SmartyStreet autocomplete api
    this.addressSuggestions = new Observable((observer: Observer<string>) => {
      observer.next(this.currentUserDetailsForm.get('streetLine').value);
    }).pipe(
      switchMap((token: string) => {
        this.isValidAddress = false;
        if (token) {
          return this.smartyStreetsService.fetchSuggestions(token)
            .pipe(
              map((data: any) => {
                return data && data.result || [];
              }),
              tap(() => noop, err => {
                this.toastrService.error(Constant.someThingWentWrong);
              })
            );
        } else {
          return of([]);
        }
      })
    );
  }

  /**
   * Method to set the variables with appsettings' values.
   * */
  async setAppSettingsValues() {
    this.currencySymbol = (await this.appService.getAppSettings()).filter(x => x.fieldName === 'Currency:Symbol')[0].value;
    this.periodUnit = (await this.appService.getAppSettings()).filter(x => x.fieldName === 'LoanNeeds:LoanDurationUnit')[0].value;
    this.minAgeYears = (Number)((await this.appService.getAppSettings()).filter(x => x.fieldName === 'User:MinimumAgeRequiredInYears')[0].value);
    this.maxDate = new Date(new Date().setFullYear(new Date().getFullYear() - this.minAgeYears));
  }

  /**
   * Method to set the consent statements in an array.
   * @param statements List of statements
   */
  setConsentStatementsInArray(statements: ConsentStatementAC[]) {
    if (statements.length > 0) {
      for (const consent of statements) {
        const consentAc = new ConsentAC();
        consent.consentText = consent.consentText.toLowerCase();
        const first = consent.consentText.substr(0, 1).toUpperCase();
        consentAc.consentText = first + consent.consentText.substr(1);
        consentAc.isConsentGiven = false;
        this.consentStatements.push(consentAc);
      }
    }
  }

  /**
   * Method to set the current company id in localForage.
   * @param id Current company id
   */
  async setCompanyId(id: string) {
    const currentCompanyId = await this.appService.getCurrentCompanyId();
    if (!currentCompanyId) {
      this.appService.setCurrentCompanyId(id);
    }
    this.getFinancialReports();
  }

  /**
   * Method to set the current user details in form.
   * @param entity EntityAC object
   */
  async setCurrentUserDetailsForm(entity: EntityAC) {
    //Set the current user details and name.
    await this.appService.setCurrentUserDetailsNew(entity);
    this.appService.setCurrentUserName(`${entity.user.firstName}${' '}${entity.user.lastName}`);
    this.currentUserDetails = entity;
    this.currentUserDetails.address = entity.address ?? new AddressAC();

    if (this.currentUserDetails !== null) {
      if (this.currentUserDetails.user.residencyStatus === null) {
        this.currentUserDetails.user.residencyStatus = ResidencyStatus.USCitizen;
      }
      const ten = 10;
      if (this.currentUserDetails.user.phone !== null && this.currentUserDetails.user.phone.includes('+')) {
        this.currentUserDetails.user.phone = this.currentUserDetails.user.phone.substr(this.currentUserDetails.user.phone.length - ten);
      }
      if (this.currentUserDetails.user.dob) {
        this.currentUserDetails.user.dob = new Date(this.currentUserDetails.user.dob);
      }
      this.currentUserDetailsForm = this.fb.group({
        id: null,
        firstName: [this.currentUserDetails.user.firstName, [Validators.required]],
        lastName: [this.currentUserDetails.user.lastName, [Validators.required]],
        ssn: [this.currentUserDetails.user.ssn, [Validators.required]],
        dob: [this.currentUserDetails.user.dob, [Validators.required]],
        email: [this.currentUserDetails.user.email, [Validators.required, Validators.email]],
        phone: [this.currentUserDetails.user.phone, [Validators.required, Validators.pattern(Constant.onlyTenDigitPattern)]],
        residencyStatus: [String(this.currentUserDetails.user.residencyStatus), [Validators.required]],
        streetLine: ['', { validators: [Validators.required], updateOn: Constant.change }],
        city: ['', { validators: [Validators.required], updateOn: Constant.change }],
        stateAbbreviation: ['', { validators: [Validators.required], updateOn: Constant.change }],
        zipCode: ['', { validators: [Validators.pattern(Constant.zipCodePattern)], updateOn: Constant.change }]
      }, { updateOn: Constant.blur });

      if (this.currentUserDetails.address.id) {
        this.setAddressFieldsInFormUserDetailsForm();
        this.currentUserAddress = this.currentUserDetails.address;
      }
      this.isUserDetailsFetched = true;
    }
  }

  /**
   * Method to fetch the additional document for given id (of loan or entity)
   * @param resourceType Resource type
   */
  async fetchAdditionalDocuments(resourceType: ResourceType) {
    //Fetch the documents according to the given resource type
    if (resourceType === ResourceType.Company) {
      this.entityService.getAdditionalDocumentsForEntity(await this.appService.getCurrentCompanyId()).subscribe(
        entityAC => {
          this.checkResponseAndSetTheList(entityAC);
        },
        err => {
        });
    } else {
      this.applicationService.getAdditionalDocumentsForApplication(this.loanApplicationId).subscribe(
        entityAC => {
          this.checkResponseAndSetTheList(entityAC);
        },
        err => {
        });
    }
  }

  /**
   * Method to fetch the tax documents for given id (of loan or entity)
   * @param resourceType Resource type
   */
  async fetchTaxDocuments(resourceType: ResourceType, id: string) {
    //Fetch the documents according to the given resource type
    if (resourceType === ResourceType.Company) {
      this.entityService.getTaxListByEntityId(id).subscribe(
        async (entityACResponse: EntityAC) => {
          this.taxes = entityACResponse.taxes;
          this.isTaxUploading = false;
          this.loader = false;
        });
    } else {
      this.applicationService.getTaxListByApplicationId(id).subscribe(
        async (entityACResponse: EntityAC) => {
          this.taxes = entityACResponse.taxes;
          this.isTaxUploading = false;
          this.loader = false;
        });
    }
  }

  /**
   * Method to check the response and do the list setting accordingly
   * @param entityAC EntityAC response object
   */
  checkResponseAndSetTheList(entityAC: EntityAC) {
    if (entityAC) {
      this.setAdditionalDocumentListInDetailsObject(entityAC);
    }
  }

  /**
   * Method to set the address in user details form.
   * */
  setAddressFieldsInFormUserDetailsForm() {
    this.currentUserDetailsForm.get('id').setValue(this.currentUserDetails.address.id);
    this.currentUserDetailsForm.get(this.streetLineControl).setValue(this.currentUserDetails.address.streetLine);
    this.currentUserDetailsForm.get(this.cityControl).setValue(this.currentUserDetails.address.city);
    this.currentUserDetailsForm.get(this.stateAbbreviationControl).setValue(this.currentUserDetails.address.stateAbbreviation);
    this.currentUserDetailsForm.get(this.zipCodeControl).setValue(this.currentUserDetails.address.zipCode);
    this.isValidAddress = this.currentUserDetails.address.id === Constant.guidEmptyString ? false : true;
  }

  /**
   * Method sets all the application details. (Basic details, taxes, relatives, product, etc..)
   * @param applicationDetail Application details object
   * @param purposes List of loan purposes
   */
  async setLoanApplicationDetails(applicationDetail: ApplicationAC, purposes: LoanPurposeAC[]) {
    this.applicationDetails = applicationDetail;
    this.applicationStatus = LoanApplicationStatusType[await this.appService.getCurrentLoanApplicationStatus()];
    this.relatives = applicationDetail.borrowingEntities[0].linkedEntities;
    this.relatives = this.setCurrentUserFirstInList(this.relatives);
    this.loanPurpose = purposes.filter(x => x.id === applicationDetail.basicDetails.loanPurposeId)[0];
    this.subLoanPurposeName = this.loanPurpose.subLoanPurposes.filter(x => x.id === applicationDetail.basicDetails.subLoanPurposeId)[0].name;
    if (this.applicationDetails.borrowingEntities[0].company.companyStructure.structure !== Constant.cCorporation) {
      this.isBusinessTypeCCorp = false;
    }

    if (this.applicationDetails.basicDetails.loanPeriod > 1) {
      this.periodUnit = this.periodUnit.concat('s');
    }

    this.setConsentStatemetArrayByUser();

    this.allDeclarationDone = false;
    if (this.relatives.filter(x => x.consents.length === 0).length === 0) {
      this.allDeclarationDone = true;
      this.declarationPending = false;
    } else {
      if (this.relatives && this.relatives.filter(x => x.id === this.currentUserDetails.id)[0].consents.length !== 0) {
        this.declarationPending = true;
      }
    }
  }

  /**
   * Method to set the consent statement array according to the current user's consents.
   * */
  setConsentStatemetArrayByUser() {
    if (this.allConsentStatements && this.allConsentStatements.length > 0) {
      if (this.currentUserDetails.consents && this.currentUserDetails.consents.length !== 0) {
        const tempAllConsentStatements = [];
        for (const consent of this.currentUserDetails.consents) {
          const element = this.allConsentStatements.filter(x => x.id === consent.consentId
            || x.consentText === consent.consentText)[0];
          if (element) {
            tempAllConsentStatements.push(element);
          }
        }
        this.allConsentStatements = tempAllConsentStatements;
        this.setConsentStatementsInArray(this.allConsentStatements);
        this.consentStatements.forEach(x => x.isConsentGiven = true);
      } else {
        this.allConsentStatements = this.allConsentStatements.filter(x => x.isEnabled);
        this.setConsentStatementsInArray(this.allConsentStatements);
      }
    }
  }

  /**
   * Method to assign the fetched product to local vairable if it is not null.
   * @param product Prodcut details object
   */
  assignProductInVariable(product: RecommendedProductAC) {
    if (product !== null) {
      this.applicationDetails.selectedProduct = product;
    }
  }

  /**
   * Method to set additional documents in the list
   * @param entityAC EntityAC object
   */
  setAdditionalDocumentListInDetailsObject(entityAC: EntityAC) {
    //If additional documents are present then only set them in the list
    if (entityAC.additionalDocuments && entityAC.additionalDocuments.length > 0) {
      this.additionalDocuments = entityAC.additionalDocuments;
      this.prepareGroupedDocumentList();
    }
  }

  /**
   * Method to set the grouped documents in the list
   * */
  prepareGroupedDocumentList() {
    if (this.additionalDocuments && this.additionalDocuments.length > 0) {
      const businessRelatedDocuments = this.additionalDocuments.filter(x => x.documentType.documentTypeFor === ResourceType.Company);
      const shareholderRelatedDocuments = this.additionalDocuments.filter(x => x.documentType.documentTypeFor === ResourceType.User);

      //Based on the types, fill documents in separate lists
      if (businessRelatedDocuments && businessRelatedDocuments.length > 0) {
        this.businessRelatedGroupedDocuments = this.prepareDocumentListGroupedByDcoumentType(businessRelatedDocuments);
      }
      if (shareholderRelatedDocuments && shareholderRelatedDocuments.length > 0) {
        this.shareholderRelatedGroupedDocuments = this.prepareDocumentListGroupedByDcoumentType(shareholderRelatedDocuments);
      }
      this.isAdditionalDocumentUploading = false;
    }
  }

  /**
   * Method to prepare list grouped by document type
   * @param documents List of document to be grouped
   */
  prepareDocumentListGroupedByDcoumentType(documents: AdditionalDocumentAC[]) {
    const groupedList = [];
    const distinctDocumentTypes = documents.map(x => x.documentType.type).filter((value, index, self) => self.indexOf(value) === index);
    //For each distinct type, fill its documents in grouped list
    for (const type of distinctDocumentTypes) {
      const groupedObject = { type: documents.find(x => x.documentType.type === type).documentType, documents: [] };
      for (const document of documents.filter(x => x.documentType.type === type)) {
        //Get the file extension to set the file type
        const fileType = document.document.name.split('.')[1];
        groupedObject.documents.push({ additionalDocument: document, fileType });
      }
      groupedList.push(groupedObject);
    }
    return groupedList;
  }

  /**
   * Method to download the additional document
   * @param additionalDocument Additional document object
   */
  downloadAdditionalDocument(additionalDocument: AdditionalDocumentAC) {
    this.fileDownload(additionalDocument.document);
  }

  /**
   * Method to set the previous and next route.
   * */
  updateButtonRoutes() {
    this.appService.updateRoute(Constant.taxesReturnsRedirectUrl);
    this.appService.updateNextRoute(Constant.loanStatusRedirectUrl);
  }

  /**
   * Method to set the address fields with selected address suggestion.
   * @param event Event of TypeaheadMatch
   */
  onSelect(event: TypeaheadMatch): void {
    this.currentUserDetailsForm.get(this.streetLineControl).setValue(event.item.streetLine);
    this.currentUserDetailsForm.get(this.cityControl).setValue(event.item.city);
    this.currentUserDetailsForm.get(this.stateAbbreviationControl).setValue(event.item.state);
  }

  /**
   * Method to set the current logged user first in the list shown in UI.
   * @param userList List of shareholders
   */
  setCurrentUserFirstInList(userList: EntityAC[]) {
    if (userList.filter(x => x.id === this.currentUserDetails.id)[0] !== null) {
      const currentuserInList = userList.filter(x => x.id === this.currentUserDetails.id)[0];
      const otherUsers = userList.filter(x => x.id !== this.currentUserDetails.id);
      otherUsers.push(currentuserInList);
      return otherUsers.reverse();
    } else {
      return userList;
    }
  }

  /**
   * Method to save the user details and consent.
   * */
  saveConsent() {  
    this.currentUserDetailsForm.markAllAsTouched();
    if (this.currentUserDetailsForm.valid && !this.appService.taxFileUploadingInProgress && !this.appService.additionalDocumentUploadingInProgress) {
        // If all the consent texts are ticked, only then isConsentGiven will be marked true
        if (this.consentStatements.filter(x => x.isConsentGiven).length === this.consentStatements.length) {
          this.appService.updateLoader(true);
          this.loanSubmissionInProgress = true;

          //Assign values to current user details object
          this.currentUserDetails.user.firstName = this.currentUserDetailsForm.get(this.firstNameControl).value;
          this.currentUserDetails.user.lastName = this.currentUserDetailsForm.get(this.lastNameControl).value;
          this.currentUserDetails.user.ssn = this.currentUserDetailsForm.get(this.ssnControl).value;
          this.currentUserDetails.user.dob = this.currentUserDetailsForm.get(this.dobControl).value;
          this.currentUserDetails.user.email = this.currentUserDetailsForm.get(this.emailControl).value;
          this.currentUserDetails.user.phone = this.currentUserDetailsForm.get(this.phoneControl).value;
          this.currentUserDetails.user.residencyStatus = this.currentUserDetailsForm.get(this.residencyStatusControl).value;
          this.currentUserDetails.address.id = this.currentUserDetailsForm.get('id').value;
          this.currentUserDetails.address.streetLine = this.currentUserDetailsForm.get(this.streetLineControl).value;
          this.currentUserDetails.address.city = this.currentUserDetailsForm.get(this.cityControl).value;
          this.currentUserDetails.address.stateAbbreviation = this.currentUserDetailsForm.get(this.stateAbbreviationControl).value;
          this.currentUserDetails.address.zipCode = this.currentUserDetailsForm.get(this.zipCodeControl).value;
          this.currentUserDetails.user.phone = `+1${this.currentUserDetails.user.phone}`;
          this.entityService.updateEntity(this.currentUserDetails.id, this.currentUserDetails).subscribe(
            (entity: EntityAC) => {
              //Update current user details in localForage
              this.appService.setCurrentUserDetailsNew(entity);

              this.applicationService.saveLoanConsentOfUser(this.loanApplicationId).subscribe(
                async data => {
                  //Fetch and save the current user's credit report.
                  const application: ApplicationBasicDetailAC = new ApplicationBasicDetailAC();
                  application.id = this.loanApplicationId;
                  this.applicationService.fetchCreditReport(application.id, this.currentUserDetails.id).subscribe(
                    res => {

                    });

                  //Get the updated details of loan application and its relatives.
                  this.applicationService.getLoanApplicationDetailsById(this.loanApplicationId).subscribe(
                    async res => {
                      this.applicationDetails = res;
                      this.relatives = this.setCurrentUserFirstInList(res.borrowingEntities[0].linkedEntities);

                      this.appService.completeDeclaration(true);
                      this.appService.changeMode(true);
                      this.appService.setViewOnlyMode(true);
                      this.isViewOnlyMode = true;

                      if (this.relatives.filter(x => x.consents.length !== 0).length === this.relatives.length) {
                        this.allDeclarationDone = true;
                        this.declarationPending = false;
                        this.applicationService.fetchCreditReport(application.id, await this.appService.getCurrentCompanyId()).subscribe(
                          result => {
                            //Lock the application
                            this.applicationService.lockLoanApplication(this.loanApplicationId).subscribe(
                              lockRes => {
                                this.applicationService.getLoanApplicationDetailsById(application.id).subscribe(
                                  async lockedDetails => {
                                    await this.appService.setLockedApplicationAsJsonString(lockedDetails);
                                    this.appService.setCurrentLoanApplicationStatus(LoanApplicationStatusType.Locked);
                                    this.lockMode.forEach(el => el.nativeElement.classList.add('locked-state'));
                                  },
                                  err => {
                                  });
                              },
                              err => {
                              });
                          }
                        );
                        this.appService.updateCurrentSectionName(Constant.loanStatus);
                        this.loanSubmissionInProgress = false;
                        this.appService.updateProgressbar(Constant.loanStatusProgressBar);
                        this.router.navigate([Constant.loanStatusRedirectUrl]);
                      } else {
                        this.loanSubmissionInProgress = false;
                      }
                    },
                    err => {
                      this.declarationPending = false;
                    });
                },
                (err: ProblemDetails) => {
                  this.toastrService.error(err.detail);
                  this.declarationPending = false;
                  this.loanSubmissionInProgress = false;
                });
            },
            (err: ProblemDetails) => {
              this.toastrService.error(err.detail);
              this.loanSubmissionInProgress = false;
            });
        } else {
          this.toastrService.error(Constant.loanConsentValidationError);
          this.declarationPending = false;
        }
      
    } else {
      if (this.appService.taxFileUploadingInProgress) {
        this.toastrService.error(Constant.taxFileUploadInProgress);
      } else if (this.appService.additionalDocumentUploadingInProgress) {
        this.toastrService.error(Constant.additionalDocumentUploadInProgress);
      } else {
        this.toastrService.error(Constant.fillAllRequiredFieldsError);
      }
    }
  }

  /**
   * Method will be called when consent is checked.
   * @param consentText Consent's text which is being checked
   */
  selectConsent(consentText) {
    this.consentStatements.filter(x => x.consentText === consentText)[0].isConsentGiven =
      !this.consentStatements.filter(x => x.consentText === consentText)[0].isConsentGiven;
  }

  /**
   * Method to download the tax file
   * @param tax
   */
  downloadTaxFile(tax: TaxAC) {
    this.fileDownload(tax.entityTaxAccount.document);
  }

  /**
   * Method to download the provided document.
   * @param document DocumentAC object
   */
  fileDownload(document: DocumentAC) {
    if (document.id && document.downloadPath === null) {
      this.globalService.getDocument(document.id).subscribe(
        (preSignedUrl: string) => {
          if (preSignedUrl != null) {
            window.open(preSignedUrl, '_blank');
          }
        }, (err: ProblemDetails) => {
          if (err.status === Constant.badRequest) {
            this.toastrService.error(err.detail);
          }
        });
    }
  }

  /**
   * Method to navigate the user to the section whose edit icon has been clicked.
   * @param redirectUrl Section's url
   */
  redirectToSection(redirectUrl: string) {
    this.modalRef.hide();
    this.router.navigate([redirectUrl]);
  }


  // Get the financial reports
  async getFinancialReports() {
    if (await this.appService.isFinanceInProgress()) {
      this.newData = null;
    } else {
      const entityId = await this.appService.getCurrentCompanyId();
      // If the application of the current loan application is locked then use JSON of its details
      // otherwise make the backend call to get the details.

      // Get the locked application object from JSON stored in localForage. If the JSON not found in localForage
      // then only make the backend call to get the application details.
      const lockedApplication = await this.appService.getLockedApplicationJsonAsObject();
      if (lockedApplication && await this.appService.getCurrentLoanApplicationStatus() !== LoanApplicationStatusType.Draft) {
        this.fetchFinancesFromBackend(this.loanApplicationId, ResourceType.Loan);
      } else {

        // Get from backend
        this.fetchFinancesFromBackend(entityId, ResourceType.Company);
      }
    }
    
    
  }

  

  // Fetch finances from backend
  fetchFinancesFromBackend(id, type: ResourceType) {
    if (type === ResourceType.Company) {
      this.entityService.getCompanyFinances(id, this.statementCsv)
        .subscribe(async (entityFinances: CompanyFinanceAC[]) => {
          this.handleFinanceSubscription(entityFinances);

        });
    }
    else if (type === ResourceType.Loan) {
      this.applicationService.getFinances(id, this.statementCsv)
        .subscribe(async (entityFinances: CompanyFinanceAC[]) => {
          this.handleFinanceSubscription(entityFinances);
        });
    }
  }

  async handleFinanceSubscription(entityFinances: CompanyFinanceAC[]) {
    // If the finances are already fetched but mapping is still in progress
    if (entityFinances && entityFinances.length > 0 && entityFinances[0].isChartOfAccountMapped) {
      // If the finances are fetched as well as mapped
      await this.appService.setFinanceMappingInProgress(false);
      this.newData = entityFinances;
      this.incomeStatementData = this.newData.filter(x => x.financialStatement === 'Income Statement')[0].standardAccountList;
      this.balanceSheetData = this.newData.filter(x => x.financialStatement === 'Balance Sheet')[0].standardAccountList;
      this.cashFlowData = this.newData.filter(x => x.financialStatement === 'Cash Flow')[0].standardAccountList;
      this.ratioData = this.newData.filter(x => x.financialStatement === 'Financial Ratios')[0].standardAccountList;

      this.periodList = this.newData.find(x => x.financialAccounts).financialAccounts.map(x => x.period);
    }
  }

  /**
   * Method to set the user's address same as business's address 
   * */
  addressIsSameAsBusiness() {
    this.isAddressSameAsBusinessAddress = !this.isAddressSameAsBusinessAddress;
    if (this.isAddressSameAsBusinessAddress) {
      const businessAddress = this.applicationDetails.borrowingEntities[0].address;
      this.currentUserDetailsForm.get(this.streetLineControl).setValue(businessAddress.streetLine);

      if (businessAddress.primaryNumber !== null) {
        this.currentUserDetailsForm.get(this.streetLineControl).setValue(`${businessAddress.primaryNumber} ${this.currentUserDetailsForm.get(this.streetLineControl).value}`);
      }
      if (businessAddress.streetSuffix !== null) {
        this.currentUserDetailsForm.get(this.streetLineControl).setValue(`${this.currentUserDetailsForm.get(this.streetLineControl).value} ${businessAddress.streetSuffix}`);
      }
      if (businessAddress.secondaryNumber !== null) {
        this.currentUserDetailsForm.get(this.streetLineControl).setValue(`${this.currentUserDetailsForm.get(this.streetLineControl).value} ${businessAddress.secondaryNumber}`);
      }
      if (businessAddress.secondaryDesignator !== null) {
        this.currentUserDetailsForm.get(this.streetLineControl).setValue(`${this.currentUserDetailsForm.get(this.streetLineControl).value} ${businessAddress.secondaryDesignator}`);
      }

      this.currentUserDetailsForm.get(this.cityControl).setValue(businessAddress.city);
      this.currentUserDetailsForm.get(this.stateAbbreviationControl).setValue(businessAddress.stateAbbreviation);
      this.currentUserDetailsForm.get(this.zipCodeControl).setValue(businessAddress.zipCode);
      this.isValidAddress = true;
    } else {
      if (this.currentUserDetails.address.id) {
        this.setAddressFieldsInFormUserDetailsForm();
      } else {
        this.currentUserDetailsForm.get('id').setValue(null);
        this.currentUserDetailsForm.get(this.streetLineControl).setValue('');
        this.currentUserDetailsForm.get(this.cityControl).setValue('');
        this.currentUserDetailsForm.get(this.stateAbbreviationControl).setValue('');
        this.currentUserDetailsForm.get(this.zipCodeControl).setValue('');
        this.isValidAddress = false;
      }
    }
  }

  // method to select statement
  selectedStatement(statement) {
    this.statement = statement;
  }

  // method to mask ssn
  ssnFocusFunction() {
    this.maskSSN = !this.maskSSN;
  }

    /**
   * Method will redirect to product list page
   */
  redirectToProductList() : void {
    this.router.navigate([Constant.loanProductRedirectUrl]);
  }

/**
 * Method to open personal finances summary modal
 */
  openModalSummary(personalSummaryModal: TemplateRef<ElementRef>) {
    this.modalRef = this.modalService.show(personalSummaryModal, this.config);
  }
}

@Pipe({
  name: 'residencyStatusEnum'
})
export class ResidencyStatusEnumPipe implements PipeTransform {
  
  transform(enumNumber): string {
    const zero = 0;
    const one = 1;
    const two = 2;
    if (+enumNumber === zero) {
      return Constant.usCitizen;
    } else if (+enumNumber === one) {
      return Constant.usPermanentResident;
    } else if (+enumNumber === two) {
      return Constant.nonResident;
    } else {
      return null;
    }
  }
}


@Pipe({
  name: 'secureInput'
})
export class SecureInputPipe implements PipeTransform {
  transform(string: string): string {
    const four = 4;
    return 'XXXXX' + string.substring(string.length - four);
  }
}​​​​​
