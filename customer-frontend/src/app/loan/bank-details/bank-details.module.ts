import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BankDetailsRoutingModule } from './bank-details-routing.module';
import { BankDetailsComponent } from './bank-details.component';
import { ReactiveFormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { ComponentHeaderModule } from '../../layout/component-header/component-header.module';
@NgModule({
  declarations: [BankDetailsComponent],
  imports: [
    CommonModule,
    BankDetailsRoutingModule,
    ReactiveFormsModule,
    NgSelectModule,
    ComponentHeaderModule
  ]
})
export class BankDetailsModule { }
