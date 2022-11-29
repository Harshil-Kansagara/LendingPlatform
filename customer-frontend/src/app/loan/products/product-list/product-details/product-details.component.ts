import { Component, OnInit } from '@angular/core';
import { AppService } from '../../../../services/app.service';
import { Constant } from '../../../../shared/constant';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Options } from '@m0t0r/ngx-slider';
import {
  RecommendedProductAC, ApplicationService, ProductDetailsAC, ApplicationAC, ApplicationBasicDetailAC, 
        LoanApplicationStatusType, ProblemDetails } from '../../../../utils/serviceNew';
import { LoanService } from '../../../loan.service';
@Component({
  selector: 'app-product-details',
  templateUrl: './product-details.component.html',
  styleUrls:['./product-details.component.scss']
})
export class ProductDetailsComponent implements OnInit {

  isViewOnlyMode=false;
  selectedProduct: RecommendedProductAC;
  selectedProductDetail: ProductDetailsAC;
  periodSliderValue = 0;
  monthlyPaymentSliderValue = 0;
  amountSliderValue = 0;
  newPeriod = 0;
  newAmount = 0;
  productPeriodOptions: Options = {
    animate: false, showSelectionBar: true,
    translate: (value: number): string => value > 1 ? `${value}${' Years'}` : `${value}${' Year'}`
  };
  monthlyPaymentOptions: Options;
  amountOptions: Options = {
    animate: false,
    showSelectionBar: true,
    translate: (value: number): string => `${'$'} ${value.toLocaleString('en')}`
  };
  amountForm: FormGroup;
  sectionName: string;
  loanApplicationId: string;
  backRoute: string = Constant.loanProductRedirectUrl;
  howMuchMoneyDoYouNeed: string = Constant.howMuchMoneyDoYouNeed;
  monthlyPayment: string = Constant.monthlyPayment;
  timeRequiredToClear: string = Constant.timeRequiredToClear;
  currencySymbol: string;
  isLoanProduct= true;
  loader = false;
  productTitle = Constant.productTitle;
  interestDisclaimer = Constant.interestDisclaimer;

  constructor(private readonly appService: AppService,
    private readonly applicationService: ApplicationService,
    private readonly toastrService : ToastrService,
    private readonly router: Router,
    private readonly loanService: LoanService,
    private readonly fb: FormBuilder) {
    this.appService.updateLoader(false);
  }


  async ngOnInit() {
    this.loader = true;
    this.currencySymbol = (await this.appService.getAppSettings()).filter(x => x.fieldName === 'Currency:Symbol')[0].value;
    this.loanApplicationId = await this.appService.getCurrentLoanApplicationId();
    this.sectionName = await this.appService.getCurrentSectionName();
    if (this.sectionName !== Constant.loanProduct) {
      this.isLoanProduct = false;
      this.backRoute = Constant.loanNeedsRedirectUrl;
    }
    this.initializeLoanProductDetail();
  }

  /**
   * Method is to initialize the particular loan product detail
   */
  async initializeLoanProductDetail() {
    this.isViewOnlyMode=await this.appService.isViewOnlyMode();
    const lockedApplication = await this.appService.getLockedApplicationJsonAsObject();
    const currentStatus = LoanApplicationStatusType[await this.appService.getCurrentLoanApplicationStatus()];
    if(lockedApplication && currentStatus && LoanApplicationStatusType[currentStatus] !== LoanApplicationStatusType.Draft) {
      this.selectedProduct = lockedApplication.selectedProduct;
      this.selectedProduct.descriptionPoints.sort(this.appService.sortByAnyIntegerField);
      this.selectedProductDetail = this.selectedProduct.productDetails;
      this.setProductDetailSliderOptions(this.selectedProductDetail);
      this.loader = false;
    } else {
      this.selectedProduct = await this.appService.getLoanProductDetail() as RecommendedProductAC;
      if (this.selectedProduct === null) {
        this.applicationService.getSelectedProduct(this.loanApplicationId).subscribe(
          (res: RecommendedProductAC) => {
            if (res != null) {
              this.selectedProduct = res;
              this.selectedProductDetail = this.selectedProduct.productDetails;
              this.setProductDetailSliderOptions(this.selectedProductDetail);
              this.loader = false;
            }
          },
          (err: ProblemDetails) => {
            if (err.status === Constant.badRequest) {
              this.toastrService.error(err.detail);
            }
          }
        );
      } else {
        this.selectedProduct.descriptionPoints.sort(this.appService.sortByAnyIntegerField);
        this.selectedProductDetail = this.selectedProduct.productDetails;
        this.setProductDetailSliderOptions(this.selectedProductDetail);
        this.loader = false;
      }
    }
  }

  /**
   * Method will redirect to product list page
   */
  redirectToProductList() : void {
    this.appService.redirectToProductListPage(true);
    this.router.navigate([Constant.loanProductRedirectUrl]);
  }

  /**
   * Method is to set the product period slider options and monthlypayment slider options
   * @param productDetailAC ProductDetailAC class object
   */
  setProductDetailSliderOptions(productDetailAC: ProductDetailsAC) {
    this.productPeriodOptions.floor = productDetailAC.minProductTenure;
    this.productPeriodOptions.ceil = productDetailAC.maxProductTenure; 
    this.productPeriodOptions.step = productDetailAC.tenureStepperCount;
    this.monthlyPaymentOptions = this.loanService.changeMinMaxValuesOfMonthlyPayment(this.selectedProductDetail.amount,
      this.selectedProductDetail.interestRate, this.productPeriodOptions.floor, this.productPeriodOptions.ceil, this.productPeriodOptions.step, this.currencySymbol);
    this.amountOptions.floor = productDetailAC.minProductAmount;
    this.amountOptions.ceil = productDetailAC.maxProductAmount;
    this.amountOptions.step = productDetailAC.amountStepperCount;
    this.setAmountForm();
  }

  /**
   * Method is used to check whether loan application data is change or not. If changes than need to update it and after save loan product data.
   */
  async saveLoanData(){
    this.loader = true;
    
    if (this.newPeriod === 0 && this.newAmount === 0) {
      this.saveLoanProductData();
    } else{
      const loanApplicationId = await this.appService.getCurrentLoanApplicationId();
      this.applicationService.getLoanApplicationDetailsById(loanApplicationId).subscribe(
        (data: ApplicationAC) => {
          if(data != null){
            const loanApplication = data.basicDetails;
            const updatedLoanApplication = new ApplicationBasicDetailAC();
            updatedLoanApplication.loanPeriod = this.selectedProductDetail.period;
            updatedLoanApplication.id = loanApplication.id;
            updatedLoanApplication.loanAmount = this.selectedProductDetail.amount;
            updatedLoanApplication.loanApplicationNumber = loanApplication.loanApplicationNumber;
            updatedLoanApplication.loanPurposeId = loanApplication.loanPurposeId;
            updatedLoanApplication.subLoanPurposeId = loanApplication.subLoanPurposeId;
            this.applicationService.updateLoanApplicationBasicDetails(loanApplicationId, updatedLoanApplication).subscribe(
              (res:ApplicationBasicDetailAC)=>{
                this.saveLoanProductData();
              }, (err: ProblemDetails) => {
                if (err.status === Constant.noContent) {
                  this.toastrService.error(err.detail);
                }
                this.loader = false;
                this.appService.updateLoader(false);
              }     
            );
          }
        }, err=>{
          this.loader = false;
          this.appService.updateLoader(false);
        }
      );
    }
  }

  /**
   * Method is used to save loan product data
   */
  saveLoanProductData(){
    this.applicationService.saveProduct(this.loanApplicationId, this.selectedProduct.id).subscribe(
      async data => {
        await this.appService.removeLoanProductDetail();
        if(this.sectionName === Constant.loanProduct){
          this.applicationService.updateCurrentSectionName(this.loanApplicationId, this.sectionName).subscribe(
            async (updatedSectionName: string) => {
              if (updatedSectionName != null) {
                await this.appService.updateCurrentSectionName(updatedSectionName);
                await this.appService.updateProgressbar(Constant.companyInfoProgressBar);
                this.router.navigate([Constant.companyInfoRedirectUrl]);
              }
              this.router.navigate([Constant.companyInfoRedirectUrl]);
            }, () => {
              this.loader = false;
              this.appService.updateLoader(false);
            }
          );
        } else {
          this.router.navigate([Constant.companyInfoRedirectUrl]);
        }
        
      }, (err : ProblemDetails) => {
        if(err.status === Constant.badRequest){
          this.toastrService.error(err.detail);
        }
        this.loader = false;
        this.appService.updateLoader(false);
      }
    );
  }

  /**
   * Method is used to set amount form
   */
  setAmountForm(): void{
    const months = 12;
    this.amountForm = this.fb.group({
      totalPayment: [this.selectedProductDetail.totalPayment],
      totalInterest: [this.selectedProductDetail.totalInterest],
      period: [this.selectedProductDetail.period],
      monthlyPayment: [this.selectedProductDetail.monthlyPayment],
      amount: [this.selectedProductDetail.amount],
      totalNumberOfPayment: [Math.round(this.selectedProductDetail.period * months)]
    });
  }

  /**
   * Method will invoke when slider of amount will change and calculate monthly payment accordingly
   * @param amount Loan amount 
   */
  onChangeAmount(amount: number): void{
    const fixedValue = 2;
    const months = 12;
    this.newAmount = amount;
    this.selectedProductDetail.amount = amount;

    this.monthlyPaymentOptions = this.loanService.changeMinMaxValuesOfMonthlyPayment(amount, this.selectedProductDetail.interestRate,
      this.productPeriodOptions.floor, this.productPeriodOptions.ceil, this.productPeriodOptions.step, this.currencySymbol);
    this.selectedProductDetail.monthlyPayment = this.loanService.calculateMonthlyEMI(amount, this.selectedProductDetail.interestRate, this.selectedProductDetail.period);
    const totalPayment = parseFloat((this.selectedProductDetail.monthlyPayment * this.selectedProductDetail.period * months).toFixed(fixedValue));
    const totalInterest = parseFloat((totalPayment - this.selectedProductDetail.amount).toFixed(fixedValue));

    this.selectedProductDetail.totalPayment = `${this.currencySymbol} ${totalPayment.toLocaleString('en')}`;
    this.selectedProductDetail.totalInterest = `${this.currencySymbol} ${totalInterest.toLocaleString('en')}`;

    this.setAmountForm();
  }

  /**
   * Method will invoke when slider of period will change and calculate monthly payment accordingly
   * @param period Period
   */
  onChangeYear(period: number): void{
    const fixedValue = 2;
    const months = 12;
    this.newPeriod = period;
    this.selectedProductDetail.period = period;

    // Calculate new monthly payment
    this.selectedProductDetail.monthlyPayment = this.loanService.calculateMonthlyEMI(this.selectedProductDetail.amount, this.selectedProductDetail.interestRate, period);
    const totalPayment = parseFloat((this.selectedProductDetail.monthlyPayment * period * months).toFixed(fixedValue));
    const totalInterest = parseFloat((totalPayment - this.selectedProductDetail.amount).toFixed(fixedValue));

    this.selectedProductDetail.totalPayment = `${this.currencySymbol} ${totalPayment.toLocaleString('en')}`;
    this.selectedProductDetail.totalInterest = `${this.currencySymbol} ${totalInterest.toLocaleString('en')}`;

    this.setAmountForm();
  }

  /**
   * Method will invoke when slider of monthly payment will change and calculate loan period accordingly
   * @param monthlyPayment Monthly Payment value
   */
  onChangeMonthlyPayment(monthlyPayment: number): void{
    const fixedValue = 2;
    const months = 12;
    this.selectedProductDetail.monthlyPayment = monthlyPayment;

    // Calculate new period 
    this.selectedProductDetail.period = this.loanService.calculatePeriod(this.selectedProductDetail.amount, monthlyPayment, this.selectedProductDetail.interestRate);
    this.newPeriod = this.selectedProductDetail.period;
    const totalPayment = parseFloat((monthlyPayment * this.selectedProductDetail.period * months).toFixed(fixedValue));
    const totalInterest = parseFloat((totalPayment - this.selectedProductDetail.amount).toFixed(fixedValue));

    this.selectedProductDetail.totalPayment = `${this.currencySymbol} ${totalPayment.toLocaleString('en')}`;
    this.selectedProductDetail.totalInterest = `${this.currencySymbol} ${totalInterest.toLocaleString('en')}`;

    this.setAmountForm();
  }
}
