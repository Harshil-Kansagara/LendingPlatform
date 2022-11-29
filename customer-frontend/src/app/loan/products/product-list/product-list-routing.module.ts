import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ProductListComponent } from './product-list.component';

const routes: Routes = [
    {
        path: '', component: ProductListComponent
    },
    {
      path:'', children:[      
      { path: 'detail', loadChildren: () => import('./product-details/product-details.module').then(m => m.ProductDetailsModule) }
      ]
    }
];

@NgModule({
  declarations: [],
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProductListRoutingModule { }
