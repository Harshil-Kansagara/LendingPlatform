import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ProductDetailsComponent } from './product-details.component';
import { ProductDetailsRoutingModule } from './product-details-routing.module';
import { NgxMaskModule } from 'ngx-mask';
import { NgxSliderModule } from '@m0t0r/ngx-slider';
import { ComponentHeaderModule } from '../../../../layout/component-header/component-header.module';
@NgModule({
  declarations: [ProductDetailsComponent],
  imports: [
    CommonModule,
    FormsModule,ReactiveFormsModule,
    ProductDetailsRoutingModule,
    NgxMaskModule.forRoot(),
    NgxSliderModule,
    ComponentHeaderModule
  ]
})
export class ProductDetailsModule { }
