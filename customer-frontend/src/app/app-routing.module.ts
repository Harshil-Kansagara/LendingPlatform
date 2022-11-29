import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { HeaderLayoutComponent } from './layout/header-layout/header-layout.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { LandingPageComponent } from './landing-page/landing-page.component';
import { BannerSidebarComponent } from './layout/banner-sidebar/banner-sidebar.component';
import { EmiCalculatorComponent } from './loan/emi-calculator/emi-calculator.component';
import { CreditProfileComponent } from './credit-profile/credit-profile.component';

const routes: Routes = [
  {
    path: '',
    component: HeaderLayoutComponent,
    children: [
      { path: '', component: LandingPageComponent },
      { path: 'notfound', component: PageNotFoundComponent }
    ]
  },
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: 'loan', loadChildren: () => import('./loan/loan.module').then(m => m.LoanModule) }
    ]
  },
  {
    path: '',
    component: BannerSidebarComponent,
    children: [
        { path: 'profile', component: CreditProfileComponent },
        { path: 'calculator', component: EmiCalculatorComponent },
    ]
  },

  {
    path: "**",
    redirectTo: "/notfound"
  }

];

@NgModule({
  imports: [RouterModule.forRoot(routes, { scrollPositionRestoration: 'enabled' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
