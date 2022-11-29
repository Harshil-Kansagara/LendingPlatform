import { BrowserModule } from '@angular/platform-browser';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { AppComponent } from './app.component';
import { LayoutComponent } from './layout/layout.component';
import { HeaderComponent } from './layout/header/header.component';
import { BannerSidebarComponent } from './layout/banner-sidebar/banner-sidebar.component';
import { SidenavComponent } from './layout/sidenav/sidenav.component';
import { HeaderLayoutComponent } from './layout/header-layout/header-layout.component';
import { LandingPageComponent } from './landing-page/landing-page.component';
import { LoanApplicationService, CompanyInfoService, API_BASE_URL, UserService } from './utils/service';
import { OidcConfigService, LogLevel, PublicEventsService, EventTypes, AuthModule } from 'angular-auth-oidc-client';
import { filter } from 'rxjs/operators';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpInterceptorService } from './services/http-interceptor.service';
import { environment } from '../environments/environment';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { ComponentHeaderModule } from './layout/component-header/component-header.module';
import { EmiCalculatorComponent } from './loan/emi-calculator/emi-calculator.component';
import { CreditProfileComponent } from './credit-profile/credit-profile.component';
import { ItemNotFoundModule } from './layout/item-not-found/item-not-found.module';
// npm library
import { NgxSliderModule } from '@m0t0r/ngx-slider';
import { CarouselModule } from 'ngx-owl-carousel-o';
//Ngx Bootstrap Module
import { CollapseModule } from 'ngx-bootstrap/collapse';
import { ModalModule } from 'ngx-bootstrap/modal';
import { ProgressbarModule } from 'ngx-bootstrap/progressbar';
import { GlobalService, EntityService, ApplicationService, API_BASE_URL_NEW } from './utils/serviceNew';
import { NgxMaskModule } from 'ngx-mask';
import { MiniProfilerModule, MiniProfilerInterceptor } from 'ng-miniprofiler';
import { MiniProfilerConfig } from 'ng-miniprofiler/lib/miniprofiler-config.model';
import { v4 as uuidv4 } from 'uuid';

export function configureAuth(oidcConfigService: OidcConfigService) {
  return () =>
    oidcConfigService.withConfig({
      stsServer: environment.identityServer.authorityUrl,
      redirectUrl: `${window.location.origin}/loan`,
      postLogoutRedirectUri: window.location.origin,
      clientId: environment.identityServer.clientId,
      scope: environment.identityServer.scopes,
      responseType: 'code',
      silentRenew: false,
      useRefreshToken: true,
      logLevel: LogLevel.Error,
      customParams: {
        prompt: "login"
      }
    });
}

@NgModule({
  declarations: [
        AppComponent,
    LayoutComponent,
    HeaderComponent,
    SidenavComponent,
    HeaderLayoutComponent,
    PageNotFoundComponent,
    LandingPageComponent,
    BannerSidebarComponent,
    EmiCalculatorComponent,
    CreditProfileComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ItemNotFoundModule,
    FormsModule,
    ReactiveFormsModule,
    NgSelectModule,
    NgxSliderModule,
    ComponentHeaderModule,
    NgSelectModule,
    AuthModule.forRoot(),
    BrowserAnimationsModule,
    ToastrModule.forRoot({
      timeOut: 3000,
      positionClass: 'toast-top-right',
    }),
    CollapseModule,
    ModalModule.forRoot(),
    CarouselModule,
    ProgressbarModule.forRoot(),
    NgxMaskModule.forRoot(),
    MiniProfilerModule.forRoot({
      baseUri: environment.apiBaseUrl,
      colorScheme: environment.miniProfiler.colorScheme as MiniProfilerConfig['colorScheme'],
      maxTraces: environment.miniProfiler.maxTraces,
      position: environment.miniProfiler.position as MiniProfilerConfig['position'],
      toggleShortcut: environment.miniProfiler.toggleShortcut,
      showControls: environment.miniProfiler.showControls,
      enabled: environment.miniProfiler.enabled,
      enableGlobalMethod: environment.miniProfiler.enableGlobalMethod
    })
  ],
  providers: [
    GlobalService,
    EntityService,
    LoanApplicationService,
    CompanyInfoService,
    UserService,
    GlobalService,
    EntityService,
    ApplicationService,
    OidcConfigService,
    {
      provide: APP_INITIALIZER,
      useFactory: configureAuth,
      deps: [OidcConfigService],
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpInterceptorService,
      multi: true,
    },
    { provide: HTTP_INTERCEPTORS, useClass: MiniProfilerInterceptor, multi: true },
    {
      provide: API_BASE_URL,
      useValue: environment.apiBaseUrl
    },
    {
      provide: API_BASE_URL_NEW,
      useValue: environment.apiBaseUrl
    }
  ],
  bootstrap: [AppComponent]
})

export class AppModule {
  constructor(private readonly eventService: PublicEventsService) {
    this.eventService
      .registerForEvents()
      .pipe(filter((notification) => notification.type === EventTypes.ConfigLoaded))
      .subscribe((config) => {

      });
    // assigning unique window name to create separate instance of localforage
    if (!window.name) {
      window.name = uuidv4();
    }
  }
}
