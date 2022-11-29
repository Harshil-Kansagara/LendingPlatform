import { Component, OnInit } from '@angular/core';
import { Options } from '@m0t0r/ngx-slider';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Constant } from '../shared/constant';
import { AppService } from '../services/app.service';
import { EntityService, EntityAC, UserAC, LoanApplicationStatusType, ApplicationAC } from '../utils/serviceNew';
import { Router } from '@angular/router';

@Component({
  selector: 'app-credit-profile',
  templateUrl: './credit-profile.component.html',
  styleUrls: ['./credit-profile.component.scss']
})

export class CreditProfileComponent implements OnInit {

  constructor(private readonly fb: FormBuilder,
    private readonly appService: AppService,
    private readonly entityService: EntityService,
    private readonly router: Router) {
    this.initializeCreditProfileForm();
  }

  creditProfileTitle = Constant.creditProfileTitle;
  creditQuestions = Constant.creditQuestions;
  isProcessFailed = false;
  isCreditSliderDisable = false;
  failedRequestTitle = Constant.failedRequestTitle;
  
  creditRangeStepArray = [
    { value: 0, legend: Constant.poor },
    { value: 5, legend: Constant.average },
    { value: 10, legend: Constant.fair},
    { value: 15, legend: Constant.good },
    { value: 20, legend: Constant.excellent }
  ];

  getValueBasedColors(value:number){
    const poorScore = 0;
    const averageScore = 5;
    const fairScore = 10;
    const goodScore = 15;
    const excellentScore = 20;

    if (value <= poorScore) {
      return '#FF4E3B';
    }
    if (value <= averageScore) {
      return '#FF8F4F';
    }
    if (value <= fairScore) {
      return '#DFAC20';
    }
    if (value <= goodScore) {
      return '#BACF69';
    }
    if (value <= excellentScore) {
      return '#3DB25B';
    }
    return '#FF4E3B';
  }
  options: Options = {
    floor: 0,
    ceil: 20,
    step: 5,
    disabled: this.isCreditSliderDisable,
    showSelectionBar: true,
    stepsArray: this.creditRangeStepArray,
    
    getSelectionBarColor: (value: number): string => this.getValueBasedColors(value),
    getPointerColor: (value: number): string => this.getValueBasedColors(value)
  };

  creditProfileForm: FormGroup;
  lockedApplication: ApplicationAC;
  currentStatus: string;

  /**
   * Method to initialize the credit profile form.
   * */
  initializeCreditProfileForm() {
    const defaultCreditScore = 10;
    this.creditProfileForm = this.fb.group({
      creditScore: [defaultCreditScore, Validators.required],
      hasBankruptcy: [false, Validators.required],
      hasAnyJudgements: [false, Validators.required]
    });
  }
  currentUser: EntityAC;
  creditRating;
  setCreditValue() {
    this.creditRating = this.creditProfileForm.controls['creditScore'].value;
  }
  poor(): void {
    const poorScore = 0;
    this.creditProfileForm.controls['creditScore'].reset(poorScore);
    this.creditRating = poorScore;
  }
  average(): void {
    const averageScore = 5;
    this.creditProfileForm.controls['creditScore'].reset(averageScore);
    this.creditRating = averageScore;
  }
  fair(): void {
    const fairScore = 10;
    this.creditProfileForm.controls['creditScore'].reset(fairScore);
    this.creditRating = fairScore;
  }
  good(): void {    
    const goodScore = 15;
    this.creditProfileForm.controls['creditScore'].reset(goodScore);
    this.creditRating = goodScore;
  }
  excellent(): void {
    const excellentScore = 20;
    this.creditProfileForm.controls['creditScore'].reset(excellentScore);
    this.creditRating = excellentScore;
  }

  async ngOnInit() {
    this.currentUser = await this.appService.getCurrentUserDetailsNew();
    // If current user is saved in localForage then only try to get the credit profile values.
    if (this.currentUser !== null && this.currentUser.user.selfDeclaredCreditScore !== null) {
      if (!this.currentUser.id) {
        await this.appService.setOpenCalculator(true);
      }
      // If the current loan application is locked then use JSON of its details
      this.lockedApplication = await this.appService.getLockedApplicationJsonAsObject();
      this.currentStatus = LoanApplicationStatusType[await this.appService.getCurrentLoanApplicationStatus()];
      if (this.lockedApplication && this.currentStatus && LoanApplicationStatusType[this.currentStatus] !== LoanApplicationStatusType.Draft) {
        this.setCreditProfileForm(this.lockedApplication.borrowingEntities[0].linkedEntities.filter(x => x.id === this.currentUser.id)[0]);
      } else {
        this.setCreditProfileForm(this.currentUser);
      }
    } else {
      if (!this.currentUser) {
        this.currentUser = new EntityAC();
        this.currentUser.user = new UserAC();
      }
    }
    this.setCreditValue();
  }

  /**
   * Method to set the fields of credit profile form.
   * @param currentUser Current user object
   */
  setCreditProfileForm(currentUser: EntityAC) {
    if (currentUser.user.selfDeclaredCreditScore === Constant.notKnown) {
      this.isCreditSliderDisable = !this.isCreditSliderDisable;
      this.options = Object.assign({}, this.options, { disabled: this.isCreditSliderDisable });
    } else {
      this.creditProfileForm.get('creditScore').setValue(this.creditRangeStepArray.filter(x => x.legend === currentUser.user.selfDeclaredCreditScore)[0].value);
    }
    this.creditProfileForm.get('hasBankruptcy').setValue(currentUser.user.hasBankruptcySelfDeclared);
    this.creditProfileForm.get('hasAnyJudgements').setValue(currentUser.user.hasAnyJudgementsSelfDeclared);
  }

  /**
   * Method to enable or disable the credit score panel.
   * */
  toggleCreditScorePanel() {
    this.isCreditSliderDisable = !this.isCreditSliderDisable;
    this.options = Object.assign({}, this.options, { disabled: this.isCreditSliderDisable });
    this.currentUser.user.selfDeclaredCreditScore = Constant.notKnown;
  }

  /**
   * Method to save the credit profile values on continue button click.
   * */
  async saveCreditProfile() {

    //If the loan application is not in draft status and is in view only mode then simply redirect to next section.
    if (this.lockedApplication && this.currentStatus && LoanApplicationStatusType[this.currentStatus] !== LoanApplicationStatusType.Draft) {
      this.redirectionHandler();
    } else {
      //Set the credit profile values to the current user object.
      const creditScoreValue = this.creditRangeStepArray.filter(x => x.value === this.creditProfileForm.get('creditScore').value)[0];
      if (creditScoreValue !== null && !this.isCreditSliderDisable) {
        this.currentUser.user.selfDeclaredCreditScore = creditScoreValue.legend;
      } else {
        this.currentUser.user.selfDeclaredCreditScore = Constant.notKnown;
      }
      this.currentUser.user.hasBankruptcySelfDeclared = this.creditProfileForm.get('hasBankruptcy').value;
      this.currentUser.user.hasAnyJudgementsSelfDeclared = this.creditProfileForm.get('hasAnyJudgements').value;

      //Update the current user object in localForage.
      await this.appService.removeCurrentUserDetails();
      await this.appService.setCurrentUserDetailsNew(this.currentUser);

      //Check if user is not authentic then just save the value in localForage.
      const token = localStorage.getItem('access_token');
      this.appService.isAuthenticated().subscribe(async isAuthorized => {
        if (isAuthorized && token) {
          //Save the credit profile values in database and check whether user is allowed to apply for a loan or not.
          this.entityService.addUserCreditProfile(this.currentUser).subscribe(
            async res => {
              await this.setCreditProfileEvaluationInLocalForage(res);
            },
            err => {
            });
        } else {
          this.redirectionHandler();
        }
      });
    }
  }

  /**
   * Method to set the credit profile in localForage after evaluating it.
   * */
  async setCreditProfileEvaluationInLocalForage(res: boolean) {
    if (res === true) {
      await this.appService.setIsCreditOkay(true);
      this.redirectionHandler();
    } else {
      this.isProcessFailed = true;
      await this.appService.setIsCreditOkay(false);
    }
  }

  /**
   * Method to handle the redirection from this component.
   * */
  async redirectionHandler() {
    if (await this.appService.getOpenCalculator()) {
      this.router.navigate([Constant.calculatorRedirectUrl]);
    } else if (this.router.url.includes('loan')) {
      this.router.navigate([Constant.loanNeedsRedirectUrl]);
    } else {
      this.router.navigate(['loan']);
    }
  }
}
