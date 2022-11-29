import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { Constant } from '../../shared/constant';
import { ApplicationService, BankAC, LoanEntityBankDetailsAC, EntityBankDetailsAC, LoanApplicationStatusType, GlobalService } from '../../utils/serviceNew';
import { Subscription } from 'rxjs';
import { AppService } from '../../services/app.service';
import { ToastrService } from 'ngx-toastr';
@Component({
  selector: 'app-bank-details',
  templateUrl: './bank-details.component.html',
})
export class BankDetailsComponent implements OnInit {

  constructor(private readonly fb: FormBuilder,
    private readonly applicationService: ApplicationService,
    private readonly globalService: GlobalService,
    private readonly appService: AppService,
    private readonly toastrService: ToastrService) {
    this.appService.updateRoute(Constant.loanStatusRedirectUrl);
  }

  loader = false;
  bankDetailsTitle = Constant.bankDetailsTitle;
  bankDetails = [
    {
      name: null,
      routingNumber: '',
      accountNumber: ''
    }
  ];

  whereToDepositLoanAmount = Constant.whereToDepositLoanAmount;
  accountNumber = Constant.accountNumberField;
  routingNumber = Constant.routingNumberField;

  bankDetailsForm: FormGroup;
  requiredField = Constant.requiredField;
  bankList = Array<BankAC>();
  subsVar: Subscription;
  loanStatus: LoanApplicationStatusType;

  createForm(bankDetails) {
    const arr = [];
    for (const bankDetail of bankDetails) {
      arr.push(this.createItem(bankDetail));
    }
    this.bankDetailsForm = this.fb.group({
      bankDetails: this.fb.array(arr)
    });
  }

  createItem(obj): FormGroup {
    return this.fb.group({
      name: obj.name,
      routingNumber: obj.routingNumber,
      accountNumber: obj.accountNumber
    });
  }

  get bankFormControls() {
    return (this.bankDetailsForm.get('bankDetails') as FormArray).controls;
  }

  async ngOnInit() {
    this.startLoader();
    this.createForm(this.bankDetails);

    this.loanStatus = await this.appService.getCurrentLoanApplicationStatus();
    const currentSection = await this.appService.getCurrentSectionName();
    if (this.loanStatus === LoanApplicationStatusType.Draft) {
      this.toastrService.error(Constant.fillPreviousSteps);
      this.appService.redirectToSection(currentSection);
    }
    else if (this.loanStatus !== LoanApplicationStatusType.Approved) {
      this.toastrService.error(Constant.onlyApprovedLoanApplicationsAllowed);
      this.appService.redirectToSection(Constant.loanStatus);
    } else {
      this.globalService.getListOfBanks().subscribe(async res => {

        this.bankList = res;
        const loanApplicationId = await this.appService.getCurrentLoanApplicationId();

        this.applicationService.getBankDetailsOfLoanApplication(loanApplicationId).subscribe(
          async bankDetails => {

            // prefill form if data exists
            if (bankDetails && bankDetails.loanAmountDepositeeBank) {
              this.bankDetails[0].accountNumber = bankDetails.loanAmountDepositeeBank.accountNumber;
              const bank = this.getBankDetailsFromId(bankDetails.loanAmountDepositeeBank.bankId);
              this.bankDetails[0].routingNumber = bank?.swiftCode;
              this.bankDetails[0].name = bank?.name;
              this.createForm(this.bankDetails);

              this.stopLoader();
            }
        }, () => {
          this.stopLoader();
        });

      }, () => {
        this.stopLoader();
      });
    }
  }

  /**
   * Fetch the bankid from the name.
   * @param name bank name.
   */
  getBankIdFromName(name: string): string {
    return this.bankList.find(s => s.name === name).id;
  }

  /**
   * Get bank details from id.
   * @param id bankId.
   */
  getBankDetailsFromId(id: string): BankAC {
    return this.bankList.find(s => s.id === id);
  }

  /**
   * Update the bank routing number while select the bank.
   * @param index Operation perform on the selected option.
   * @param bank bank details.
   */
  addBankRoutingNumber(index: number, bank: BankAC) {
    let updateBankObject = [];
    const firstIndex = 0;
    if (index === firstIndex) {
      updateBankObject = [{ routingNumber: bank.swiftCode }];
    } else {
      updateBankObject = [{}, { routingNumber: bank.swiftCode }];
    }

    // Add the routing number value.
    this.bankDetailsForm.patchValue({
      bankDetails: updateBankObject
    });
  }

  /**
   * Save the bank details.
   * */
  async saveBankDetails() {
    this.bankDetailsForm.markAllAsTouched();
    if (this.bankDetailsForm.valid) {
      this.startLoader();

      // Bind the details.
      const loanEntityBankDetailsAC = new LoanEntityBankDetailsAC();
      loanEntityBankDetailsAC.loanApplicationId = await this.appService.getCurrentLoanApplicationId();
      loanEntityBankDetailsAC.loanAmountDepositeeBank = new EntityBankDetailsAC();
      loanEntityBankDetailsAC.loanAmountDepositeeBank.accountNumber = this.bankDetailsForm.value['bankDetails'][0].accountNumber;
      loanEntityBankDetailsAC.loanAmountDepositeeBank.bankId = this.getBankIdFromName(this.bankDetailsForm.value['bankDetails'][0].name);
      
      // Save the bank details.
      this.applicationService.addBankDetails(loanEntityBankDetailsAC, loanEntityBankDetailsAC.loanApplicationId).subscribe(async res => {
        this.stopLoader();
        this.toastrService.success(Constant.bankDataSavedSuccessfully);
      }, err => {
        this.stopLoader();
        if (this.loanStatus !== LoanApplicationStatusType.Approved) {
          this.toastrService.error(Constant.onlyApprovedLoanApplicationsAllowed);
        }
        else {
          this.toastrService.error(Constant.someThingWentWrong);
        }
      })
    }
  }

  /**
   * Start the loader
   * */
  startLoader(): void {
    this.loader = true;
  }

  /**
   * Stop the loader
   * */
  stopLoader(): void {
    this.loader = false;
  }
}
