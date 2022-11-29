import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { AppService } from '../../../../services/app.service';
import { EventEmitterService } from '../../../../services/event-emitter.service';
import { Constant } from '../../../../shared/constant';
import { ToastrService } from 'ngx-toastr';
import { timer } from 'rxjs';
import { BsModalRef, ModalDirective, BsModalService } from 'ngx-bootstrap/modal';
import {
  ApplicationService, EntityService, CompanyFinanceAC, ThirdPartyServiceCallbackDataAC,
  LoanApplicationStatusType, FilterAC, ProblemDetails, ResourceType
} from '../../../../utils/serviceNew';
import { switchMap } from 'rxjs/operators';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-finances',
  templateUrl: './statements.component.html',
  styleUrls: ['./statements.component.scss']
})
export class StatementsComponent implements OnInit {
  ifComingSoon:boolean;
  bsModal: BsModalRef;
  financialInfoTitle = Constant.financialInfoTitle;
  @ViewChild(ModalDirective, { static: false }) modal: ModalDirective;
  retrieveHeading = Constant.retrieveHeading;
  retrieveInfo = Constant.retrieveInfo;
  accountFirmHeading = Constant.accountFirmHeading;
  availableSummaryHeading = Constant.availableSummaryHeading;
  financialStatements: string[] = [Constant.incomeStatement, Constant.balanceSheet, Constant.cashFlow, Constant.financialRatio];
  statement: string;
  services: string[];
  keyRatios: string = Constant.keyRatios;
  companyName: string;
  companyFiscalYearStartMonth: string;
  companyFiscalYearStartMonthDays: number;
  mappedData = [];
  endPeriod: string;
  showLoader: boolean;
  connected = true;
  authUrl: string = null;
  loanApplicationId: string;
  entityId: string;
  showAsyncMessage: boolean;
  statementCsv = `${Constant.incomeStatement},${Constant.balanceSheet},${Constant.cashFlow},${Constant.financialRatio}`;
  showFinancialSummary: boolean;
  financeReports: Array<CompanyFinanceAC>;
  periodList: Array<string>;
  connectedService: string;
  backRoute: string = Constant.companyInfoRedirectUrl;
  statementName: string;
  hasFinancesAlready = false;
  issueWithFetchingFinances = false;
  currentSectionName: string;
  financeRetrievalDate: Date;
  constructor(
    private readonly appService: AppService,
    private readonly eventEmitterService: EventEmitterService,
    private readonly toastrService: ToastrService,
    private readonly entityService: EntityService,
    private readonly applicationService: ApplicationService,
    private readonly router: Router,
    private readonly modalService: BsModalService) {

  }

  async ngOnInit(): Promise<void> {
    this.currentSectionName = await this.appService.getCurrentSectionName();
    this.showAsyncMessage = false;
    await this.getLocalForageValues();
    this.eventEmitterService.clearButtonClickedEvent.subscribe((reset: boolean) => {
      if (reset) {
        this.connected = !this.connected;
        this.connectedService = null;
        this.showAsyncMessage = false;
        this.showFinancialSummary = false;
      }
    });

  }




  // Assign/Fetch necessary local storage values into local object.
  async getLocalForageValues(): Promise<void> {
    this.services = (await this.appService.getThirdPartyServiceConfigurations()).filter(x => x === 'Quickbooks' || x === 'Xero');
    this.services = this.services[0] === 'Xero' ?
      this.services.reverse() :
      this.services;
    this.loanApplicationId = await this.appService.getCurrentLoanApplicationId();

    this.entityId = await this.appService.getCurrentCompanyId();
    const filterAC = new FilterAC();
    filterAC.field = 'type';
    filterAC.operator = '=';
    filterAC.value = 'company';
    const filters = new Array<FilterAC>();
    filters.push(filterAC);
    if (this.entityId) {
      this.entityService.getEntityList(null, null, null, JSON.stringify(filters), null, null).subscribe(
        res => {
          this.companyName = res.filter(x => x.id === this.entityId)[0].company.name;
          const twelve = 12;
          if (res.filter(x => x.id === this.entityId)[0].company.companyFiscalYearStartMonth !== null
            && res.filter(x => x.id === this.entityId)[0].company.companyFiscalYearStartMonth !== 1) {
            this.companyFiscalYearStartMonth = Constant.months
              .filter(y => y.number === res.filter(x => x.id === this.entityId)[0].company.companyFiscalYearStartMonth - 1)[0].name;
            this.companyFiscalYearStartMonthDays = Constant.months
              .filter(y => y.number === res.filter(x => x.id === this.entityId)[0].company.companyFiscalYearStartMonth - 1)[0].days;
          } else {
            this.companyFiscalYearStartMonth = Constant.months.filter(x => x.number === twelve)[0].name;
            this.companyFiscalYearStartMonthDays = Constant.months.filter(x => x.number === twelve)[0].days;
          }
          this.getFinancialReports();
        },
        (err: ProblemDetails) => {

          if (err.status === Constant.badRequest)
            this.toastrService.error(err.detail);
        });
    }
   


  }

  // Get the financial reports
  async getFinancialReports() {
    this.showLoader = true;

    // If the application of the current loan application is locked then use JSON of its details
    // otherwise make the backend call to get the details.

    // Get the locked application object from JSON stored in localForage. If the JSON not found in localForage
    // then only make the backend call to get the application details.
    const lockedApplication = await this.appService.getLockedApplicationJsonAsObject();
    if (lockedApplication && await this.appService.getCurrentLoanApplicationStatus() !== LoanApplicationStatusType.Draft) {
      this.fetchFinancesFromBackend(ResourceType.Loan);
    } else {

      // Get from backend
      this.fetchFinancesFromBackend(ResourceType.Company);
    }
  }

  

  // Fetch finances from backend
  fetchFinancesFromBackend(resourceType: ResourceType) {
    if (resourceType === ResourceType.Company) {
      this.entityService.getCompanyFinances(this.entityId, this.statementCsv)
        .subscribe(async (entityFinances: CompanyFinanceAC[]) => {
          this.fetchFinanceSubscriptionHandler(entityFinances);
        },
          err => {
            this.fetchFinanceErrorHandler();
          });
    }
    else {
      this.applicationService.getFinances(this.loanApplicationId, this.statementCsv)
        .subscribe(async (entityFinances: CompanyFinanceAC[]) => {
          this.fetchFinanceSubscriptionHandler(entityFinances);
        },
          err => {
            this.fetchFinanceErrorHandler();
          });
    }
    
  }

  // Actions to perform when there is error in finance subscription
  fetchFinanceErrorHandler() {
    this.showLoader = false;
    // If the finances are not fetched altogether (no record in finances), show connect items
    this.connected = false;
    this.hasFinancesAlready = false;
    this.issueWithFetchingFinances = false;
  }

  // Actions to perform when response comes for finance subscription
  async fetchFinanceSubscriptionHandler(entityFinances) {
    // If the finances are already fetched but mapping is still in progress
    this.connected = true;
    if (entityFinances.some(x => !x.isChartOfAccountMapped) && entityFinances.some(x => !x.isDataEmpty)) {
      this.showAsyncMessage = true;
      this.connectedService = entityFinances[0].thirdPartyServiceName;
      this.showFinancialSummary = false;
      await this.appService.setFinanceMappingInProgress(true);
    } else if (entityFinances && entityFinances.length > 0 && entityFinances[0].isChartOfAccountMapped && entityFinances.some(x => !x.isDataEmpty)) {
      await this.financesFetchedAndMappedAsync(entityFinances);
    } else if (entityFinances.some(x => x.isDataEmpty)) {
      this.connectedService = null;
      this.hasFinancesAlready = false;
      this.issueWithFetchingFinances = true;
    }
    //// Stop the loader.
    this.showLoader = false;
  }

  //If the finances are fetched as well as mapped
  async financesFetchedAndMappedAsync(entityFinances) {
    this.financeReports = entityFinances;
    this.showAsyncMessage = false;
    this.showFinancialSummary = true;

    await this.appService.setFinanceMappingInProgress(false);
    // If this is not the first loan for that company and the loan is new
    if (this.currentSectionName === Constant.finances || this.currentSectionName === Constant.company) {
      this.connectedService = null;
      this.hasFinancesAlready = true;
      this.issueWithFetchingFinances = false;
      this.financeRetrievalDate = this.financeReports[0].creationDateTime;
    } else {
      this.connectedService = this.financeReports[0].thirdPartyServiceName;
      this.hasFinancesAlready = false;
      this.issueWithFetchingFinances = false;
    }
    const thirdPartyWiseCompanyName = this.financeReports[0].thirdPartyWiseCompanyName;
    if (thirdPartyWiseCompanyName) {
      this.companyName = thirdPartyWiseCompanyName;
    }
  }

  // Connect to service
  async connectToService(service) {

    this.showLoader = true;
    // Get third party service's auth url
    this.entityService.getAuthUrl(this.entityId, service).subscribe(
      (data) => {
        this.authUrl = data;
        if (this.authUrl !== null) {
          const winObj = window.open(this.authUrl, 'newwindow', 'width=600,height=424');
          winObj.parent.name = window.name;
          const self = this;
          const loop = setInterval(function () {
            if (winObj.closed) {
              clearInterval(loop);
              self.redirectHandler(service);
            }
          }, 1000);

        }
      }, (err) => {
      });

  }

  // Handle third party service's redirection flow
  async redirectHandler(service) {
    this.connectedService = service;
    const code = await this.appService.getFinanceCodeParam();
    const realmId = await this.appService.getRealmIdParam();
    const state = await this.appService.getFinanceStateParam();

    if (code && state) {
      const callbackData = new ThirdPartyServiceCallbackDataAC();

      callbackData.authorizationCode = code;
      callbackData.realmId = realmId;
      callbackData.csrfToken = state;
      callbackData.thirdPartyServiceName = service;

      // For two statements send request one after other
      this.entityService.addCompanyFinances(this.entityId, this.statementCsv, callbackData).subscribe(
        async res => {
          // Start polling
          this.showLoader = false;
          this.showAsyncMessage = true;
          this.connected = true;
          this.showFinancialSummary = false;
          await this.appService.removeFinanceCodeParam();
          await this.appService.removeFinanceStateParam();
          await this.appService.removeRealmIdParam();

          await this.appService.setFinanceMappingInProgress(true);
          const resetTimer = 3000;
          const startTime = new Date().getTime();
          const timedSubscription = timer(0, resetTimer).pipe(
            switchMap(_ => this.entityService.getCompanyFinances(this.entityId, this.statementCsv))
          ).subscribe(async res => {
            this.showLoader = false;
            const sixtyThousand = 60000;
            if (res && res.length > 0 && res[0].isChartOfAccountMapped) {
              timedSubscription.unsubscribe();
              await this.appService.setFinanceMappingInProgress(false);
              this.showAsyncMessage = false;
              this.showFinancialSummary = true;
              this.financeReports = res;
              const thirdPartyWiseCompanyName = this.financeReports[0].thirdPartyWiseCompanyName;
              if (thirdPartyWiseCompanyName) {
                this.companyName = thirdPartyWiseCompanyName;
              }
            } else if (((new Date().getTime()) - startTime) / sixtyThousand >= environment.defaultFinanceWaitingTime) {
              timedSubscription.unsubscribe();
              await this.appService.setFinanceMappingInProgress(false);
              this.showAsyncMessage = false;
              this.connectedService = null;
              this.hasFinancesAlready = false;
              this.issueWithFetchingFinances = true;
            }
          });
        });
    }
    else {
      this.connected = false;
      this.showLoader = false;
      this.connectedService = null;
      this.showAsyncMessage = false;
      this.showFinancialSummary = false;
    }

  }


  //Method to open modal of statements
  async showModal(statement) {
    if (this.financeReports) {
      this.statement = statement;
      this.statementName = statement;
      if (this.statement === Constant.cashFlow) {
        this.statementName = 'Cash Flow Statement';
      }
      else if (this.statement === Constant.financialRatio) {
        this.statementName = Constant.keyRatios;
      }
      this.mappedData = this.financeReports.filter(x => x.financialStatement === statement)[0].standardAccountList;
      this.endPeriod = this.financeReports[0].endPeriod;
      const periodList = this.financeReports.find(x => x.financialAccounts).financialAccounts.map(x => x.period);
      this.periodList = periodList;
      this.modal.show();

    }

  }


  // Change section and redirect
  async redirectToNextSection() {


    if (this.currentSectionName === Constant.finances || this.currentSectionName === Constant.company) {
      this.applicationService.updateCurrentSectionName(this.loanApplicationId, this.currentSectionName).subscribe(
        async (updatedSectionName: string) => {
          if (updatedSectionName != null) {
            await this.appService.updateCurrentSectionName(updatedSectionName);
            await this.appService.updateProgressbar(Constant.taxesProgressBar);
            this.router.navigate([Constant.personalFinancesRedirectUrl]);
          }
        }
      );
    } else {
      this.router.navigate([Constant.personalFinancesRedirectUrl]);
    }
  }

  // Handler of interstial page response
  existingFinancesAction(reuseExistingFinances: boolean) {
    this.hasFinancesAlready = false;
    this.issueWithFetchingFinances = false;
    if (!reuseExistingFinances) {
      this.eventEmitterService.resetFinances();
    }else {
      this.connectedService = this.financeReports[0].thirdPartyServiceName;
    }
  }

  // Coming Soon MOdal
  modalRef: BsModalRef;
  openComingSoonModal(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, Object.assign({}, { class: 'modal-dialog-centered' }));
  }

}
