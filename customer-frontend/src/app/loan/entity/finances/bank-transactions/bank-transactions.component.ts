import { Component, OnInit, Input, TemplateRef, OnDestroy } from '@angular/core';
import { Constant } from '../../../../shared/constant';
import { AppService } from '../../../../services/app.service';
import {
  CompanyInfoService, CompanyAC, ProviderBankAC, BankAccountTransactionAC, EntityBankAccountsMappingAC,
  YodleeFastLinkAC, ProviderBankListAC, TransactionInformationFrom
} from '../../../../utils/service';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { EventEmitterService } from '../../../../services/event-emitter.service';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-bank-transactions',
  templateUrl: './bank-transactions.component.html',
  styleUrls: ['./bank-transactions.component.scss']
})
export class BankTransactionsComponent implements OnInit, OnDestroy {

  loanApplicationId: string;
  entityId: string;
  loader: boolean = false;
  transactionsArray = [];
  accountType: string;
  currentBalance: string;
  modalRef: BsModalRef;
  isCleared: boolean = false;
  startingYear: string;
  endingYear: string;
  subsVar: Subscription;
  currentSectionName: string;
  defaultTransactionInformationFrom: TransactionInformationFrom;

  @Input() linkedBanks: ProviderBankAC[] = [];
  @Input() linkedBankAccountTransactions: BankAccountTransactionAC[] = [];
  @Input() connected: boolean = true;
  @Input() consentVisible = true;
  @Input() viewOnly = true;

  constructor(private readonly appService: AppService,
    private readonly companyInfoService: CompanyInfoService,
    private readonly modalService: BsModalService,
    private readonly eventEmitterService: EventEmitterService,
    private readonly router: Router,
    private readonly toastrService: ToastrService) {
    this.appService.updateLoader(false);
    this.appService.updateRoute(Constant.invoicesRedirectUrl);
    this.appService.updateNextRoute(Constant.taxesReturnsRedirectUrl);
  }

  async ngOnInit() {
    if (this.router.url.search(Constant.transactions.toLowerCase()) !== -1) {
      this.disableSaveButton();
      this.currentSectionName = await this.appService.getCurrentSectionName();
      this.loanApplicationId = await this.appService.getCurrentLoanApplicationId();
      this.entityId = new CompanyAC(await this.appService.getCurrentCompanyDetails()).id;
      this.subsVar = this.eventEmitterService.
        invokeSaveFunctionInTransactions.subscribe(() => {
          this.router.navigate([Constant.taxesReturnsRedirectUrl]);
        });
      this.initializeBanks();
    }
    if (this.linkedBankAccountTransactions.length !== 0) {
      this.selectAccount(this.linkedBankAccountTransactions[0]);
    }
  }
  /**
   * Load default provider script.
   * */
  loadProviderScript(): void {
    const transactionJsUrl = this.fetchTransactionJsUrl();
    const scriptHtmlTag = document.createElement('script');
    scriptHtmlTag.type = 'text/javascript';
    scriptHtmlTag.src = transactionJsUrl;
    document.getElementById('transactionPop-Up').append(scriptHtmlTag);
  }
  /**
   * Fetch the transaction javascript url.
   * */
  fetchTransactionJsUrl(): string {
    return this.defaultTransactionInformationFrom === TransactionInformationFrom.Plaid
      ? 'https://cdn.plaid.com/link/v2/stable/link-initialize.js'
      : 'https://cdn.yodlee.com/fastlink/v3/initialize.js';
  }
  /**
   * Start plaid process to import transaction.
   * */
  connectPlaid(): void {
    this.loader = true;
    // Fetch a link token from server and pass it back to app to initialize Link.
    this.companyInfoService.getPlaidLinkToken().subscribe((linkToken: string) => {
      this.openPlaidDialogue(window, linkToken);
    }, err => {
      this.loader = false;
      if (err.status === Constant.badRequest) {
        this.toastrService.error(err.response);
      }
    });
  }
  /**
   * Plaid script for open the plaid dialogue.
   * @param window
   * @param fetchLinkToken
   */
  openPlaidDialogue(window: any, fetchLinkToken: string) {
    const self = this;
    const configs = {
      token: fetchLinkToken,
      onSuccess: async function (token: string) {
        // Send the public token to server and fetch the bank transactions.
        self.getPlaidBankTransactions(token);
      },
      onExit: async function (err) {
        self.loader = false;
        if (err != null) {
          // The user encountered a Plaid API error prior to exiting.
          handler.destroy();
          self.toastrService.error(err.error_message);
        }
      }
    };
    const handler = window.Plaid.create(configs);
    handler.open();
  }

  /**
   * Get the plaid bank transaction.
   */
  async getPlaidBankTransactions(publicToken: string): Promise<void> {
    const entityBankAccountsMappingAC = new EntityBankAccountsMappingAC();
    entityBankAccountsMappingAC.entityId = this.entityId;
    entityBankAccountsMappingAC.loanApplicationId = this.loanApplicationId;
    entityBankAccountsMappingAC.isCleared = this.isCleared;
    entityBankAccountsMappingAC.publicToken = publicToken;
    this.companyInfoService.getPlaidBankTransactions(entityBankAccountsMappingAC)
      .subscribe(async (linkedBankListAC: ProviderBankListAC) => {
        this.enableSaveButton();
        if (linkedBankListAC.providerBanks.length !== 0) {
          this.startingYear = linkedBankListAC.startingYear;
          this.endingYear = linkedBankListAC.endingYear;
          this.linkedBanks = linkedBankListAC.providerBanks;
          this.selectBank(this.linkedBanks[0]);
          this.connected = true;
          this.isCleared = false;
          this.showContinueButton();
          if (this.currentSectionName === Constant.transactions) {
            await this.appService.updateCurrentSectionName(Constant.taxes);
          }
        } else {
          this.loader = false;
        }
      }, err => this.executeApiErrorBlock(err.status, err.response));
  }
  ngOnDestroy(): void {
    this.enableSaveButton();
    this.hideContinueButton();
    if (this.subsVar) {
      this.subsVar.unsubscribe();
    }
  }

  /**
   * Fetch the existing bank list
   */
  initializeBanks() {
    this.loader = true;
    this.companyInfoService.getProviderBankList(this.loanApplicationId).subscribe(
      async (linkedBankListAC: ProviderBankListAC) => {
        if (linkedBankListAC.providerBanks.length !== 0) {
          this.startingYear = linkedBankListAC.startingYear;
          this.endingYear = linkedBankListAC.endingYear;
          this.linkedBanks = linkedBankListAC.providerBanks;
          this.selectBank(this.linkedBanks[0]);
          this.showContinueButton();
        }
        else {
          this.connected = false;
          this.disableSaveButton();
          this.hideContinueButton();
          this.loader = false;
        }

        // Set the default provider for import transaction.
        this.defaultTransactionInformationFrom = linkedBankListAC.defaultTransactionInformationFrom;
        this.loadProviderScript();
        if (this.appService.getSectionNumber(Constant.transactions) === this.appService.getSectionNumber(this.currentSectionName) && linkedBankListAC.providerBanks.length !== 0) {
          this.appService.updateCurrentSectionName(Constant.taxes);
        }
      },
      async err => {
        if (err.status === 400) {
          this.toastrService.error(err.response);
        }
        this.loader = false;
      }
    );
  }

  /**
   * Fetch all the account and transaction as per the selected bank
   * @param event Selected Bank
   */
  selectBank(event) {
    this.accountType = null;
    this.currentBalance = null;
    this.transactionsArray = [];

    var providerBankId = event.id;
    this.linkedBankAccountTransactions = [];
    this.companyInfoService.getBankAccountTransactionList(providerBankId).subscribe(
      (linkedBankAccountTransactionList: BankAccountTransactionAC[]) => {
        if (linkedBankAccountTransactionList.length !== 0) {
          this.linkedBankAccountTransactions = linkedBankAccountTransactionList;
          this.selectAccount(linkedBankAccountTransactionList[0]);
        }
        this.loader = false;
      }, err => this.executeApiErrorBlock(err.status, err.response)
    );
  }
  /**
   * Execute API error block.
   * @param statusCode API response status code.
   * @param message error message.
   */
  executeApiErrorBlock(statusCode: number, errorMessage: string): void {
    this.loader = false;
    this.disableSaveButton();
    if (statusCode === Constant.badRequest) {
      this.toastrService.error(errorMessage);
    }
  }
  /**
   * Show the detail of selected account
   * @param event Selected account
   */
  selectAccount(event) {
    this.accountType = event.accountType;
    this.currentBalance = event.currentBalance;
    this.transactionsArray = event.transactionACs;
  }

  /**
   * Open the pop up
   * @param transactionModal Modal Reference 
   */
  async connectWith(transactionModal: TemplateRef<any>) {
    this.loader = true;
    if (this.defaultTransactionInformationFrom === TransactionInformationFrom.Plaid) {
      this.connectPlaid();
    } else {
      // Load Fastlink data
      await this.companyInfoService.getYodleeFastLink(this.entityId).subscribe(
        (yodleeFastLinkAC: YodleeFastLinkAC) => {
          if (yodleeFastLinkAC !== null) {

            // Open Pop Up
            this.modalRef = this.modalService.show(transactionModal, {
              backdrop: 'static',
              class: 'transaction-modal'
            });
            this.openYodleeModel(window, yodleeFastLinkAC);
          }
        },
        err => {
          this.toastrService.error(err?.Message);
        }
      );
    }
  }

  /**
   * Load the yodlee fastlink url
   * @param window yodlee fastlink window
   * @param yodleeFastLinkAC YodleeFastLinkAC obj 
   */
  openYodleeModel(window, yodleeFastLinkAC: YodleeFastLinkAC) {
    let self = this;
    window.fastlink.open({
      fastLinkURL: yodleeFastLinkAC.fastLinkUrl,
      accessToken: `Bearer ${yodleeFastLinkAC.accessToken}`,
      params: { userExperienceFlow: 'Aggregation' },
      onError: function (data) {
      },
      onExit: function (data) {
        self.saveAccountTransactionData(data.sites);
      }
    }, 'container-fastlink');
  }

  /**
   * Save the accounts and transaction data 
   */
  saveAccountTransactionData(sites: any) {
    this.accountType = null;
    this.currentBalance = null;
    this.transactionsArray = [];
    let providerAccountIds = [];
    this.closeModal();
    for (const site of sites) {
      providerAccountIds.push(site.providerAccountId);
    }
    this.loader = true;
    const entityBankAccountsMappingAC = new EntityBankAccountsMappingAC();
    entityBankAccountsMappingAC.entityId = this.entityId;
    entityBankAccountsMappingAC.loanApplicationId = this.loanApplicationId;
    entityBankAccountsMappingAC.isCleared = this.isCleared;
    entityBankAccountsMappingAC.providerAccountIds = providerAccountIds;
    this.companyInfoService.addAccountTransactions(entityBankAccountsMappingAC).subscribe(
      async (linkedBankListAC: ProviderBankListAC) => {
        if (linkedBankListAC.providerBanks.length !== 0) {
          this.startingYear = linkedBankListAC.startingYear;
          this.endingYear = linkedBankListAC.endingYear;
          this.linkedBanks = linkedBankListAC.providerBanks;
          this.selectBank(this.linkedBanks[0]);
          this.loader = false;
          this.connected = true;
          this.isCleared = false;
          this.showContinueButton();
          if (this.currentSectionName === Constant.transactions) {
            await this.appService.updateCurrentSectionName(Constant.taxes);
          }
        }
      }, (err) => {
        this.loader = false;
        this.connected = true;
        this.disableSaveButton();
        if (err.status === 400) {
          this.toastrService.error(err.response);
        }
      }
    );
  }

  /**
   * Clear the previously filled data and goes to connect with page
   */
  clearConnection() {
    this.connected = false;
    this.isCleared = true;
    this.disableSaveButton();
    this.hideContinueButton();
  }
  /**
   * Display connect to import button page and related setting.
   */
  showImportButtonPage(): void {
    this.connected = false;
    this.disableSaveButton();
    this.hideContinueButton();
  }
  /**
   * Method will close the modal and the yodlee fastlink url
   */
  closeModal() {
    this.modalRef.hide();
    const s2 = document.createElement('script');
    let innerHtml = `(function (window) {
      window.fastlink.close();
    }(window)); `
    s2.innerHTML = innerHtml;
    document.getElementById('transactionPop-Up').append(s2);
    this.loader = false;
  }

  // Enable Save button
  enableSaveButton(): void {
    this.eventEmitterService.disableSaveButton(false);
  }

  //Disable save button.
  disableSaveButton(): void {
    this.eventEmitterService.disableSaveButton(true);
  }

  //Show Continue button and Hide Save & Continue button.
  showContinueButton(): void {
    this.eventEmitterService.showContinueButton(true);
  }

  //Hide Continue button and Show Save & Continue button.
  hideContinueButton(): void {
    this.eventEmitterService.showContinueButton(false);
  }
}
