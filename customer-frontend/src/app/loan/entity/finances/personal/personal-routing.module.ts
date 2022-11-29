import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { PersonalComponent } from './personal.component';
import { AuthGuard } from '../../../../services/auth-guard.service';

const routes: Routes = [
    {
        path: '', component: PersonalComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard],
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class PersonalRoutingModule { }
