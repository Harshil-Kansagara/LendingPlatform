import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoanComponent } from './loan.component';
import { AuthGuard } from '../services/auth-guard.service';
import { CreditProfileComponent } from '../credit-profile/credit-profile.component';


const routes: Routes = [
  { path: '', component: LoanComponent },
  { path: 'profile', component: CreditProfileComponent },
  { path: 'needs', loadChildren: () => import('./loan-needs/loan-needs.module').then(m => m.LoanNeedsModule) },
  { path: 'company', loadChildren: () => import('./entity/entity.module').then(m => m.EntityModule) },
  { path: 'products', loadChildren: () => import('./products/products.module').then(m => m.LoanProductsModule) },
  { path: 'consent', loadChildren: () => import('./loan-consent/loan-consent.module').then(m => m.LoanConsentModule) },
  { path: 'status', loadChildren: () => import('./loan-status/loan-status.module').then(m => m.LoanStatusModule) },
  { path: 'bank-details', loadChildren: () => import('./bank-details/bank-details.module').then(m => m.BankDetailsModule) },
  { path: 'additional-documents', loadChildren: () => import('./additional-documents/additional-documents.module').then(m => m.AdditionalDocumentsModule) },
  { path: 'personal', loadChildren: () => import('./entity/entity.module').then(m => m.EntityModule) },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LoanRoutingModule { }
