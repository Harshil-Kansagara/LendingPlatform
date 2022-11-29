import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvoicesComponent } from './invoices.component';
import {InvoicesRoutingModule} from './invoices-routing.module';
@NgModule({
  declarations: [InvoicesComponent],
  imports: [
    CommonModule,
    InvoicesRoutingModule
  ],
  exports: [InvoicesComponent]
})
export class InvoicesModule { }
