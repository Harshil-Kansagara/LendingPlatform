import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ProductDetailsComponent } from './product-details.component';
import { AuthGuard } from '../../../../services/auth-guard.service';

const routes: Routes = [
    {
    path: '', component: ProductDetailsComponent, canActivate: [AuthGuard], canActivateChild: [AuthGuard]
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ProductDetailsRoutingModule { }
