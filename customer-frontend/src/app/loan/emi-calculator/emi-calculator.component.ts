import { Component, ElementRef, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Constant } from '../../shared/constant';
import { AppService } from '../../services/app.service';
import { GlobalService, AppSettingAC, LoanPurposeAC } from '../../utils/serviceNew';
import { Options } from '@m0t0r/ngx-slider';
import { LoanNeedsValues } from '../models/loan-needs-values.model';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { LoanService } from '../loan.service';
import { BsModalRef, BsModalService, ModalDirective } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-emi-calculator',
  templateUrl: './emi-calculator.component.html',
  styleUrls: ['./emi-calculator.component.scss']
})
export class EmiCalculatorComponent implements OnInit {

  constructor(private readonly fb: FormBuilder,
    private readonly appService: AppService,
    private readonly globalService: GlobalService,
    private readonly router: Router,
    private readonly loanService: LoanService,
    private readonly oidcSecurityService: OidcSecurityService,
    private readonly modalService: BsModalService) {
    this.initializeEmiCalculatorForm(0, 0, 0);
  }

  modalRef: BsModalRef;
  @ViewChild(ModalDirective, { static: false }) modal: ModalDirective;
  @ViewChild('template') template: TemplateRef<ElementRef>;

  interestDisclaimer: string;
  calculatorTitle = Constant.calculatorTitle;
  appSettings: AppSettingAC[] = [];
  loanPurposeList: LoanPurposeAC[] = [];

  loanPurpose: string;
  subLoanOptions;

  totalInterest = 0;
  totalPayment = 0;
  monthlyEMI = 0;
  currencySymbol: string;
  periodUnit: string;
  loanTenureInMonths = 0;

  amountSliderValue: number;
  periodSliderValue: number;
  monthlyPaymentSliderValue: number;
  loanNeedsValues = new LoanNeedsValues();

  amountOptions: Options;
  periodOptions: Options;
  monthlyPayment: Options;

  question1 = Constant.howMuchMoneyDoYouNeed;
  question2 = Constant.howLongDoYouNeedTheMoneyFor;
  question3 = Constant.whatIsThePrimaryPurpose;
  question4 = Constant.whatTypeOfProperty;
  question5 = Constant.whatTypeOfAssets;
  question6 = Constant.whatDoYouIntend;
  monthlyRepayment = Constant.monthlyRepayment;
  interestRate = 0;

  isPurposeEnabled: boolean;
  emiCalculator: FormGroup;

  afterLoginProvide: string;

  information = Constant.information;
  infoDesk = Constant.infoDesk;
  continueApply = Constant.continueApply;

  /**
   * Method to initialize the emi calculator form.
   * @param amountNeededDefault Default value of amount
   * @param amountPeriodDefault Default value of period
   * @param monthlyPaymentDefault Default value of monthly payment
   */
  initializeEmiCalculatorForm(amountNeededDefault: number, amountPeriodDefault: number, monthlyPaymentDefault: number) {
    const two = 2;
    this.emiCalculator = this.fb.group({
      amountNeeded: [parseFloat(amountNeededDefault.toFixed(two)), Validators.required],
      amountPeriod: [parseFloat(amountPeriodDefault.toFixed(two)), Validators.required],
      monthlyPayment: [parseFloat(monthlyPaymentDefault.toFixed(two)), Validators.required],
      purpose: ['', Validators.required],
      subLoanPurpose: [null, Validators.required]
    });
    if (this.subLoanOptions) {
      this.emiCalculator.get('subLoanPurpose').setValue(this.subLoanOptions[0].id);
    }
  }

  async ngOnInit() {
    this.afterLoginProvide = Constant.afterLoginProvide;
    //Set the openCalculator to false in localForage as the calculator page is opened.
    await this.appService.setOpenCalculator(false);
    this.appSettings = await this.appService.getAppSettings();
    
    //If the app settings are present then only set the required UI properties with their values.
    if (this.appSettings.length !== 0) {
      this.currencySymbol = this.appSettings.filter(x => x.fieldName === 'Currency:Symbol')[0].value;
      this.periodUnit = this.appSettings.filter(x => x.fieldName === 'LoanNeeds:LoanDurationUnit')[0].value;

      const currentUser = await this.appService.getCurrentUserDetailsNew();
      if (currentUser.user !== null) {
        this.interestRate = await this.loanService.getRateOfInterestForGivenCreditScore(currentUser.user.selfDeclaredCreditScore);
      }

      this.globalService.getLoanPurposeList().subscribe(
        async purposeList => {
          this.loanPurposeList = purposeList;
          this.loanPurposeList.sort(this.appService.sortByAnyIntegerField);

          //Set the first enabled purpose as selected if available.
          const firstEnabledPurpose = this.loanPurposeList.filter(x => x.isEnabled === true)[0];
          if (firstEnabledPurpose) {
            await this.setOptionsForSelectedPurpose(firstEnabledPurpose);
          } else {
            await this.setOptionsForSelectedPurpose(this.loanPurposeList[0]);
          }
        });
    }
  }

  //save the amount/period/purpose in localForage and move to login or loan needs.
  async applyLoan() {
    if (this.modalRef) {
      this.modalRef.hide();
    }
    this.loanNeedsValues.amount = this.emiCalculator.get('amountNeeded').value;
    this.loanNeedsValues.period = this.emiCalculator.get('amountPeriod').value;
    this.loanNeedsValues.purposeId = this.emiCalculator.get('purpose').value;
    this.loanNeedsValues.subLoanPurposeId = this.emiCalculator.get('subLoanPurpose').value;
    await this.appService.setLoanNeedsValues(this.loanNeedsValues);

    //If the user is authentic then only redirect to loan component.
    const token = localStorage.getItem('access_token');
    this.appService.isAuthenticated().subscribe(async isAuthorized => {
      if (isAuthorized && token) {
        this.router.navigate(['loan']);
      } else {
        this.oidcSecurityService.authorize();
      }
    });
  }

  //Show information modal
  async openModal(template: TemplateRef<ElementRef>) {
    if (await this.appService.getCurrentLoanApplicationId()) {
      this.applyLoan();
    } else {
      this.modalRef = this.modalService.show(template, Object.assign({}, { class: 'modal-dialog-centered modal-sm' }));
    }
  }

  /**
   * Method to calculate the values of total payment, interest, and EMI when amount slider changes.
   * @param amount Changed amount
   */
  onAmountSliderChange(amount: number) {
    this.emiCalculator.get('amountNeeded').setValue(amount);
    this.monthlyPayment = this.loanService.changeMinMaxValuesOfMonthlyPayment(amount, this.interestRate,
      this.periodOptions.floor, this.periodOptions.ceil, this.periodOptions.step, this.currencySymbol);
    this.calculateInterestTotalEMI();
  }

  /**
   * Method to calculate the values of total payment, interest, and EMI when period slider changes.
   * @param period Changed period
   */
  onPeriodSliderChange(period: number) {
    this.emiCalculator.get('amountPeriod').setValue(period);
    this.calculateInterestTotalEMI();
  }

  /**
   * Method to calculate the values of total payment, interest, and EMI when period slider changes.
   * @param payment Changed monthly payment
   */
  onMonthlyPaymentSliderChange(payment: number) {
    const monthCount = 12;
    const hundred = 100;
    this.emiCalculator.get('monthlyPayment').setValue(payment);
    this.emiCalculator.get('amountPeriod').setValue(this.loanService.calculatePeriod(this.emiCalculator.get('amountNeeded').value, payment, this.interestRate));
    this.loanTenureInMonths = Math.round(this.emiCalculator.get('amountPeriod').value * monthCount);
    this.monthlyEMI = payment;
    this.totalInterest = Math.round(((this.monthlyEMI * (this.emiCalculator.get('amountPeriod').value * monthCount))
      - this.emiCalculator.get('amountNeeded').value + Number.EPSILON) * hundred) / hundred;
    this.totalPayment = Math.round(((Number)(this.emiCalculator.get('amountNeeded').value) + this.totalInterest + Number.EPSILON) * hundred) / hundred;
  }

  /**
   * Method to calculate the total payment, total interest and monthly EMI.
   * */
  calculateInterestTotalEMI() {
    const monthCount = 12;
    const hundred = 100;
    const two = 2;
    this.loanTenureInMonths = Math.round(this.emiCalculator.get('amountPeriod').value * monthCount);
    this.monthlyEMI = this.loanService.calculateMonthlyEMI(this.emiCalculator.get('amountNeeded').value, this.interestRate, this.emiCalculator.get('amountPeriod').value);
    this.emiCalculator.get('monthlyPayment').setValue(this.monthlyEMI);
    this.totalInterest = (Number)((Math.round(((this.monthlyEMI * (this.emiCalculator.get('amountPeriod').value * monthCount))
      - this.emiCalculator.get('amountNeeded').value + Number.EPSILON) * hundred) / hundred).toFixed(two));
    this.totalPayment = (Number)((Math.round(((Number)(this.emiCalculator.get('amountNeeded').value) + this.totalInterest + Number.EPSILON) * hundred) / hundred).toFixed(two));
  }

  /**
   * Method to set the amount and period slider according to the selected purpose.
   * @param purpose Selected purpose
   */
  async setOptionsForSelectedPurpose(purpose: LoanPurposeAC) {
    this.setSubLoanPurposeOptions(purpose);
    //If purpose is enabled then only set the options
    if (purpose.isEnabled) {
      //Fetch the set options for amount and perios from loan service.
      const options = this.loanService.getAmountAndPeriodOptionsSet(this.currencySymbol, this.periodUnit, purpose, this.interestRate);
      const two = 2;
      const thousand = 1000;
      const decimalFive = 0.5;
      this.amountOptions = options[0];
      this.periodOptions = options[1];

      const averageAmount = Math.round(((this.amountOptions.floor + this.amountOptions.ceil) / two) / thousand) * thousand;
      const averagePeriod = Math.round(((this.periodOptions.floor + this.periodOptions.ceil) / two) / decimalFive) * decimalFive;
      this.monthlyPayment = this.loanService.changeMinMaxValuesOfMonthlyPayment(averageAmount, this.interestRate,
        this.periodOptions.floor, this.periodOptions.ceil, this.periodOptions.step, this.currencySymbol);
      this.initializeEmiCalculatorForm(averageAmount, averagePeriod, this.loanService.calculateMonthlyEMI(averageAmount, this.interestRate, averagePeriod));
      this.calculateInterestTotalEMI();

      this.interestDisclaimer = Constant.interestDisclaimerWhereInterestNotQuoted.replace('{0}',
        (await this.loanService.getRateOfInterestForGivenCreditScore(
          (await this.appService.getCurrentUserDetailsNew()).user.selfDeclaredCreditScore)).toString());
      this.isPurposeEnabled = true;
    } else {
      this.isPurposeEnabled = false;
    }
    this.emiCalculator.get('purpose').setValue(purpose.id);
  }

  /**
   * Set the options for sub loan purpose
   * */
  setSubLoanPurposeOptions(loanPurpose: LoanPurposeAC) {
    this.loanPurpose = loanPurpose.name;
    this.subLoanOptions = loanPurpose.subLoanPurposes.filter(x => x.isEnabled === true).sort(this.appService.sortByAnyIntegerField);
    this.emiCalculator.get('subLoanPurpose').setValue(loanPurpose.subLoanPurposes[0].id);
  }
}
