import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoanComponent } from './loan.component';
import { LoanRoutingModule } from './loan-routing.module';
import { LoanService } from './loan.service';


@NgModule({
    declarations: [LoanComponent],
  imports: [
      CommonModule,
      LoanRoutingModule
  ],
  providers: [
    LoanService,
  ]
})
export class LoanModule { }
