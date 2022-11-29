import { Component, OnInit } from '@angular/core';
import { AppService } from '../../services/app.service';
import { EntityAC } from '../../utils/serviceNew';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Constant } from '../../shared/constant';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {

  constructor(private readonly appService: AppService,
    private readonly oidcSecurityService: OidcSecurityService) { }

  
  isCollapsed = true;
  isLoggedOut = true;
  selectedCompany: string;
  companies = [];
  registrationUrl = window.location.origin.indexOf('dev') === -1 ? environment.identityServer.registrationUrl : 'https://auth.jamoon.net/auth/realms/development/protocol/openid-connect/registrations?client_id=LendingPlatform.Web.Client&redirect_uri=https://customer-dev.jamoon.net&response_type=code&scope=openid%20profile%20LendingPlatform.Web.Client%20offline_access';
  username: string = null;
  overflow: boolean;
  selectedThemeLogo: string;
  cities3 = [];
  async ngOnInit() {
    this.cities3 = environment.availableThemes.filter(x => x.isEnabled);

    if (this.cities3 && this.cities3.length > 1) {
      const selectedTheme = await this.appService.getSelectedTheme();
      if (selectedTheme) {
        this.selectedThemeLogo = selectedTheme.name;
      } else {
        this.selectedThemeLogo = Constant.defaultTheme;
      }
    }else {
      this.selectedThemeLogo = this.cities3[0]?.name;
      this.themeChanged();
    }
    
    
    this.appService.isAuthenticated().subscribe(async isAuthorized => {
      if (isAuthorized) {

        //fetch the current user details. If not available then subscribe to the required fields to get the values.
        const currentUser = await this.appService.getCurrentUserDetailsNew();
        if (currentUser === null || currentUser === undefined) {
          this.appService.currentUserName.subscribe(val => this.username = val);
          this.appService.currentUserEntityList.subscribe((val: EntityAC[]) => {
            this.setCompanies(val);
          });
        } else {
          this.isLoggedOut = false;
          this.username = (`${currentUser.user.firstName} ${currentUser.user.lastName}`);
          this.appService.currentUserEntityList.subscribe(val => {
            this.setCompanies(val);
          });
        }
      } else {
        this.isLoggedOut = true;
      }
    });
  }

  /**
   * Method sets the companies for dropdown.
   * @param list List of entity (company) linked with current user.
   */
  setCompanies(list: EntityAC[]) {
    if (list.length !== 0) {
      for (const entity of list) {
        this.companies.push({ id: entity.id, name: entity.company.name });
      }
      this.selectedCompany = this.companies.length !== 0 ? this.companies[0].name : '';
    }
  }

  /**
   * Method set the current selected company in the app service variable to use it in landing page. 
   * @param event Event of ngSelect
   */
  selectCompany(event: { id: string, name: string }) {
    this.appService.removeAllDataFromLocalForage();
    this.appService.updateSelectedEntity(event);
  }

  /**
   * Method logs out the current user.
   */
  logoff() {
    this.appService.logoff();
  }

  /**
   * Method redirect user to login page.
   */
  login() {
    this.oidcSecurityService.authorize();
  }

  // Method to redirect user to register page
  register() {
    this.oidcSecurityService.authorize({
      urlHandler: url => {
        const substring = '/auth?';
        window.location.href = `${environment.identityServer.registrationUrl}?${url.substr(url.indexOf(substring) + substring.length)}`;
      }
    });

  }

  // Change theme
  async themeChanged() {
    const customThemeTag = document.getElementById('custom_theme');
    const customThemeFavicon = document.getElementById('custom_favicon');
    if (customThemeTag) {
      customThemeTag.remove();
    }

    if (customThemeFavicon) {
      customThemeFavicon.remove();
    }

    const selectedTheme = this.cities3.filter(x => x.name === this.selectedThemeLogo)[0];

    await this.appService.setSelectedTheme(selectedTheme);
    if (this.cities3 && this.cities3.length > 1) {
      window.location.reload();
    }
    

  }
}
