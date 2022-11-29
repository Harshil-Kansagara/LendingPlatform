import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, noop } from 'rxjs';
import { AppSettingAC, SectionAC, ConfigurationAC,LoanEntityBankDetailsAC, LoanApplicationStatusType,
  EntityAC, ApplicationAC, ResidencyStatus, RecommendedProductAC, ApplicationBasicDetailAC, TaxAC } from '../utils/serviceNew';
import localForage from 'localforage';
import { Constant } from '../shared/constant';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { map, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { LoanNeedsValues } from '../loan/models/loan-needs-values.model';
import { UserAC, CompanyAC } from '../utils/service';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})

export class AppService {

  //Taxes
  taxFileUploadingInProgress = false;
  taxesData = [];
  taxes: TaxAC[] = [];
  showTaxTimer = false;

  //Additional documents
  additionalDocumentUploadingInProgress = false;
  businessRelatedDocumentsData = [];
  shareholderRelatedDocumentsData = [];
  showAdditionalDocumentTimer = false;

  private readonly loaderChange = new BehaviorSubject(false);
  currentLoader = this.loaderChange.asObservable();

  private readonly currentUserNameChange = new BehaviorSubject('');
  currentUserName = this.currentUserNameChange.asObservable();

  private readonly currentUserEmailChange = new BehaviorSubject('');
  currentUserEmail = this.currentUserEmailChange.asObservable();

  private readonly companiesListAtHeaderChange = new BehaviorSubject(new Array<EntityAC>());
  companiesListAtHeader = this.companiesListAtHeaderChange.asObservable();

  //To toggle sidenav 
  private readonly menuMode = new BehaviorSubject(false);
  overflow = this.menuMode.asObservable();

  // Back button method
  private readonly peviousSource = new BehaviorSubject('');
  currentSource = this.peviousSource.asObservable();

  // Next button method
  private readonly nextSource = new BehaviorSubject('');
  nextAvailableSource = this.nextSource.asObservable();

  // Hide Footer after loan consent
  private readonly noFooter = new BehaviorSubject(false);
  hideFooter = this.noFooter.asObservable();

  // Show Declaration on Applying
  private readonly showDeclaration = new BehaviorSubject(false);
  pendingDeclaration = this.showDeclaration.asObservable();

  // view only button method
  private readonly viewOnly = new BehaviorSubject(false);
  currentMode = this.viewOnly.asObservable();

  // Is company editable for current user.
  private readonly isCompanyEditable = new BehaviorSubject(true);
  companyEditMode = this.isCompanyEditable.asObservable();

  // loan application number
  private readonly loanApplicationNumber = new BehaviorSubject('');
  currentLoanApplicationNumber = this.loanApplicationNumber.asObservable();

  // Current url string
  private readonly currentUrlString = new BehaviorSubject('');
  currentUrl = this.currentUrlString.asObservable();

  // Previous url string
  private readonly previousUrlString = new BehaviorSubject('');
  previousUrl = this.previousUrlString.asObservable();

  // List of entity
  private readonly entityList = new BehaviorSubject([]);
  currentUserEntityList = this.entityList.asObservable();

  // Selected entity
  private readonly selectedEntity = new BehaviorSubject({ id: null, name: null });
  selectedEntityEvent = this.selectedEntity.asObservable();


  // Per window/tab localforage store
  localForage = localForage.createInstance({
    name: window.name,
    storeName: window.name
  });


  // Common localForage store
  localForageShared = localForage.createInstance({
    name: Constant.shared,
    storeName: Constant.shared
  });

  constructor(private readonly oidcSecurityService: OidcSecurityService,
    private readonly router: Router,
    private readonly httpClient: HttpClient) { }

  // From product detail component to product list component
  private readonly fromProductDetail = new BehaviorSubject(false);
  toProductListPage = this.fromProductDetail.asObservable();

  // From product detail component to product list component
  redirectToProductListPage(flag: boolean) {
    this.fromProductDetail.next(flag);
  }

  // Loan  product detail
  async setLoanProductDetail(recommendedProductAC: RecommendedProductAC) {
    await this.localForage.setItem(Constant.currentLoanProduct, recommendedProductAC);
  }

  async getLoanProductDetail() {
    return this.localForage.getItem(Constant.currentLoanProduct);
  }

  async removeLoanProductDetail() {
    await this.localForage.removeItem(Constant.currentLoanProduct);
  }

  /* To Update progressbar on each component */
  async updateProgressbar(progress) {
    await this.localForage.setItem(Constant.currentProgress, progress);
  }

  async getCurrentProgress() {
    return this.localForage.getItem(Constant.currentProgress);
  }

  updateLoader(flag: boolean) {
    this.loaderChange.next(flag);
  }

  // Current section name 
  async updateCurrentSectionName(section: string) {
    await this.localForage.setItem(Constant.currentSectionName, section);
  }

  async getCurrentSectionName(): Promise<string> {
    return this.localForage.getItem(Constant.currentSectionName);
  }

  async removeCurrentSectionName() {
    await this.localForage.removeItem(Constant.currentSectionName);
  }

  // Loan Application Number
  async setCurrentLoanApplicationNumber(number: string) {
    this.loanApplicationNumber.next(number);
    await this.localForage.setItem(Constant.currentLoanApplicationNumber, number);
  }

  async getCurrentLoanApplicationNumber(): Promise<string> {
    return this.localForage.getItem(Constant.currentLoanApplicationNumber);
  }

  async removeCurrentLoanApplicationNumber() {
    await this.localForage.removeItem(Constant.currentLoanApplicationNumber);
  }

  // Loan application id
  async setCurrentLoanApplicationId(id: string) {
    await this.localForage.setItem(Constant.currentLoanApplicationId, id);
  }

  async getCurrentLoanApplicationId(): Promise<string> {
    return this.localForage.getItem(Constant.currentLoanApplicationId);
  }

  async removeCurrentLoanApplicationId() {
    await this.localForage.removeItem(Constant.currentLoanApplicationId);
  }

  // async mode
  async setFinanceMappingInProgress(isFinanceInProgress) {
    await this.localForage.setItem('financesInProgress', isFinanceInProgress);
  }

  async isFinanceInProgress(): Promise<boolean> {
    return this.localForage.getItem('financesInProgress');
  }

  async removeFinanceMappingInProgress() {
    await this.localForage.removeItem('financesInProgress');
  }

  // View only Mode
  async setViewOnlyMode(isViewOnlyMode: boolean) {
    await this.localForage.setItem(Constant.isViewOnlyMode, isViewOnlyMode);
  }

  async isViewOnlyMode(): Promise<boolean> {
    return this.localForage.getItem(Constant.isViewOnlyMode);
  }

  // Company details  (OLD)
  async setCurrentCompanyDetails(companyAC: CompanyAC) {
    await this.localForage.setItem(Constant.currentCompanyDetails, companyAC);
  }

  async getCurrentCompanyDetails(): Promise<CompanyAC> {
    return this.localForage.getItem(Constant.currentCompanyDetails);
  }

  //Company Id
  async setCurrentCompanyId(companyId: string) {
    await this.localForage.setItem(Constant.currentCompanyId, companyId);
  }

  async removeCurrentCompanyId() {
    await this.localForage.removeItem(Constant.currentCompanyId);
  }

  async getCurrentCompanyId(): Promise<string> {
    return this.localForage.getItem(Constant.currentCompanyId);
  }

  //Company Linked
  async setCompanyLinked(isLinked: boolean) {
    await this.localForage.setItem(Constant.isLinked, isLinked);
  }

  async getCompanyLinked(): Promise<boolean> {
    return this.localForage.getItem(Constant.isLinked);
  }

  // Company details  (NEW) 
  async setCurrentCompanyDetailsNew(companyAC: EntityAC) {
    await this.localForage.setItem(Constant.currentCompanyDetails, companyAC);
  }

  async getCurrentCompanyDetailsNew(): Promise<EntityAC> {
    return this.localForage.getItem(Constant.currentCompanyDetails);
  }

  async removeCurrentCompanyDetails() {
    await this.localForage.removeItem(Constant.currentCompanyDetails);
  }

  // Current logged in user details  (OLD)
  async setCurrentUserDetails(user: UserAC) {
    await this.localForageShared.setItem(Constant.currentUserDetails, user);
  }

  async getCurrentUserDetails(): Promise<UserAC> {
    return this.localForageShared.getItem(Constant.currentUserDetails);
  }

  // Current logged in user details  (NEW)
  async setCurrentUserDetailsNew(user: EntityAC) {
    await this.localForageShared.setItem(Constant.currentUserDetails, user);
  }

  async getCurrentUserDetailsNew(): Promise<EntityAC> {
    return this.localForageShared.getItem(Constant.currentUserDetails);
  }

  async removeCurrentUserDetails() {
    await this.localForageShared.removeItem(Constant.currentUserDetails);
  }

  setCurrentUserName(name: string) {
    this.currentUserNameChange.next(name);
  }

  setCurrentUserEmail(email: string) {
    this.currentUserEmailChange.next(email);
  }

  updateRoute(newroute: string) {
    this.peviousSource.next(newroute);
  }
  updateNextRoute(newroute: string) {
    this.nextSource.next(newroute);
  }

  /* Hide Footer */
  removeFooter(flag: boolean) {
    this.noFooter.next(flag);
  }
  /* Toggle Sidenav */
  toggleSidenav(val: boolean) {
    this.menuMode.next(val);
  }
  /* show Declaration in Loan consent */
  completeDeclaration(flag: boolean) {
    this.showDeclaration.next(flag);
  }
  /* Change mode to view only */
  changeMode(val: boolean) {
    this.viewOnly.next(val);
  }

  /* Change company mode */
  changeCompanyMode(val: boolean) {
    this.isCompanyEditable.next(val);
  }

  // check if the user is authenticated
  isAuthenticated(): Observable<boolean> {
    const token = localStorage.getItem('access_token');
    if (token) {
      return of(true);
    }
    return this.oidcSecurityService.isAuthenticated$.pipe(
      map((isAuthorized: boolean) => {

        if (!isAuthorized) {
          return false;
        }
        return false;
      }),
      tap(() => noop,
        error => false
        ));
  }

  // Method to get section Number
  // This method has reference in transaction and invoice.
  // It's not in use in any other working component as of now
  getSectionNumber(sectionName: string) {
    return 1;
  }

  // Log off
  logoff() {
    this.oidcSecurityService.logoff();
    this.localForage.clear();
    this.removeCurrentUserDetails();
    localStorage.removeItem('access_token');
  }

  // Loan bank details
  async setCurrentLoanApplicationBankDetails(loanApplicationBankDetails: LoanEntityBankDetailsAC) {
    await this.localForage.setItem(Constant.currentLoanBankDetails, loanApplicationBankDetails);
  }

  async getCurrentLoanApplicationBankDetails(): Promise<LoanEntityBankDetailsAC> {
    return this.localForage.getItem<LoanEntityBankDetailsAC>(Constant.currentLoanBankDetails);
  }

  async removeCurrentLoanApplicationBankDetails() {
    await this.localForage.removeItem(Constant.currentLoanBankDetails);
  }

  // Loan Status
  async setCurrentLoanApplicationStatus(status: LoanApplicationStatusType) {
    await this.localForage.setItem(Constant.currentLoanStatus, status);
  }

  async getCurrentLoanApplicationStatus(): Promise<LoanApplicationStatusType> {
    return this.localForage.getItem<LoanApplicationStatusType>(Constant.currentLoanStatus);
  }

  async removeCurrentLoanApplicationStatus() {
    await this.localForage.removeItem(Constant.currentLoanStatus);
  }

  /**
   * Sort the objects by dates of format yyyy-MM-dd.
   * @param object1
   * @param object2
   */
  sortByDate(object1, object2) {
    const dateArrayA = object1.invoiceDate.split('-');
    const dateA = new Date(dateArrayA[2], dateArrayA[1] - 1, dateArrayA[0]);
    const dateArrayB = object2.invoiceDate.split('-');
    const dateB = new Date(dateArrayB[2], dateArrayB[1] - 1, dateArrayB[0]);
    return (dateA <= dateB) ? -1 : 1;
  }

  // Configurations
  /**
   * Method to set all the configurations in local forage.
   * @param Configurations Configuration object
   */
  async setConfigurations(Configurations: ConfigurationAC) {
    await this.localForageShared.setItem(Constant.sectionConfigurations, Configurations.sections);
    await this.localForageShared.setItem(Constant.thirdPartyServiceConfigurations, Configurations.thirdPartyServices);
    await this.localForageShared.setItem(Constant.appSettings, Configurations.appSettings);
  }

  /**
   * Method to get the configurations related to sections.
   * */
  async getSectionConfigurations(): Promise<SectionAC[]> {
    return this.localForageShared.getItem<SectionAC[]>(Constant.sectionConfigurations);
  }

  /**
   * Drop local forage instance.
   * */
  dropInstance() {
    localForage.dropInstance({
      name: window.name
    });
  }

  /**
   * Method to get the configurations related to third party services.
   * */
  async getThirdPartyServiceConfigurations(): Promise<string[]> {
    return this.localForageShared.getItem<string[]>(Constant.thirdPartyServiceConfigurations);
  }

  /**
   * Method to get the configurations related to app settings.
   * */
  async getAppSettings(): Promise<AppSettingAC[]> {
    return this.localForageShared.getItem<AppSettingAC[]>(Constant.appSettings);
  }

  /**
   * Method to remove all the configurations.
   * */
  async removeConfigurations() {
    await this.localForageShared.removeItem(Constant.sectionConfigurations);
    await this.localForageShared.removeItem(Constant.thirdPartyServiceConfigurations);
    await this.localForageShared.removeItem(Constant.appSettings);
  }

  /**
   * Method to sort the list by any integer type field present in object.
   * @param a first object
   * @param b second object
   */
  sortByAnyIntegerField(a, b) {
    if (a.order > b.order) {
      return 1;
    }
    if (a.order < b.order) {
      return -1;
    }
    return 0;
  }

  /**
   * Method to save the values set via calculator.
   * @param values
   */
  async setLoanNeedsValues(values: LoanNeedsValues) {
    await this.localForage.setItem(Constant.loanNeedsValues, values);
  }

  /**
   * Method to get the loan needs value (If) set via calculator.
   * */
  async getLoanNeedsValues() {
    return this.localForage.getItem<LoanNeedsValues>(Constant.loanNeedsValues);
  }

  /**
   * Update the list of entity for dropdown.
   * @param entityList
   */
  updateEntityList(entityList: EntityAC[]) {
    this.entityList.next(entityList);
  }

  /**
   * Update the selected entity in app service.
   * @param event Evenet of ngSelect
   */
  updateSelectedEntity(event: {id: string, name: string}) {
    this.selectedEntity.next(event);
  }

  /**
   * Convert residency status enum to string
   * @param enumNumber enum number
   */
  convertResidencyStatusEnumNumberToString(enumNumber?: number) {
    if (enumNumber === 0) {
      return Constant.usPermanentResident;
    } else if (enumNumber === 1) {
      return Constant.usCitizen;
    } else if (enumNumber === 2) {
      return Constant.nonResident;
    } else {
      return null;
    }
  }

  /**
   * convert residency status string to enum number
   * @param enumString residency status string
   */
  convertStringToResidencyStatusEnum(enumString?: string) {
    if (enumString === Constant.usPermanentResident) {
      return ResidencyStatus.USPermanentResident;
    } else if (enumString === Constant.usCitizen) {
      return ResidencyStatus.USCitizen;
    } else if (enumString === Constant.nonResident) {
      return ResidencyStatus.NonResident;
    } else {
      return null;
    }
  }

  /**
   * Method to set the locked application as JSON string in this.localForage.
   * @param applicationDetail Application details object
   */
  async setLockedApplicationAsJsonString(applicationDetail: ApplicationAC) {
    await this.localForage.setItem(Constant.lockedApplicationJson, JSON.stringify(applicationDetail));
  }

  /**
   * Method to get the locked application JSON as Object.
   * */
  async getLockedApplicationJsonAsObject(): Promise<ApplicationAC> {
    return JSON.parse(await this.localForage.getItem(Constant.lockedApplicationJson));
  }

  /**
   * Method to remove the locked application JSON.
   * */
  async removeLockedApplicationJson() {
    await this.localForage.removeItem(Constant.lockedApplicationJson);
  }

  /**
   * Open selected loan application with setting all the required details in this.localForage and redirect to the current section of the application.
   * @param application Application object
   */
  async openLoanApplication(application: ApplicationBasicDetailAC, isCreditOkay: boolean, setCompanyLinked = true, performRedirection = true) {

    await this.setCurrentLoanApplicationId(application.id);
    await this.setCurrentLoanApplicationNumber(application.loanApplicationNumber);
    await this.setViewOnlyMode(application.isReadOnlyMode);
    if (application.isReadOnlyMode) {
      this.changeMode(true);
    }
    await this.updateCurrentSectionName(application.sectionName);
    await this.setCurrentLoanApplicationStatus(application.status);
    if (application.status === LoanApplicationStatusType.Approved) {
      await this.setCurrentLoanApplicationBankDetails(application.entityBankDetails);
    }
    await this.setCompanyLinked(true);
    this.setProgressBar(application.sectionName);
    if (performRedirection) {
      this.redirectToSection(application.sectionName);
    }
  }

  /**
   * Method to set the progress bar according to the section of the loan application.
   * @param sectionName Current section name
   */
  async setProgressBar(sectionName: string) {
    if (sectionName.includes(Constant.companyInfo)) {
      await this.setCompanyLinked(false);
      await this.updateProgressbar(Constant.companyInfoProgressBar);
    } else if (sectionName.includes(Constant.finances) || sectionName.includes(Constant.company)) {
      await this.updateProgressbar(Constant.financesProgressBar);
    } else if (sectionName.includes(Constant.invoices)) {
      await this.updateProgressbar(Constant.financesProgressBar);
    } else if (sectionName.includes(Constant.transactions)) {
      await this.updateProgressbar(Constant.financesProgressBar);
    } else if (sectionName.includes(Constant.taxes)) {
      await this.updateProgressbar(Constant.taxesProgressBar);
    } else if (sectionName.includes(Constant.loanProduct)) {
      await this.updateProgressbar(Constant.loanProductProgressBar);
    } else if (sectionName.includes(Constant.loanConsent)) {
      await this.updateProgressbar(Constant.loanConsentProgressBar);
    } else if (sectionName.includes(Constant.loanStatus) || sectionName.includes(Constant.bankDetails)) {
      await this.updateProgressbar(Constant.loanStatusProgressBar);
    } else if (sectionName.includes(Constant.personal)) {
      await this.updateProgressbar(Constant.financesProgressBar);
    } else if (sectionName.includes(Constant.additionalDocuments)) {
      await this.updateProgressbar(Constant.additionalDocumentsProgressBar);
    }
  }

  /**
   * Method redirects user to the respective section
   * @param sectionName Current section name
   */
  redirectToSection(sectionName: string) {
    // Append remaining sections as per thier routes
    if (sectionName === Constant.companyInfo) {
      this.router.navigate([Constant.companyInfoRedirectUrl]);
    } else if (sectionName === Constant.finances || sectionName === Constant.company) {
      this.router.navigate([Constant.financesRedirectUrl]);
    } else if (sectionName === Constant.invoices) {
      this.router.navigate([Constant.invoicesRedirectUrl]);
    } else if (sectionName === Constant.transactions) {
      this.router.navigate([Constant.transactionsRedirectUrl]);
    } else if (sectionName === Constant.taxes) {
      this.router.navigate([Constant.taxesReturnsRedirectUrl]);
    } else if (sectionName === Constant.loanProduct) {
      this.router.navigate([Constant.loanProductRedirectUrl]);
    } else if (sectionName === Constant.loanConsent) {
      this.router.navigate([Constant.loanConsentRedirectUrl]);
    } else if (sectionName === Constant.loanStatus) {
      this.router.navigate([Constant.loanStatusRedirectUrl]);
    } else if (sectionName === Constant.loanNeeds) {
      this.router.navigate([Constant.loanNeedsRedirectUrl]);
    } else if (sectionName === Constant.bankDetails) {
      this.router.navigate([Constant.bankDetailsRedirectUrl]);
    } else if (sectionName === Constant.personal) {
      this.router.navigate([Constant.personalFinancesRedirectUrl]);
    } else if (sectionName === Constant.additionalDocuments) {
      this.router.navigate([Constant.additionalDocumentsRedirectUrl]);
    }else {
      this.router.navigate(['']);
    }
  }

  /**
   * Method to remove all the data from localForage.
   * */
  async removeAllDataFromLocalForage() {
    await this.removeCurrentLoanApplicationId();
    await this.removeCurrentLoanApplicationBankDetails();
    await this.removeCurrentLoanApplicationStatus();
    await this.removeCurrentLoanApplicationNumber();
    await this.removeCurrentSectionName();
    await this.removeCurrentCompanyDetails();
    await this.removeCurrentCompanyId();
    await this.setCompanyLinked(false);
    await this.updateProgressbar(0);
    await this.setViewOnlyMode(false);
    await this.removeFinanceMappingInProgress();
    this.completeDeclaration(false);
  }



  /**
   * Method to set selected theme
   * @param applicationDetail Application details object
   */
  async setSelectedTheme(theme) {
    await this.localForageShared.setItem(Constant.selectedTheme, JSON.stringify(theme));
  }

  /**
   * Method to get the locked application JSON as Object.
   * */
  async getSelectedTheme() {
    return JSON.parse(await this.localForageShared.getItem(Constant.selectedTheme));
  }

  /**
   * Method to remove the locked application JSON.
   * */
  async removeSelectedTheme() {
    await this.localForage.removeItem(Constant.selectedTheme);
  }

  /**
   * Method to upload file to specified presigned URL
   * @param fileToUpload file to upload
   * @param uploadPreSignedUrl PreSigned URL
   */
  uploadFile(fileToUpload: File, uploadPreSignedUrl: string): Observable<Object> {
    const _formData = new FormData();
    _formData.append(Constant.file, fileToUpload, fileToUpload.name);
    const headers = new HttpHeaders({
      'Content-Type': Constant.contentTypeOctetStream,
      'x-amz-server-side-encryption': Constant.AWSEncryptionMethod
    });
    return this.httpClient.put(uploadPreSignedUrl, _formData, { headers });
  }

  // set is credit okay
  async setIsCreditOkay(isCreditOkay: boolean) {

    await this.localForage.setItem(Constant.isCreditOkay, isCreditOkay);
  }

  // get is credit okay
  async getIsCreditOkay(): Promise<boolean> {
    return this.localForage.getItem(Constant.isCreditOkay);
  }

  // set state param
  async setFinanceStateParam(state: string) {
    await this.localForage.setItem(Constant.state, state);
  }

  // get state param
  async getFinanceStateParam(): Promise<string> {
   return this.localForage.getItem(Constant.state);
  }

  // remove state param
  async removeFinanceStateParam() {
    this.localForage.removeItem(Constant.state);
  }

  // set code param
  async setFinanceCodeParam(code: string) {
    await this.localForage.setItem(Constant.code, code);
  }

  // get code param
  async getFinanceCodeParam(): Promise<string> {
      return this.localForage.getItem(Constant.code);
  }

  // remove code param
  async removeFinanceCodeParam() {
      await this.localForage.removeItem(Constant.code);
  }

  // set realmId param
  async setRealmIdParam(realmId: string) {
    
    await this.localForage.setItem(Constant.realmId, realmId);
  }

  // get realmId param
  async getRealmIdParam(): Promise<string> {
      return this.localForage.getItem(Constant.realmId);
  }

  // remove realmId param
  async removeRealmIdParam() {
      await this.localForage.removeItem(Constant.realmId);
  }

  // set openCalculator
  async setOpenCalculator(openCalculator: boolean) {
      await this.localForage.setItem(Constant.openCalculator, openCalculator);
  }

  // get openCalculator
  async getOpenCalculator(): Promise<boolean> {
      return this.localForage.getItem(Constant.openCalculator);
  }

  // Remove all 
  async resetCurrentLocalForage() {
    
      this.localForage.clear();
      await this.updateProgressbar(0);
    }

  public deepCopy<T>(source: T): T {
    return Array.isArray(source)
      ? source.map(item => this.deepCopy(item))
      : source instanceof Date
        ? new Date(source.getTime())
        : source && typeof source === 'object'
          ? Object.getOwnPropertyNames(source).reduce((o, prop) => {
            Object.defineProperty(o, prop, Object.getOwnPropertyDescriptor(source, prop));
            o[prop] = this.deepCopy(source[prop]);
            return o;
          }, Object.create(Object.getPrototypeOf(source)))
          : source;
  }
}
