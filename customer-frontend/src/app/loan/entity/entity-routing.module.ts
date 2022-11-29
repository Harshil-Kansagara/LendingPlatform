import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { EntityComponent } from './entity.component';
import { AuthGuard } from '../../services/auth-guard.service';


const routes: Routes = [
  {
    path: '', component: EntityComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard],
    children: [
      { path: 'info', loadChildren: () => import('./company-info/company-info.module').then(m => m.CompanyInfoModule) },
      { path: 'finances', loadChildren: () => import('./finances/finances.module').then(m => m.FinancesModule) }
    ]
  },
  {
    path: 'quickbooksredirect', component: EntityComponent

  },
  { path: 'xeroredirect', component: EntityComponent },
  { path: 'paypalredirect', component: EntityComponent },
  { path: 'striperedirect', component: EntityComponent },
  { path: 'squareredirect', component: EntityComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EntityRoutingModule { }
