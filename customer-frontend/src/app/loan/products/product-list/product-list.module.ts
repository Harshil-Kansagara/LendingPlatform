import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {ProductAmountRangeEndingValuePipe, ProductAmountRangeStartingValuePipe, ProductListComponent} from './product-list.component';
import {ProductListRoutingModule} from './product-list-routing.module';
import { ComponentHeaderModule } from '../../../layout/component-header/component-header.module';
import { ItemNotFoundModule } from '../../../layout/item-not-found/item-not-found.module';
import { NgxMaskModule } from 'ngx-mask';

@NgModule({
declarations: [ProductListComponent, ProductAmountRangeStartingValuePipe, ProductAmountRangeEndingValuePipe],
  imports: [
    CommonModule,
    ProductListRoutingModule,
    ComponentHeaderModule,
    ItemNotFoundModule,
    NgxMaskModule.forRoot()
  ]
})
export class ProductListModule { }
