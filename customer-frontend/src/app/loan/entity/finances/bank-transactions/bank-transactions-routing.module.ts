import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { BankTransactionsComponent } from './bank-transactions.component';
import { AuthGuard } from '../../../../services/auth-guard.service';

const routes: Routes = [
    {
        path: '', component: BankTransactionsComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard],
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class BankTransactionsRoutingModule { }
