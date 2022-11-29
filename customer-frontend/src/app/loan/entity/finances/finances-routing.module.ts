import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { FinancesComponent } from './finances.component';
import { AuthGuard } from '../../../services/auth-guard.service';

const routes: Routes = [
    {
        path: '', canActivate: [AuthGuard], canActivateChild: [AuthGuard],
        component: FinancesComponent, children: [
            { path: 'statements', loadChildren: () => import('./statements/statements.module').then(m => m.StatementsModule) },
            { path: 'invoices', loadChildren: () => import('./invoices/invoices.module').then(m => m.InvoicesModule) },
            { path: 'transactions', loadChildren: () => import('./bank-transactions/bank-transactions.module').then(m => m.BankTransactionsModule) },            
            { path: 'taxes', loadChildren: () => import('./taxes/taxes.module').then(m => m.TaxesModule) },
            { path: '', loadChildren: () => import('./personal/personal.module').then(m => m.PersonalModule) }
        ]
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class FinancesRoutingModule { }
