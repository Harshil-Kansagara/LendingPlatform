import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { Constant } from '../../../../shared/constant';
import { AppService } from '../../../../services/app.service';
import { CompanyInfoService, CompanyAC, InvoiceRequestParametersAC, InvoiceListAC, InvoiceAC, FinancialInformationFrom } from '../../../../utils/service';
import * as localForage from "localforage";
import { ToastrService } from 'ngx-toastr';
import { EventEmitterService } from '../../../../services/event-emitter.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-invoices',
  templateUrl: './invoices.component.html'
})
export class InvoicesComponent implements OnInit, OnDestroy {

  /**
  * Creates an instance of documenter.
  */
  constructor(private readonly appService: AppService,
    private readonly companyInfoService: CompanyInfoService,
    private readonly toastrService: ToastrService,
    private readonly eventEmitterService: EventEmitterService,
    private readonly router: Router) {
    this.appService.updateLoader(false);
    this.appService.updateRoute(Constant.financesRedirectUrl);
    this.appService.updateNextRoute(Constant.transactionsRedirectUrl);
  }

  @Input() consentVisible = true;
  @Input() connected = false;
  @Input() viewOnly = true;
  loader = false;
  isPaypal = false;
  isStripe = false;
  isSquare = false;
  payPal = Constant.paypal;
  stripe = Constant.stripe;
  square = Constant.square;
  subsVar: Subscription;

  entityId: string;
  loanApplicationId: string;
  currentSectionName: string;

  userName: string = null;
  @Input() data: InvoiceListAC;
  startingYear: string;
  endingYear: string;

  async ngOnInit() {
    if (this.router.url.search(Constant.invoices.toLowerCase()) !== -1) {
      this.disableSaveButton();
      this.setLocalForageVariables().then(x => this.getInvoices());
      this.subsVar = this.eventEmitterService.
        invokeSaveFunctionInInvoices.subscribe(() => {
          this.router.navigate([Constant.transactionsRedirectUrl]);
        });
    }
    if (this.data !== null && this.data !== undefined) {
      this.data.invoices = this.setCurrencySymbol(this.data.invoices);
      this.data.invoices.forEach(x => x.invoiceDate = x.invoiceDate.split("-").reverse().join("-"));
      this.data.invoices.sort(this.appService.sortByDate);
    }
  }

  /**
  * Get the invoices from the database.
  */
  getInvoices() {
    this.loader = true;

    this.companyInfoService.getInvoices(this.entityId, this.loanApplicationId).subscribe(
      async (resp: InvoiceListAC) => {
        if (resp) {
          this.data = resp;

          //If the invoices are not empty then only set the values.
          if (resp.invoices.length !== 0) {
            this.userName = resp.invoices[0].invoicerEmail?.split("@")[0];
            this.data.invoices = this.setCurrencySymbol(this.data.invoices);
            this.data.invoices.forEach(x => x.invoiceDate = x.invoiceDate.split("-").reverse().join("-"));
            this.data.invoices.sort(this.appService.sortByDate);
          }
          else {
            if (this.router.url.search(Constant.invoices.toLowerCase()) !== -1) {
              this.toastrService.info(Constant.noInvoicesAreSavedPleaseSave);
            }
            this.disableSaveButton();
          }
          this.startingYear = resp.startingYear.split(" - ")[0] + "-" + resp.startingYear.split(" ")[3];
          this.endingYear = resp.endingYear.split(" - ")[1].replace(" ", "-");

          //Check the API name and change boolean flag accordingly.
          if (resp.financialInformationFrom === FinancialInformationFrom.PayPal) {
            this.isPaypal = true;
            this.isSquare = false;
            this.isStripe = false;
            this.connected = true;
          }
          else if (resp.financialInformationFrom === FinancialInformationFrom.Stripe) {
            this.isPaypal = false;
            this.isSquare = false;
            this.isStripe = true;
            this.connected = true;
          }
          else if (resp.financialInformationFrom === FinancialInformationFrom.Square) {
            this.isPaypal = false;
            this.isSquare = true;
            this.isStripe = false;
            this.connected = true;
          }
          if (this.router.url.search(Constant.invoices.toLowerCase()) !== -1) {
            this.showContinueButton();
          }
          else {
            this.hideContinueButton();
          }

          //If the current section order is as same as Invoices then only update the section name.
          if (this.appService.getSectionNumber(Constant.invoices) === this.appService.getSectionNumber(this.currentSectionName) && this.data !== null) {
            this.appService.updateCurrentSectionName(Constant.transactions);
          }
        }
        else {
          this.connected = false;
          this.disableSaveButton();
          this.hideContinueButton();
        }
        this.loader = false;
      },
      (err) => {
        if (err.status === 400 && (this.appService.getSectionNumber(Constant.invoices) !== this.appService.getSectionNumber(this.currentSectionName))) {
          this.toastrService.error(err.response);
        }
        this.loader = false;
        this.connected = false;
        this.disableSaveButton();
        this.hideContinueButton();
      });
  }

  /**
   * Connect with selected service.
   * @param service
   */
  async connectWith(service: string) {

    if (service === this.payPal) {
      this.loader = true;
      //Get paypal authorization url.
      this.companyInfoService.getPayPalAuthorizationUrl(new CompanyAC(await this.appService.getCurrentCompanyDetails()).id).subscribe(
        (data) => {
          this.openPopUp(data, service);
        },
        (err) => {
          this.toastrService.error(err.response);
          this.loader = false;
        });
    }
    else if (service === this.stripe) {
      this.loader = true;
      //Get stripe authorization url.
      this.companyInfoService.getStripeAuthorizationUrl(new CompanyAC(await this.appService.getCurrentCompanyDetails()).id).subscribe(
        (data) => {
          this.openPopUp(data, service);
        },
        (err) => {
          this.toastrService.error(err.response);
          this.loader = false;
        });
    }
    else if (service === this.square) {
      this.loader = true;
      //Get square authorization url.
      this.companyInfoService.getSquareAuthorizationUrl(new CompanyAC(await this.appService.getCurrentCompanyDetails()).id).subscribe(
        (data) => {
          this.openPopUp(data, service);
        },
        (err) => {
          this.toastrService.error(err.response);
          this.loader = false;
        });
    }
  }

  /**
   * Method to open pop up.
   * @param data
   * @param service
   */
  openPopUp(data: string, service: string) {
    if (data !== null) {
      const winObj = window.open(data, "newwindow", 'width=510,height=424');
      const self = this;
      const loop = setInterval(function () {
        if (winObj.closed) {
          clearInterval(loop);
          self.invoicesRedirectHandlerAsync(service);
        }
      }, 1000);
    }
  }

  /**
   * Method will be executed immediately after popup closing
   * @param service
   */
  async invoicesRedirectHandlerAsync(service) {
    const code = await this.getInvoiceAuthorizationCode();
    const state = await this.getInvoiceState();

    if (code && state) {

      const request = new InvoiceRequestParametersAC();
      request.authorizationCode = code;
      request.state = state;
      if (service === this.payPal) {
        request.financialInformationFrom = FinancialInformationFrom.PayPal;
      } else if (service === this.stripe) {
        request.financialInformationFrom = FinancialInformationFrom.Stripe;
      } else if (service === this.square) {
        request.financialInformationFrom = FinancialInformationFrom.Square;
      }
      request.loanApplicationId = await this.appService.getCurrentLoanApplicationId();

      this.companyInfoService.saveInvoices(request).subscribe(
        async (resp) => {
          if (resp) {
            await localForage.removeItem("invoices_authorization_code");
            await localForage.removeItem("invoices_state");

            this.data = resp;
            this.userName = resp.invoices[0].invoicerEmail?.split("@")[0];
            this.startingYear = resp.startingYear.split(" - ")[0] + "-" + resp.startingYear.split(" ")[3];
            this.endingYear = resp.endingYear.split(" - ")[1].replace(" ", "-");
            this.data.invoices = this.setCurrencySymbol(this.data.invoices);
            this.data.invoices.forEach(x => x.invoiceDate = x.invoiceDate.split("-").reverse().join("-"));
            this.data.invoices.sort(this.appService.sortByDate);

            if (resp.financialInformationFrom === FinancialInformationFrom.PayPal) {
              this.isPaypal = true;
              this.isStripe = false;
              this.isSquare = false;
            } else if (resp.financialInformationFrom === FinancialInformationFrom.Stripe) {
              this.isPaypal = false;
              this.isStripe = true;
              this.isSquare = false;
            } else if (resp.financialInformationFrom === FinancialInformationFrom.Square) {
              this.isPaypal = false;
              this.isStripe = false;
              this.isSquare = true;
            }
            this.connected = true;
            this.loader = false;
            this.showContinueButton();

            if (this.currentSectionName === Constant.invoices) {
              await this.appService.updateCurrentSectionName(Constant.transactions);
            }
          }
          else {
            this.toastrService.info(Constant.noInvoicesAreSavedPleaseSave);
            this.loader = false;
            this.connected = false;
            this.disableSaveButton();
            this.hideContinueButton();
          }
        },
        (err) => {
          this.loader = false;
          this.connected = false;
          this.disableSaveButton();
          this.toastrService.error(err.response);
        });
    }
    else {
      this.clearConnection();
    }
  }

  /**
   * Get the authorization code value from the local forage.
   */
  async getInvoiceAuthorizationCode(): Promise<string> {
    return localForage.getItem("invoices_authorization_code");
  }

  /**
   * Get the state parameter value from the local forage.
   */
  async getInvoiceState(): Promise<string> {
    return localForage.getItem("invoices_state");
  }

  /**
   * Set the currency symbol along with amount of invoice.
   */
  setCurrencySymbol(invoices: InvoiceAC[]): InvoiceAC[] {
    for (var invoice of invoices) {
      invoice.totalAmount = `${Constant[invoice.currencyCode.toLowerCase()]} ${invoice.totalAmount}`;
    }
    return invoices;
  }

  /**
   * Set values from local forage for all the required variables.
   */
  async setLocalForageVariables(): Promise<void> {
    this.loanApplicationId = await this.appService.getCurrentLoanApplicationId();
    this.entityId = new CompanyAC(await this.appService.getCurrentCompanyDetails()).id;
    this.currentSectionName = await this.appService.getCurrentSectionName();
  }

  /**
   * Enable save button.
   */
  enableSaveButton(): void {
    this.eventEmitterService.disableSaveButton(false);
  }

  /**
   * Disable save button.
   */
  disableSaveButton(): void {
    this.eventEmitterService.disableSaveButton(true);
  }

  /**
   * Show Continue button and Hide Save & Continue button.
   */
  showContinueButton(): void {
    this.eventEmitterService.showContinueButton(true);
  }

  /**
   * Hide Continue button and Show Save & Continue button.
   */
  hideContinueButton(): void {
    this.eventEmitterService.showContinueButton(false);
  }

  /**
   * On 'Clear' button click, to go to the connection page.
   */
  clearConnection() {
    this.connected = false;
    this.isPaypal = false;
    this.isStripe = false;
    this.isSquare = false;
    this.loader = false;
    this.hideContinueButton();
    //Disable save button.
    this.disableSaveButton();
  }

  ngOnDestroy(): void {
    this.enableSaveButton();
    this.hideContinueButton();
    if (this.subsVar) {
      this.subsVar.unsubscribe();
    }
  }
}
