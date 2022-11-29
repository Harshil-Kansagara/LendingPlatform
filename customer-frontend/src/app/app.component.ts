import { Component, HostListener} from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Router, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { AppService } from './services/app.service';
import { Constant } from './shared/constant';
import { Title } from '@angular/platform-browser';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'ClientApp';

  constructor(private readonly oidcSecurityService: OidcSecurityService,
    private readonly router: Router,
    private readonly location: Location,
    private readonly route: ActivatedRoute,
    private readonly appService: AppService,
    private readonly titleService: Title) { }

  @HostListener('window:beforeunload', ['$event'])
  onWindowClose(event: Event){
    //this.appService.dropInstance();

    //If file uploading is in progress then warns user.
    if (this.appService.taxFileUploadingInProgress || this.appService.additionalDocumentUploadingInProgress) {
      event.returnValue = confirm(Constant.uploadingInProgressLeaveAnyway);
    }
  }

  async ngOnInit() {
    await this.themeChanged();
    const currentPath = this.location.path();
    // It will not check authentication while redirect url from quickbooks, xero, paypal, stripe or square.
    const redirections = ['/quickbooksredirect', '/xeroredirect', '/striperedirect', '/paypalredirect', '/squareredirect'];

    if (redirections.filter(x => currentPath.search(x) !== -1).length === 0) {
      this.oidcSecurityService.checkAuthIncludingServer().subscribe((isAuthenticated) => {
        if (!isAuthenticated) {
          this.router.navigate(['']);
        }
        this.route.queryParams.subscribe(params => {
          let code = params['code'];
                   
          // When there is redirection from identity server
          if (code) {
            let token = this.oidcSecurityService.getToken();
            localStorage.setItem('access_token', token);
            this.router.navigate(['loan']).then(x => window.location.reload());
          }
        });
      });
    }
  }

  async themeChanged() {
    const customThemeTag = document.getElementById('custom_theme');
    const customThemeFavicon = document.getElementById('custom_favicon');
    if (customThemeTag) {
      customThemeTag.remove();
    }

    if (customThemeFavicon) {
      customThemeFavicon.remove();
    }

    const selectedTheme = await this.appService.getSelectedTheme();

    // Change tab Favicon
    const faviconLink = this.createLinkElement('custom_favicon', 'image/x-icon', 'icon');
    if (selectedTheme && selectedTheme.name !== Constant.defaultTheme) {

      // Change css
      const link = this.createLinkElement('custom_theme', 'text/css', 'stylesheet');
      link.href = selectedTheme.cssUrl;

      // Change tab title
      this.titleService.setTitle(selectedTheme.name);

      // Change favicon
      faviconLink.setAttribute('href', selectedTheme.faviconUrl);

    }else {
      const defaultTheme = environment.availableThemes.filter(x => x.name === Constant.defaultTheme)[0];
      this.appService.setSelectedTheme(defaultTheme);
      faviconLink.setAttribute('href', defaultTheme.faviconUrl);

      // Reset tab title
      this.titleService.setTitle(Constant.defaultTitle);

    }

  }


  // Create link element
  createLinkElement(id, type, rel) {
    const headId = document.getElementsByTagName('head')[0];
    const link = document.createElement('link');
    link.type = type;
    link.id = id;
    link.rel = rel;
    headId.appendChild(link);
    return link;
  }
}
