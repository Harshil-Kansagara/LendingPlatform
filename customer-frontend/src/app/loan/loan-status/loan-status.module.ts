import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {LoanStatusRoutingModule} from './loan-status-routing.module';
import {LoanStatusComponent} from './loan-status.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { ComponentHeaderModule } from '../../layout/component-header/component-header.module';
import { CollapseModule } from 'ngx-bootstrap/collapse';
@NgModule({
  declarations: [LoanStatusComponent],
  imports: [
    CommonModule,
    LoanStatusRoutingModule,
    ComponentHeaderModule,
    NgSelectModule,
    CollapseModule
  ]
})
export class LoanStatusModule { }
