import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AppService } from '../../services/app.service';
import { Constant } from '../../shared/constant';
import { ToastrService } from 'ngx-toastr';
import {
  GlobalService, ApplicationBasicDetailAC, LoanPurposeAC, ApplicationService, ApplicationAC,
  AppSettingAC, LoanApplicationStatusType, ProblemDetails, EntityAC
} from '../../utils/serviceNew';
import { Options } from '@m0t0r/ngx-slider';
import { LoanService } from '../loan.service';

@Component({
  selector: 'app-loan-application',
  templateUrl: './loan-needs.component.html',
  styleUrls: ['./loan-needs.component.scss']
})

export class LoanNeedsComponent implements OnInit {

  constructor(private readonly fb: FormBuilder,
    private readonly appService: AppService,
    private readonly router: Router,
    private readonly globalService: GlobalService,
    private readonly applicationService: ApplicationService,
    private readonly loanService: LoanService,
    private readonly toastrService: ToastrService) {
    this.initializeLoanNeedsForm(0, 0, 0);
    this.appService.updateRoute(Constant.homeRedirectUrl);
    this.appService.updateLoader(false);
    this.appService.updateNextRoute(Constant.loanProductRedirectUrl);
  }

  subLoanOptions;
  creditProfileSectionRedirectUrl = Constant.creditProfileSectionRedirectUrl;
  loanPurposeList: LoanPurposeAC[] = [];
  loanApplicationBasicDetailAC = new ApplicationBasicDetailAC();
  loader = true;
  loanPurpose: string;
  appSettings: AppSettingAC[] = [];
  currencySymbol: string;
  periodUnit: string;
  yearlyInterestRateForEMI: number;
  fundsPurpose = [];
  amountSliderValue: number;
  periodSliderValue: number;
  monthlyPaymentSliderValue: number;
  question1 = Constant.howMuchMoneyDoYouNeed;
  question2 = Constant.howLongDoYouNeedTheMoneyFor;
  question3 = Constant.whatIsThePrimaryPurpose;
  question4 = Constant.whatTypeOfProperty;
  question5 = Constant.whatTypeOfAssets;
  question6 = Constant.whatDoYouIntend;
  monthlyPaymentText = Constant.monthlyPaymentBasedOnCreditProfile;
  isPurposeEnabled: boolean;
  loanNeedsForm: FormGroup;
  tempForm: FormGroup;
  interestDisclaimer: string;
  initiatorsCreditScore: string;
  productId: string = null;
  monthlyRepayment: string;
  totalPayment = 0;
  totalInterest=0;
  loanTenureInMonths = 0;
  monthlyEMI = 0;
  interestRate = 0;
  /**
   * Method to initialize the loan needs and temp form.
   * @param amountNeededDefault Default value of amount
   * @param amountPeriodDefault Default value of period
   * @param monthlyPaymentDefault Default value of monthly payment
   */
  initializeLoanNeedsForm(amountNeededDefault: number, amountPeriodDefault: number, monthlyPaymentDefault: number) {
    this.loanNeedsForm = this.fb.group({
      amountNeeded: [amountNeededDefault, Validators.required],
      amountPeriod: [amountPeriodDefault, Validators.required],
      monthlyPayment: [monthlyPaymentDefault, Validators.required],
      purpose: ['', Validators.required],
      subLoanPurpose: [null, Validators.required]
    });
    if (this.subLoanOptions) {
      this.loanNeedsForm.get('subLoanPurpose').setValue(this.subLoanOptions[0].id);
    }
    this.tempForm = this.fb.group({
      amountNeeded: [amountNeededDefault, Validators.required],
      amountPeriod: [amountPeriodDefault, Validators.required],
      purpose: ['', Validators.required],
      subLoanPurpose: [null, Validators.required]
    });
  }

  // Slider options
  amountOptions: Options;
  periodOptions: Options;
  monthlyPayment: Options;
  currentLoanApplicationId: string;
  currentUserDetails: EntityAC;
  // Code as per new UX ends here

  async ngOnInit() {
    this.monthlyRepayment = Constant.monthlyRepaymentText;
    //Fetch all the loan purposes
    this.globalService.getLoanPurposeList().subscribe(
      async purposeList => {
        this.loanPurposeList = purposeList;
        this.loanPurposeList.sort(this.appService.sortByAnyIntegerField);
        this.currentLoanApplicationId = await this.appService.getCurrentLoanApplicationId();
        this.currentUserDetails = await this.appService.getCurrentUserDetailsNew();
        if (this.currentUserDetails.user !== null) {
          this.interestRate = await this.loanService.getRateOfInterestForGivenCreditScore(this.currentUserDetails.user.selfDeclaredCreditScore);
        }
        await this.setAppSettingsToVariables();
        if (this.currentLoanApplicationId) {

          // If the status of the current loan application is locked then use JSON of its details
          // otherwise make the backend call to get the details.

          // Get the locked application object from JSON stored in localForage. If the JSON not found in localForage
          // then only make the backend call to get the application details.
          const lockedApplication = await this.appService.getLockedApplicationJsonAsObject();
          if (lockedApplication && await this.appService.getCurrentLoanApplicationStatus() !== LoanApplicationStatusType.Draft) {
            this.initiatorsCreditScore = lockedApplication.borrowingEntities[0].linkedEntities.filter(x =>
              x.id === lockedApplication.basicDetails.entityId)[0].user.selfDeclaredCreditScore;
            await this.initializeLoanApplicationBasicDetailAC(lockedApplication);
          } else {
            this.applicationService.getLoanApplicationDetailsById(this.currentLoanApplicationId).subscribe(
              async resp => {
                if (resp.borrowingEntities && resp.borrowingEntities[0] && resp.borrowingEntities[0].linkedEntities) {
                  this.initiatorsCreditScore = resp.borrowingEntities[0].linkedEntities.filter(x =>
                    x.id === resp.basicDetails.entityId)[0].user.selfDeclaredCreditScore;
                } else {
                  this.initiatorsCreditScore = (await this.appService.getCurrentUserDetailsNew()).user.selfDeclaredCreditScore;
                }

                await this.initializeLoanApplicationBasicDetailAC(resp);
              });
          }
        } else {
          const applicationDetailsAC = new ApplicationAC();
          applicationDetailsAC.basicDetails = new ApplicationBasicDetailAC();
          this.initiatorsCreditScore = this.currentUserDetails.user.selfDeclaredCreditScore;
          await this.appService.updateProgressbar(Constant.loanNeedsProgressBar);
          await this.initializeLoanApplicationBasicDetailAC(applicationDetailsAC);
        }
      },
      (err: ProblemDetails) => {
        this.toastrService.error(err.detail);
      });
  }

  /**
   * Method to set all the app settings with relevant variables.
   * */
  async setAppSettingsToVariables() {
    this.appSettings = await this.appService.getAppSettings();
    if (this.appSettings.length !== 0) {
      this.currencySymbol = this.appSettings.filter(x => x.fieldName === 'Currency:Symbol')[0].value;
      this.periodUnit = this.appSettings.filter(x => x.fieldName === 'LoanNeeds:LoanDurationUnit')[0].value;


      if (this.currentUserDetails && this.currentUserDetails.user) {
        this.yearlyInterestRateForEMI = await this.loanService.getRateOfInterestForGivenCreditScore(this.currentUserDetails.user.selfDeclaredCreditScore);
      } else {
        this.yearlyInterestRateForEMI = 0;
      }
    }
  }

  /**
   * Method to add/update loan application.
   * */
  async saveLoanApplication() {

    if (!this.currentLoanApplicationId) {

      this.loanApplicationBasicDetailAC = new ApplicationBasicDetailAC();
      this.loanApplicationBasicDetailAC.loanAmount = this.loanNeedsForm.get(Constant.amountNeeded).value;
      this.loanApplicationBasicDetailAC.loanPeriod = this.loanNeedsForm.get(Constant.amountPeriod).value;
      this.loanApplicationBasicDetailAC.loanPurposeId = this.loanNeedsForm.get(Constant.purpose).value;
      this.loanApplicationBasicDetailAC.status = LoanApplicationStatusType.Draft;
      this.loanApplicationBasicDetailAC.subLoanPurposeId = this.loanNeedsForm.get(Constant.subLoanPurpose).value;
      this.applicationService.addLoanApplicationBasicDetails(this.loanApplicationBasicDetailAC).subscribe(
        async data => {
          this.appService.openLoanApplication(data, true, false,false);
          this.router.navigate([Constant.loanProductRedirectUrl]);
          await this.appService.updateProgressbar(Constant.loanProductProgressBar);
          await this.appService.updateCurrentSectionName(Constant.loanProduct);
          this.loader = false;
        },
        () => {
          this.appService.updateLoader(false);
        });
    } else {
      await this.appService.removeLoanProductDetail();
      if (this.tempForm.get(Constant.amountNeeded).value === this.loanNeedsForm.get(Constant.amountNeeded).value
        && this.tempForm.get(Constant.amountPeriod).value === this.loanNeedsForm.get(Constant.amountPeriod).value
        && this.tempForm.get(Constant.purpose).value === this.loanNeedsForm.get(Constant.purpose).value
        && this.tempForm.get(Constant.subLoanPurpose).value === this.loanNeedsForm.get(Constant.subLoanPurpose).value) {
        await this.redirectHandlerForLoanProductSection();
      } else {
        this.loanApplicationBasicDetailAC = new ApplicationBasicDetailAC();
        this.loanApplicationBasicDetailAC.id = this.currentLoanApplicationId;
        this.loanApplicationBasicDetailAC.loanAmount = this.loanNeedsForm.get(Constant.amountNeeded).value;
        this.loanApplicationBasicDetailAC.loanPeriod = this.loanNeedsForm.get(Constant.amountPeriod).value;
          this.loanApplicationBasicDetailAC.loanPurposeId = this.loanNeedsForm.get(Constant.purpose).value;
          this.loanApplicationBasicDetailAC.subLoanPurposeId = this.loanNeedsForm.get(Constant.subLoanPurpose).value;
        this.loanApplicationBasicDetailAC.loanApplicationNumber = await this.appService.getCurrentLoanApplicationNumber();
        

        this.applicationService.updateLoanApplicationBasicDetails(this.currentLoanApplicationId, this.loanApplicationBasicDetailAC).subscribe(
          async (applicationBasicDetailAC: ApplicationBasicDetailAC) => {
            await this.redirectHandlerForLoanProductSection();
          },
          () => {
            this.appService.updateLoader(false);
          });
      }
    }
  }

  /**
   * Method to handle redirection for loan product section
   */
  async redirectHandlerForLoanProductSection() {
    const sectionName = await this.appService.getCurrentSectionName();
    if (sectionName === Constant.loanProduct) {
      this.router.navigate([Constant.loanProductRedirectUrl]);
    } else {
      this.router.navigate([Constant.loanProductDetailsRedirectUrl]);
    }
  }

  /**
   * Intialize the loan application object(with values if exsiting).
   * @param res Application details object
   */
  async initializeLoanApplicationBasicDetailAC(res: ApplicationAC) {
    await this.appService.setCurrentLoanApplicationNumber(res.basicDetails.loanApplicationNumber);
    if (res.selectedProduct) {
      this.productId = res.selectedProduct.id;
    }

    if (res && res.basicDetails && res.basicDetails.id) {
      this.loanApplicationBasicDetailAC.id = res.basicDetails.id;
      this.loanNeedsForm.get('amountNeeded').setValue(res.basicDetails.loanAmount);
      this.loanNeedsForm.get('amountPeriod').setValue(res.basicDetails.loanPeriod);
      this.loanNeedsForm.get('purpose').setValue(res.basicDetails.loanPurposeId);
      this.loanNeedsForm.get('subLoanPurpose').setValue(res.basicDetails.subLoanPurposeId);
      this.loanNeedsForm.get('monthlyPayment').setValue(this.loanService.calculateMonthlyEMI(
        res.basicDetails.loanAmount, this.yearlyInterestRateForEMI, res.basicDetails.loanPeriod));
      await this.appService.openLoanApplication(res.basicDetails,true,true,false);
    } else {
      const values = await this.appService.getLoanNeedsValues();
      if (values && values.purposeId) {
        this.loanNeedsForm.get('amountNeeded').setValue(values.amount);
        this.loanNeedsForm.get('amountPeriod').setValue(values.period);
        this.loanNeedsForm.get('purpose').setValue(values.purposeId);
        this.loanNeedsForm.get('subLoanPurpose').setValue(values.subLoanPurposeId);
        this.loanNeedsForm.get('monthlyPayment').setValue(this.loanService.calculateMonthlyEMI(values.amount, this.yearlyInterestRateForEMI, values.period));
      }
    }
    this.tempForm.get('amountNeeded').setValue(this.loanNeedsForm.get('amountNeeded').value);
    this.tempForm.get('amountPeriod').setValue(this.loanNeedsForm.get('amountPeriod').value);
    this.tempForm.get('purpose').setValue(this.loanNeedsForm.get('purpose').value);
    this.tempForm.get('subLoanPurpose').setValue(this.loanNeedsForm.get('subLoanPurpose').value);
    await this.setSlidersOnPurposeSelection(this.loanNeedsForm.get('purpose').value);
    this.onMonthlyPaymentSliderChange(this.loanNeedsForm.get('monthlyPayment').value);
    this.loader = false;
  }

  /**
   * Method to calculate the total payment, total interest and monthly EMI.
   * */
  calculateInterestTotalEMI() {
    const monthCount = 12;
    const hundred = 100;
    const two = 2;
    this.loanTenureInMonths = Math.round(this.loanNeedsForm.get('amountPeriod').value * monthCount);
    this.monthlyEMI = this.loanService.calculateMonthlyEMI(this.loanNeedsForm.get('amountNeeded').value, this.interestRate, this.loanNeedsForm.get('amountPeriod').value);
    this.loanNeedsForm.get('monthlyPayment').setValue(this.monthlyEMI);
    this.totalInterest = (Number)((Math.round(((this.monthlyEMI * (this.loanNeedsForm.get('amountPeriod').value * monthCount))
      - this.loanNeedsForm.get('amountNeeded').value + Number.EPSILON) * hundred) / hundred).toFixed(two));
    this.totalPayment = (Number)((Math.round(((Number)(this.loanNeedsForm.get('amountNeeded').value) + this.totalInterest + Number.EPSILON) * hundred) / hundred).toFixed(two));
  }

  /**
   * Method to calculate the values of monthly payment when amount slider changes.
   * @param amount Changed amount
   */
  onAmountSliderChange(amount: number) {
    this.loanNeedsForm.get('amountNeeded').setValue(amount);
    this.monthlyPayment = this.loanService.changeMinMaxValuesOfMonthlyPayment(amount, this.yearlyInterestRateForEMI,
      this.periodOptions.floor, this.periodOptions.ceil, this.periodOptions.step, this.currencySymbol);
    this.loanNeedsForm.get('monthlyPayment').setValue(this.loanService.calculateMonthlyEMI(amount, this.yearlyInterestRateForEMI, this.loanNeedsForm.get('amountPeriod').value));
    this.calculateInterestTotalEMI();
  }

  /**
   * Method to calculate the values of monthly payment when period slider changes.
   * @param period Changed period
   */
  onPeriodSliderChange(period: number) {
    this.loanNeedsForm.get('amountPeriod').setValue(period);
    this.loanNeedsForm.get('monthlyPayment').setValue(this.loanService.calculateMonthlyEMI(this.loanNeedsForm.get('amountNeeded').value, this.yearlyInterestRateForEMI, period));
    this.calculateInterestTotalEMI();
  }

  /**
   * Method to calculate the values of period when monthly payment slider changes.
   * @param payment Changed monthly payment
   */
  onMonthlyPaymentSliderChange(payment: number) {
    const monthCount = 12;
    const hundred = 100;
    this.loanNeedsForm.get('monthlyPayment').setValue(payment);
    this.loanNeedsForm.get('amountPeriod').setValue(this.loanService.calculatePeriod(this.loanNeedsForm.get('amountNeeded').value, payment, this.yearlyInterestRateForEMI));
    this.loanTenureInMonths = Math.round(this.loanNeedsForm.get('amountPeriod').value * monthCount);
    this.monthlyEMI = payment;
    this.totalInterest = Math.round(((this.monthlyEMI * (this.loanNeedsForm.get('amountPeriod').value * monthCount))
      - this.loanNeedsForm.get('amountNeeded').value + Number.EPSILON) * hundred) / hundred;
    this.totalPayment = Math.round(((Number)(this.loanNeedsForm.get('amountNeeded').value) + this.totalInterest + Number.EPSILON) * hundred) / hundred;
  }

  /**
   * Method to set the amount and period slider according to the selected purpose.
   * @param purposeId Selected purpose's id
   */
  async setSlidersOnPurposeSelection(purposeId: string) {
    const purposeLinkedWithGivenId = this.loanPurposeList.filter(x => x.id === purposeId)[0];
    //If purpose linked with given id is not null then select the specific purpose -
    // - otherwise go for first enabled purpose.
    if (purposeLinkedWithGivenId) {
      this.setSubLoanPurposeOptions(purposeLinkedWithGivenId);
      await this.setOptionsForSelectedPurpose(purposeLinkedWithGivenId);
    } else {
      const firstEnabledPurpose = this.loanPurposeList.filter(x => x.isEnabled)[0];
      if (firstEnabledPurpose) {
        this.setSubLoanPurposeOptions(firstEnabledPurpose);
        await this.setOptionsForSelectedPurpose(firstEnabledPurpose);
      } else {
        this.setOptionsForDefaultPurpose();
      }
    }
  }

  /**
   * Set the options for selected purpose if it is avaliable.
   * @param selectedPurpose Selected purpose object
   */
  async setOptionsForSelectedPurpose(selectedPurpose: LoanPurposeAC) {
    if (selectedPurpose.isEnabled) {
      //Fetch the set options for amount and perios from loan service.
      const options = this.loanService.getAmountAndPeriodOptionsSet(this.currencySymbol, this.periodUnit, selectedPurpose, this.yearlyInterestRateForEMI);
      this.amountOptions = options[0];
      this.periodOptions = options[1];

      //If it is allowed to change the form value when values are not already saved
      if (this.tempForm.get('purpose').value !== selectedPurpose.id) {
        //Set the form values with selected purpose's values
        const two = 2;
        const amount = Math.round((this.amountOptions.floor + this.amountOptions.ceil) / two);
        const period = Math.round((this.periodOptions.floor + this.periodOptions.ceil) / two);

        this.monthlyPayment = this.loanService.changeMinMaxValuesOfMonthlyPayment(amount, this.yearlyInterestRateForEMI,
          this.periodOptions.floor, this.periodOptions.ceil, this.periodOptions.step, this.currencySymbol);
        this.initializeLoanNeedsForm(amount, period, this.loanService.calculateMonthlyEMI(amount, this.yearlyInterestRateForEMI, period));
        this.loanNeedsForm.get('purpose').setValue(selectedPurpose.id);
      } else {
        this.loanNeedsForm.get('amountNeeded').setValue(this.tempForm.get('amountNeeded').value);
        this.loanNeedsForm.get('amountPeriod').setValue(this.tempForm.get('amountPeriod').value);
        this.loanNeedsForm.get('purpose').setValue(this.tempForm.get('purpose').value);
        this.loanNeedsForm.get('subLoanPurpose').setValue(this.tempForm.get('subLoanPurpose').value);
        this.monthlyPayment = this.loanService.changeMinMaxValuesOfMonthlyPayment(this.loanNeedsForm.get('amountNeeded').value,
          this.yearlyInterestRateForEMI, this.periodOptions.floor, this.periodOptions.ceil, this.periodOptions.step, this.currencySymbol);
      }

      this.interestDisclaimer = Constant.interestDisclaimerWhereInterestNotQuoted.replace('{0}',
        (await this.loanService.getRateOfInterestForGivenCreditScore(this.initiatorsCreditScore)).toString());
      this.isPurposeEnabled = true;
    } else {
      this.setOptionsForDefaultPurpose();
    }
  }

  /**
   * Set the options for default purpose.
   * */
  setOptionsForDefaultPurpose() {
    this.loanNeedsForm.get('purpose').setValue(this.loanPurposeList[0].id);
    this.setSubLoanPurposeOptions(this.loanPurposeList[0]);
    this.isPurposeEnabled = false;
  }

  /**
   * Set the options for sub loan purpose
   * */
  setSubLoanPurposeOptions(loanPurpose: LoanPurposeAC) {
    this.loanPurpose = loanPurpose.name;
    this.subLoanOptions = loanPurpose.subLoanPurposes.filter(x => x.isEnabled === true).sort(this.appService.sortByAnyIntegerField);
    this.loanNeedsForm.get('subLoanPurpose').setValue(loanPurpose.subLoanPurposes[0].id);
  } 
}


