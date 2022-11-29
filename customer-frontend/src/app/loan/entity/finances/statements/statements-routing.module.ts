import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { StatementsComponent } from './statements.component';
import { AuthGuard } from '../../../../services/auth-guard.service';

const routes: Routes = [
    {
        path: '', component: StatementsComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard]
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class StatementsRoutingModule { }
