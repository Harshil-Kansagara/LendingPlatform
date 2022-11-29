import { Injectable, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { Constant } from '../shared/constant';


@Injectable({
  providedIn: 'root'
})
export class EventEmitterService {

  currentSectionName: string;
  invokeSaveFunctionInLoanNeeds = new EventEmitter();
  invokeSaveFunctionInCompanyInfo = new EventEmitter();
  invokeSaveFunctionInFinancialInfo = new EventEmitter();
  invokeSaveFunctionInTaxReturnInfo = new EventEmitter();
  saveBtnStatus: EventEmitter<boolean> = new EventEmitter();
  invokeSaveFunctionInLoanConsent = new EventEmitter();
  confirmAndApply: EventEmitter<boolean> = new EventEmitter();
  invokeSaveFunctionInLoanProductDetail = new EventEmitter();
  proceedToApply: EventEmitter<boolean> = new EventEmitter();
  isContinueButtonEvent: EventEmitter<boolean> = new EventEmitter();
  invokeSaveFunctionInInvoices: EventEmitter<boolean> = new EventEmitter();
  invokeSaveFunctionInTransactions: EventEmitter<boolean> = new EventEmitter();
  invokeSaveFunctionInBankDetails = new EventEmitter();
  isNextButtonEvent: EventEmitter<boolean> = new EventEmitter();
  clearButtonClickedEvent: EventEmitter<boolean> = new EventEmitter();
  
  constructor(private readonly router: Router) { }

  onSaveButtonClick() {
    if (this.router.url.includes(Constant.loanNeedsRedirectUrl)) {
      this.invokeSaveFunctionInLoanNeeds.emit();
    } else if (this.router.url.includes(Constant.companyInfoRedirectUrl)) {
      this.invokeSaveFunctionInCompanyInfo.emit();
    } else if (this.router.url.includes(Constant.financesRedirectUrl)) {
      this.invokeSaveFunctionInFinancialInfo.emit();
    } else if (this.router.url.includes(Constant.taxesReturnsRedirectUrl)) {
      this.invokeSaveFunctionInTaxReturnInfo.emit();
    } else if (this.router.url.includes(Constant.invoicesRedirectUrl)) {
      this.invokeSaveFunctionInInvoices.emit();
    } else if(this.router.url.includes(Constant.transactionsRedirectUrl)){
      this.invokeSaveFunctionInTransactions.emit();
    }
  }

  // On confirm and Apply button click
  onConfirmAndApplyButtonClick() {
    if (this.router.url.includes(Constant.loanConsentRedirectUrl)) {
      this.invokeSaveFunctionInLoanConsent.emit();
    }
  }

  // On proceed to apply button click
  onProceedToApplyButtonClick() {
    if (this.router.url.includes(Constant.loanProductDetailsRedirectUrl)) {
      this.invokeSaveFunctionInLoanProductDetail.emit();
    }
  }

  // On next button click
  onNextButtonClick() {
    if (this.router.url.includes(Constant.bankDetailsRedirectUrl)) {
      this.invokeSaveFunctionInBankDetails.emit();
    }
  }

  // Enable | Disable save button event. 
  disableSaveButton(state: boolean) {
    this.saveBtnStatus.emit(state);
  }

  showHideConfirmAndApply(state: boolean) {
    this.confirmAndApply.emit(state);
  }

  /**
   * If isShow is true then Show Continue button and Hide Save & Continue button.
   * Else Hide Continue button and Show Save & Continue button.
   * @param isShow true | false
   */
  showContinueButton(isShow: boolean): void {
    this.isContinueButtonEvent.emit(isShow);
  }

  resetFinances() {
    this.clearButtonClickedEvent.emit(true);
  }
}   
