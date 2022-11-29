import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { TaxesComponent } from './taxes.component';
import { AuthGuard } from '../../../../services/auth-guard.service';

const routes: Routes = [
    {
        path: '', component: TaxesComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard],
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class TaxesRoutingModule { }
