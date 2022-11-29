import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';


import { LoanNeedsComponent } from './loan-needs.component';
import { AuthGuard } from '../../services/auth-guard.service';

const routes: Routes = [
    { path: '', component: LoanNeedsComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LoanNeedsRoutingModule { }
