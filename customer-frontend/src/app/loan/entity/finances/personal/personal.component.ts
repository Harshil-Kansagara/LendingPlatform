import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Constant } from '../../../../shared/constant';
import { BsModalRef, BsModalService, ModalDirective } from 'ngx-bootstrap/modal';
import {
  ApplicationService, EntityService, FilterAC, LoanApplicationStatusType,
  PersonalFinanceAC, PersonalFinanceAttributeFieldType, PersonalFinanceCategoryAC, ProblemDetails, ResourceType
} from '../../../../utils/serviceNew';
import { AppService } from '../../../../services/app.service';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { FieldSetList } from './fieldSetList.modal';

@Component({
  selector: 'app-personal',
  templateUrl: './personal.component.html',
})
export class PersonalComponent implements OnInit {
  modalRef: BsModalRef;
  selectedCategory: PersonalFinanceCategoryAC;
  showObligationComponent: number;
  realestate: string;
  constructor(private readonly modalService: BsModalService,
    private readonly entityService: EntityService,
    private readonly appService: AppService,
    private readonly toastrService: ToastrService,
    private readonly applicationService: ApplicationService,
    private readonly router: Router) { }
  nextRoute: string;
  backRoute: string;
  extraThingsQuestion: string;
  continue: string;
  userId: string;
  loanApplicationId: string;
  showFinances = false;
  isProprietor: boolean;
  currentSectionName: string;
  currencySymbol: string;
  @ViewChild(ModalDirective, { static: false }) modal: ModalDirective;
  @ViewChild('template') template;
  showLoader = true;
  hasAnyAnswers = false;
  showModalLoader = false;
  showSummaryModal = false;
  summaryFinances: PersonalFinanceAC;
  isLocked = false;
  attributeFieldType: typeof PersonalFinanceAttributeFieldType = PersonalFinanceAttributeFieldType;

  //Category names
  creditCards = Constant.creditCards;
  realEstate = Constant.realEstate;
  mortgageLoans = Constant.mortgageLoans;
  automobiles = Constant.autoMobile;
  checking = Constant.checking;
  savings = Constant.savings;
  brokerage = Constant.brokerages;
  retirement = Constant.retirement;
  lifeInsurance = Constant.lifeInsurance;
  receivables = Constant.receivables;
  yourObligations = Constant.receivables;
  otherLoans = Constant.otherLoans;
  unpaidTaxes = Constant.unpaidTaxes;
  incomeInformation = Constant.incomeInformation;
  installmentLoans = Constant.installmentLoans;
  personalProperty = Constant.personalProperty;
  parentCategory: PersonalFinanceCategoryAC;
  isConsent = false;

  config = {
    backdrop: true,
    ignoreBackdropClick: true,
    class: 'gray modal-sm report-container modal-dialog-large summary-modal'
  };
  personalFinances: PersonalFinanceAC;

  /**
   * Method to open Assets modals 
   */
  openPersonalFinancesModal(template, category: PersonalFinanceCategoryAC) {
    this.parentCategory = new PersonalFinanceCategoryAC();
    this.parentCategory = this.getParentOfSelectedCategory(category);
    this.showSummaryModal = false;
    this.modalRef = this.modalService.show(template, this.config);
    this.selectedCategory = category;
  }

  getParentOfSelectedCategory(category: PersonalFinanceCategoryAC) {
    if (!this.isLocked) {
      for (const allAccounts of this.personalFinances.accounts) {
        for (const theCategory of allAccounts.categories) {
          if (theCategory.childCategories && theCategory.childCategories.some(x => x.name === category.name)) {
            return theCategory;
          }
        }
      }
    }
  }

  async ngOnInit() {
    if (this.router.url.includes('consent')) {
      this.isConsent = true;
    } else {
      this.isConsent = false;
    }
    this.personalFinances = new PersonalFinanceAC();
    this.nextRoute = Constant.taxesReturnsRedirectUrl;
    this.backRoute = Constant.financesRedirectUrl;
    this.extraThingsQuestion = Constant.extraThingsQuestion;
    this.continue = Constant.continue;

    const appSettings = await this.appService.getAppSettings();

    //If the app settings are present then only set the required UI properties with their values.
    if (appSettings.length !== 0) {
      this.currencySymbol = appSettings.filter(x => x.fieldName === 'Currency:Symbol')[0].value;
    }
    this.userId = (await this.appService.getCurrentUserDetailsNew()).id;
    this.currentSectionName = await this.appService.getCurrentSectionName();
    this.loanApplicationId = await this.appService.getCurrentLoanApplicationId();
    // Get current company structure
    const entityId = await this.appService.getCurrentCompanyId();
    const filterAC = new FilterAC();
    filterAC.field = 'type';
    filterAC.operator = '=';
    filterAC.value = 'company';
    const filters = new Array<FilterAC>();
    filters.push(filterAC);
    if (entityId) {
      this.entityService.getEntityList(null, null, null, JSON.stringify(filters), null, null).subscribe(
        res => {
          const currentCompany = res.filter(x => x.id === entityId)[0].company;
          if (currentCompany.companyStructure.structure === Constant.proprietorship) {
            this.isProprietor = true;
          }
          this.getFinancialReports();
        },
        (err: ProblemDetails) => {
          if (err.status === Constant.badRequest) {
            this.toastrService.error(err.detail);
          }
        });
    }
  }

  // Method to check if category has answers
  checkIfCategoryIsFilled(category: PersonalFinanceCategoryAC) {
    return category.attributes && category.attributes.some(x => x.answer);
  }

  // Method to fetch personal finances from backend
  getPersonalFinances(resourceType: ResourceType, navigateToNewCategory: boolean = null, selectedCategory: number = null) {
    this.showLoader = true;
    if (resourceType === ResourceType.Company) {
      this.entityService.getPersonalFinances(this.userId, 'details').subscribe(finances => {
        this.personalFinances = finances;
        this.getPersonalFinancesSubscriptionHandler(navigateToNewCategory, selectedCategory);
      });
    } else if (resourceType === ResourceType.Loan) {
      this.applicationService.getPersonalFinances(this.loanApplicationId, 'details').subscribe(entityAC => {
        this.personalFinances = entityAC.filter(x => x.id === this.userId)[0].personalFinance;
        this.getPersonalFinancesSubscriptionHandler(navigateToNewCategory, selectedCategory);
      });
    }
  }

  // Method to show/hide proprietor question
  addProprietoryFinance(showFinances) {
    this.showFinances = showFinances;
    if (this.showFinances) {
      this.getFinancialReports();
    }
  }

  // Method that handles click of continue button and showing Summary modal
  submitPersonalFinances() {
    if (this.isLocked) {
      this.applicationService.getPersonalFinances(this.loanApplicationId, 'summary').subscribe(entity => {
        this.summaryFinances = entity.filter(x => x.id === this.userId)[0].personalFinance;
        this.showFilledCategoriesInViewOnlyMode(true);
        this.summaryViewSubscriptionHandler();
      });
    } else {
      if (!this.showFinances) {
        this.redirectToNextSection();
      } else {
        if ((!this.showFinances && this.isProprietor) ||
          this.personalFinances.accounts.some(x => x.categories.some(y => y.attributes && y.attributes.length
            && y.attributes[0].childAttributeSets.some(z => z.childAttributes.some(a => a.answer))))) {
          this.entityService.getPersonalFinances(this.userId, 'summary').subscribe(finances => {
            this.summaryFinances = finances;
            this.summaryViewSubscriptionHandler();
          });
        } else {
          this.toastrService.error(Constant.fillAtLeastOneInformation);
        }
      }
    }
  }

  // Method that handles response mapping of summary view
  summaryViewSubscriptionHandler() {
    if (!this.isConsent) {
      this.modalRef = this.modalService.show(this.template, this.config);
    }
    this.showSummaryModal = true;
  }

  // Method that handles click of continue button inside summary modal
  redirectToNextSection() {
    if (this.modalRef) {
      this.modalRef.hide();
    }
    // If proprietor has selected NO in question OR there are answers submitted, redirect to next section
    if ((!this.showFinances && this.isProprietor) ||
      this.personalFinances.accounts.some(x => x.categories.some(y => y.attributes[0]?.childAttributeSets.some(z => z.childAttributes.some(a => a.answer))))) {
      if (this.currentSectionName === Constant.company || this.currentSectionName === Constant.personal) {
        this.applicationService.updateCurrentSectionName(this.loanApplicationId, this.currentSectionName).subscribe(
          async (updatedSectionName: string) => {
            if (updatedSectionName != null) {
              await this.appService.updateCurrentSectionName(updatedSectionName);
              await this.appService.updateProgressbar(Constant.taxesProgressBar);
              this.router.navigate([this.nextRoute]);
            }
          }
        );
      } else {
        this.router.navigate([this.nextRoute]);
      }
    } else {
      this.toastrService.error(Constant.fillAtLeastOneInformation);
    }
  }

  // Method that handles submit button click of all category popups
  saveModalInfo(savedCategory: FieldSetList, selectedCategoryNumber: number) {
    this.showModalLoader = true;
    this.entityService.addPersonalFinances(this.userId, savedCategory.savedCategory)
      .subscribe(res => {
        this.toastrService.success(Constant.detailsSavedSuccessfully);
        this.getPersonalFinancesSubscriptionHandler(savedCategory.navigateToNextCategory, selectedCategoryNumber);
      }, (err: ProblemDetails) => {
        if (err && err.detail) {
          this.toastrService.error(err.detail);
          this.showModalLoader = false;
        }
      });
  }

  // Method that decides which personal finance to fetch
  async getFinancialReports(navigateToNewCategory: boolean = null, selectedCategory: number = null) {
    this.showLoader = true;

    // If the application of the current loan application is locked then use JSON of its details
    // otherwise make the backend call to get the details.

    // Get the locked application object from JSON stored in localForage. If the JSON not found in localForage
    // then only make the backend call to get the application details.
    const lockedApplication = await this.appService.getLockedApplicationJsonAsObject();
    const isViewOnlyMode = await this.appService.isViewOnlyMode();
    if (lockedApplication && await this.appService.getCurrentLoanApplicationStatus() !== LoanApplicationStatusType.Draft) {
      this.isLocked = true;
      this.getPersonalFinances(ResourceType.Loan, navigateToNewCategory, selectedCategory);
    } else {
      if (isViewOnlyMode) {
        this.applicationService.getLoanApplicationDetailsById(this.loanApplicationId).subscribe(resp => {
          if (resp.basicDetails.createdByUserId === this.userId) {
            this.isLocked = true;
          }
          // Get from backend
          this.getPersonalFinances(ResourceType.Company, navigateToNewCategory, selectedCategory);
        });
      }
      else {
        // Get from backend
        this.getPersonalFinances(ResourceType.Company, navigateToNewCategory, selectedCategory);
      }
    }
  }

  // Method that binds personal finance response with view
  getPersonalFinancesSubscriptionHandler(navigateToNewCategory: boolean = null, selectedCategory: number = null) {

    if (this.isLocked) {
      this.showFilledCategoriesInViewOnlyMode(false);
    }
    this.personalFinances.accounts.sort((a, b) => a.order - b.order);
    for (const finance of this.personalFinances.accounts) {
      finance.categories.sort((a, b) => a.order - b.order);
    }
    this.hasAnyAnswers = this.personalFinances.accounts.some(x => x.categories.some(y => y.attributes && y.attributes.length > 0
      && (y.attributes[0].childAttributeSets.some(z => z.childAttributes.some(a => a.answer) || y.attributes[0].answer))));

    // if answers are already there, then super question by default will become yes for proprietor OR show finances without super question
    if (!this.isProprietor || this.hasAnyAnswers) {
      this.showFinances = true;
    }

    console.log(this.personalFinances);

    if (navigateToNewCategory !== null && navigateToNewCategory !== undefined) {
      this.modalRef.hide();
      this.modalService.onHidden.subscribe(x => this.showModalLoader = false);
    }
    if (navigateToNewCategory && navigateToNewCategory === true && selectedCategory && selectedCategory > 0) {
      let nextCategory = this.personalFinances.accounts.filter(x => x.name === 'Assets')[0].categories.filter(x => x.order === selectedCategory + 1)[0];
      if (!nextCategory) {
        nextCategory = this.personalFinances.accounts.filter(x => x.name === 'Obligations')[0].categories.filter(x => x.order === selectedCategory + 1)[0];
      }
      this.openPersonalFinancesModal(this.template, nextCategory);
    }
    this.showLoader = false;
    if (this.isConsent) {
      this.submitPersonalFinances();
    }
  }

  // Method that handles showing of filled categories only in view only mode
  showFilledCategoriesInViewOnlyMode(summaryView: boolean) {
    const filledCategories = new Array<string>();
    const filledAccounts = new Array<string>();
    this.personalFinances.accounts = this.personalFinances.accounts.filter(x => x.categories.some(x => x.attributes.some(y => y.answer)));
    this.personalFinances.accounts.forEach(x => filledAccounts.push(x.name));
    for (const account of this.personalFinances.accounts) {
      account.categories = account.categories.filter(x => x.attributes.some(y => y.answer));
      account.categories.forEach(x => filledCategories.push(x.name));
    }
    if (summaryView) {
      this.summaryFinances.summary.accounts = this.summaryFinances.summary.accounts.filter(x => filledAccounts.includes(x.name));
      for (const account of this.summaryFinances.summary.accounts) {
        account.categories = account.categories.filter(x => filledCategories.includes(x.name));
      }
    }
  }

  closeModal(modalRef) {
    this.getFinancialReports();
    modalRef.hide();
  }

  openParentModal(parentCategory: PersonalFinanceCategoryAC) {
    this.modalRef.hide();
    this.openPersonalFinancesModal(this.template, parentCategory);
  }

  checkIfParentCategoryIsFilled(category: PersonalFinanceCategoryAC) {
    const parentCategory = this.getParentOfSelectedCategory(category);
    if (parentCategory) {
      if (parentCategory.attributes && parentCategory.attributes.length > 0
        && (parentCategory.attributes[0].childAttributeSets.some(z => z.childAttributes.some(a => a.answer)
          || parentCategory.attributes[0].answer === 'true'))) {
        return false;
      } else {
        return true;
      }
    } else {
      return false;
    }
  }
}
