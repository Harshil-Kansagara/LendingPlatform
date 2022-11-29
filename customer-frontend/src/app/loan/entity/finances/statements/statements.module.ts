import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { StatementsComponent } from './statements.component';
import { StatementsRoutingModule } from './statements-routing.module';
import { ComponentHeaderModule } from '../../../../layout/component-header/component-header.module';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { ManualStatementsModule } from './manual-statements/manual-statements.module';
import { ProgressbarModule } from 'ngx-bootstrap/progressbar';
import { ModalModule } from 'ngx-bootstrap/modal';
import { AccordionModule } from 'ngx-bootstrap/accordion';
import { StatementModalModule } from './statement-modal/statement-modal.module';
import { ComingSoonModule } from '../../../../shared/coming-soon/coming-soon.module';
import { NgxMaskModule } from 'ngx-mask';
import { InterstitialRetrievalPageModule } from '../../../../shared/interstitial-retrieval-page/interstitial-retrieval-page.module';
@NgModule({
    declarations: [StatementsComponent],
    imports: [
        FormsModule,
        ReactiveFormsModule,
        CommonModule,
        ComponentHeaderModule,
        StatementsRoutingModule,
        ManualStatementsModule,
      ComingSoonModule,
      InterstitialRetrievalPageModule,
        TabsModule.forRoot(),
        ProgressbarModule.forRoot(),
        ModalModule.forRoot(),
      AccordionModule.forRoot(),
      StatementModalModule,
      NgxMaskModule.forRoot(),

    ],
})
export class StatementsModule { }
