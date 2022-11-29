import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';



import { AuthGuard } from '../../services/auth-guard.service';
import { AdditionalDocumentsComponent } from './additional-documents.component';

const routes: Routes = [
  { path: '', component: AdditionalDocumentsComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdditionalDocumentsRoutingModule { }
