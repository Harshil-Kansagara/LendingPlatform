import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { InvoicesComponent } from './invoices.component';
import { AuthGuard } from '../../../../services/auth-guard.service';

const routes: Routes = [
    {
        path: '', component: InvoicesComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard],
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class InvoicesRoutingModule { }
