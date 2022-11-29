import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoanConsentComponent } from './loan-consent.component';
import { AuthGuard } from '../../services/auth-guard.service';

const routes: Routes = [{ path: '', component: LoanConsentComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard] }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LoanConsentRoutingModule { }
