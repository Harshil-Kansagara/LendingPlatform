import { Component, OnInit, Pipe, PipeTransform } from '@angular/core';
import { AppService } from '../../../services/app.service';
import { Constant } from '../../../shared/constant';
import { Router } from '@angular/router';
import { ApplicationService, EntityAC, EntityService, LoanApplicationStatusType, ProblemDetails, RecommendedProductAC } from '../../../utils/serviceNew';
import { ToastrService } from 'ngx-toastr';
import { LoanService } from '../../loan.service';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {

  //set title of the component
  productTitle = Constant.productTitle;

  recommendedProducts: RecommendedProductAC[];
  currencySymbol: string;
  isFromProductDetailComponent: boolean;
  loader = true;
  sectionName: string;
  loanApplicationId: string;
  currentSectionName: string;
  ifNoProducts = false;
  loanNeedsRedirectUrl: string = Constant.loanNeedsRedirectUrl;
  isProcessFailed = false;
  interestRateForBestCreditScore: number;

  constructor(private readonly appService: AppService,
    private readonly applicationService: ApplicationService,
    private readonly entityService: EntityService,
    private readonly router: Router,
    private readonly loanService: LoanService,
    private readonly toastrService: ToastrService) {
    this.recommendedProducts = [];
    this.appService.updateLoader(false);
    this.appService.toProductListPage.subscribe(val => this.isFromProductDetailComponent = val);
  }
  
  productDetailsRedirectUrl = Constant.loanProductDetailsRedirectUrl;

  async ngOnInit() {
    this.interestRateForBestCreditScore = await this.loanService.getRateOfInterestForGivenCreditScore(Constant.excellent);
    this.currencySymbol = (await this.appService.getAppSettings()).filter(x => x.fieldName === 'Currency:Symbol')[0].value;
    const lockedApplication = await this.appService.getLockedApplicationJsonAsObject();
    this.loanApplicationId = await this.appService.getCurrentLoanApplicationId();
    const currentStatus = LoanApplicationStatusType[await this.appService.getCurrentLoanApplicationStatus()];
    if(lockedApplication && currentStatus && LoanApplicationStatusType[currentStatus] !== LoanApplicationStatusType.Draft){
      this.router.navigate([this.productDetailsRedirectUrl]);
    } else {
      this.initializeRecommendedProductsList();
    }
  }

  /**
   * Method is to open product detail of particular product
   * @param i Index of selected product from the list
   */
  async sendProductDetail(i: number) {
    await this.appService.setLoanProductDetail(this.recommendedProducts[i]);
  }

  /**
   * Method is to initialize the recommended product list
   */
  async initializeRecommendedProductsList() {
    this.currentSectionName = await this.appService.getCurrentSectionName();
    if (this.currentSectionName !== Constant.loanProduct && !this.isFromProductDetailComponent) {
      this.router.navigate([this.productDetailsRedirectUrl]);
    } else {
      this.appService.redirectToProductListPage(false);
      
      this.applicationService.getRecommendedLoanProducts(this.loanApplicationId).subscribe(
        async (recommendedProductsResult: RecommendedProductAC[]) => {
          this.recommendedProducts = recommendedProductsResult;
          for (let product of this.recommendedProducts) {
            product.descriptionPoints.sort(this.appService.sortByAnyIntegerField);
          }
          const productLocalForage = await this.appService.getLoanProductDetail();
          if (this.recommendedProducts.length === 1 && this.currentSectionName === Constant.loanProduct && !productLocalForage) {
            this.sendProductDetail(0);
            this.router.navigate([this.productDetailsRedirectUrl]);
          } else {
            this.loader = false;
          }
          
        }, (err:ProblemDetails) =>{
          if(err.status === Constant.noContent){
            this.ifNoProducts = true;
          } else {
            this.toastrService.error(err.detail);
          }
          this.loader = false;
        }
      );
      
    }
  }

  /**
   * Method is to check whethere user and company has access to view the product list
   */
  async checkProductListAccess() {
    const entityCurrentUser = await this.appService.getCurrentUserDetailsNew();
    const currentEntityId = await this.appService.getCurrentCompanyId(); 
    let currentNAICSIndustryCode = null;
    if(currentEntityId === null){
      this.router.navigate([Constant.routeRootUrl]);
    }

    return this.entityService.getEntity(currentEntityId).toPromise().then(
      async (entity: EntityAC) =>{
        currentNAICSIndustryCode =  entity.company.industryType.industryCode;

          if(this.appService.convertResidencyStatusEnumNumberToString(entityCurrentUser.user.residencyStatus) === Constant.nonResident){
            return false;
          }
      
          const naicsIndustryCodeExcluded = (await this.appService.getAppSettings()).filter(x => x.fieldName === 'Product:NAICSIndustryCodeExcluded')[0].value;
          const naicsIndustryCodeExcludedList = naicsIndustryCodeExcluded.split(',');
          return !(currentNAICSIndustryCode !== null && naicsIndustryCodeExcludedList.includes(currentNAICSIndustryCode));
      }
    );
  }
}

@Pipe({
  name: 'startingValue'
})
export class ProductAmountRangeStartingValuePipe implements PipeTransform {
  transform(amountRange: string, by: string, index = 0) {
    const arr = amountRange.split(by);
    return arr[index].trim();
  }
}

@Pipe({
  name: 'endingValue'
})
export class ProductAmountRangeEndingValuePipe implements PipeTransform {
  transform(amountRange: string, by: string, index = 1) {
    const arr = amountRange.split(by);
    return arr[index].trim();
  }
}
