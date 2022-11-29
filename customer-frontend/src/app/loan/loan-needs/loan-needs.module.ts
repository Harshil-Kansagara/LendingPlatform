import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LoanApplicationService } from 'src/app/utils/service';
import { LoanNeedsComponent } from './loan-needs.component';
import { LoanNeedsRoutingModule } from './loan-needs-routing.module';
import { NgxMaskModule } from 'ngx-mask';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgxSliderModule } from '@m0t0r/ngx-slider';
import { ComponentHeaderModule } from '../../layout/component-header/component-header.module';
@NgModule({
  declarations: [LoanNeedsComponent],
  imports: [
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    LoanNeedsRoutingModule,
    NgxMaskModule.forRoot(),
    NgxSliderModule,
    NgSelectModule,
    ComponentHeaderModule
  ],
  providers: [
    LoanApplicationService
  ]
})
export class LoanNeedsModule { }
