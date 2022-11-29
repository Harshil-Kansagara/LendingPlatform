import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BankTransactionsRoutingModule } from './bank-transactions-routing.module';
import { BankTransactionsComponent } from './bank-transactions.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgxMaskModule } from 'ngx-mask';
import { ModalModule } from 'ngx-bootstrap/modal';
@NgModule({
  declarations: [BankTransactionsComponent],
  imports: [
    CommonModule,
    FormsModule,
    BankTransactionsRoutingModule,
    NgSelectModule,
    NgxMaskModule.forRoot(),
    ModalModule.forRoot()
  ],
  exports:[BankTransactionsComponent]
})
export class BankTransactionsModule { }
