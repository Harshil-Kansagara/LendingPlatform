import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoanStatusComponent } from './loan-status.component';
import { AuthGuard } from '../../services/auth-guard.service';

const routes: Routes = [{ path: '', component: LoanStatusComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard] }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LoanStatusRoutingModule { }
