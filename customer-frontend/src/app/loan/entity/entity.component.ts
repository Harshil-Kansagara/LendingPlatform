import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import localForage from 'localforage';
import { Constant } from '../../shared/constant';
import { AppService } from '../../services/app.service';

@Component({
  selector: 'app-borrower',
  templateUrl: './entity.component.html',
  styleUrls: ['./entity.component.scss']
})
export class EntityComponent implements OnInit {

  constructor(private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly appService: AppService) { }

  code: string = null;
  state: string = null;
  realmId: string = null;
  invoicesAuthorizationCode: string = null;
  invoicesState: string = null;

  async ngOnInit(): Promise<void> {

    // Get the current url route.
    const routeUrl = this.router.url;
    const isQuickbooks = (routeUrl.search('/quickbooksredirect') !== -1);
    const isXero = (routeUrl.search('/xeroredirect') !== -1);
    const isPayPal = (routeUrl.search('/paypalredirect') !== -1);
    const isStripe = (routeUrl.search('/striperedirect') !== -1);
    const isSquare = (routeUrl.search('/squareredirect') !== -1);

    // If Quickbooks and Xero then bind variable and close the window.
    if (isQuickbooks || isXero) {

      this.route.queryParams.subscribe(async data => {
        // code and state are common variable for both..
        this.code = data['code'];
        this.state = data['state'];
        
        await this.appService.setFinanceCodeParam(this.code);
        await this.appService.setFinanceStateParam(this.state);

        // realmId is only return from quickbooks.
        if (isQuickbooks) {
          this.realmId = data['realmId'];
          await this.appService.setRealmIdParam(this.realmId);
        }
        window.close();
      });

    }

    //If PayPal or Stripe or Square then store the required values in local forage and close the window.
    if (isPayPal || isStripe || isSquare) {
      this.route.queryParams.subscribe(async data => {
        //Set authorization code and state of paypal account in local forage.
        if (window.location.origin.search(Constant.localHostIpAddressUrl) !== -1 && isPayPal) {
          window.open(Constant.payPalPopUpCloseLocalHostRedirectUrl + data['code'] + Constant.payPalStateParameterInRedirectUrl + data['state'], '_self');
        } else {
          this.invoicesAuthorizationCode = data['code'];
          this.invoicesState = data['state'];
          await localForage.setItem('invoices_authorization_code', this.invoicesAuthorizationCode);
          await localForage.setItem('invoices_state', this.invoicesState);
          window.close();
        }
      });
    }
  }

}
