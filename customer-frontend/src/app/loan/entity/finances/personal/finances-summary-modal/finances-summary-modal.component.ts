import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { Constant } from '../../../../../shared/constant';
import { PersonalFinanceAC } from '../../../../../utils/serviceNew';

@Component({
  selector: 'app-finances-summary-modal',
  templateUrl: './finances-summary-modal.component.html',
  styleUrls: ['./finances-summary-modal.component.scss']
})
export class FinancesSummaryModalComponent implements OnInit {
  yourAssets = Constant.yourAssets;
  originalValue = Constant.originalValue;
  currentValue = Constant.currentValue;
  currentBalance = Constant.currentBalance;
  originalBalance = Constant.originalBalance;
  currentNetWorth = Constant.currentNetWorth;
  totalAssets = Constant.totalAssets;
  totalObligations = Constant.totalObligations;
  currentNetWorthAmount: number;
  totalAssetsAmount: number;
  totalObligationsAmount: number;
  checking = Constant.checking;
  savings = Constant.savings;
  brokerages = Constant.brokerages;
  retirement = Constant.retirement;
  lifeInsurance = Constant.lifeInsurance;
  receivables = Constant.receivables;
  realEstate = Constant.realEstate;
  autoMobile = Constant.autoMobile;
  yourObligations = Constant.yourObligations;
  creditCards = Constant.creditCards;
  mortgageLoans = Constant.mortgageLoans;
  otherLoans = Constant.otherLoans;
  unpaidTaxes = Constant.unpaidTaxes;
  incomeSection = Constant.incomeSection;
  incomeInformation = Constant.incomeInformation;
  instalmentLoans = Constant.installmentLoans;
  isConsent = false;
  @Input() finances: PersonalFinanceAC;
  @Output() submitEvent = new EventEmitter<boolean>();
  @Input() currency: string;
  accountArrayWithTotalAmount = [];

  constructor(private readonly router: Router) { }

  ngOnInit(): void {
    if (this.router.url.includes('consent')) {
      this.isConsent = true;
    } else {
      this.isConsent = false;
    }
    this.finances.summary.accounts.sort((a, b) => a.order - b.order);
    for (const finance of this.finances.summary.accounts) {
      finance.categories.sort((a, b) => a.order - b.order);

      const total = finance.categories.filter(x => x.currentAmount).reduce((sum, current) => sum + current.currentAmount, 0);
      this.accountArrayWithTotalAmount.push({ account: finance, totalAmount: total });
    }

    this.totalAssetsAmount = this.accountArrayWithTotalAmount.find(x => x.account.name === 'Assets')?.totalAmount ?? 0;
    this.totalObligationsAmount = this.accountArrayWithTotalAmount.find(x => x.account.name === 'Obligations')?.totalAmount ?? 0;
    this.currentNetWorthAmount = this.totalAssetsAmount - this.totalObligationsAmount;
  }

  redirect() {
    this.submitEvent.emit(true);
  }

}
