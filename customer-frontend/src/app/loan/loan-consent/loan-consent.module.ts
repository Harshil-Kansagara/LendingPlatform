import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { LoanConsentComponent, ResidencyStatusEnumPipe, SecureInputPipe } from './loan-consent.component';
import { LoanConsentRoutingModule } from './loan-consent-routing.module';
import { UploadFileModule } from '../entity/finances/taxes/upload-file/upload-file.module';
import { ManualStatementsModule } from '../entity/finances/statements/manual-statements/manual-statements.module';
import { ComponentHeaderModule } from '../../layout/component-header/component-header.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { NgxMaskModule } from 'ngx-mask';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';
import { EntityService } from '../../utils/service';
import { BankTransactionsModule } from '../entity/finances/bank-transactions/bank-transactions.module';
import { InvoicesModule } from '../entity/finances/invoices/invoices.module';
import { NgSelectModule } from '@ng-select/ng-select';
import { ProgressbarModule } from 'ngx-bootstrap/progressbar';
import { StatementModalModule } from '../entity/finances/statements/statement-modal/statement-modal.module';
import { AccordionConfig } from 'ngx-bootstrap/accordion';
import { PersonalModule } from '../entity/finances/personal/personal.module';

@NgModule({
  declarations: [LoanConsentComponent, ResidencyStatusEnumPipe, SecureInputPipe,],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ComponentHeaderModule,
    LoanConsentRoutingModule,
    UploadFileModule,
    ManualStatementsModule,
    StatementModalModule,
    ModalModule.forRoot(),
    TabsModule.forRoot(),
    NgxMaskModule.forRoot(),
    TypeaheadModule.forRoot(),
    BsDatepickerModule.forRoot(),
    BankTransactionsModule,
    InvoicesModule,
    NgSelectModule,
    ProgressbarModule.forRoot(),
    PersonalModule
   
  ],
  providers: [
    AccordionConfig, 
    EntityService,
    DatePipe,
  ],
 
})
export class LoanConsentModule { }
