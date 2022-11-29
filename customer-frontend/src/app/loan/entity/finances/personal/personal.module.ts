import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PersonalRoutingModule } from '../personal/personal-routing.module';
import { PersonalComponent } from './personal.component';
import { ComponentHeaderModule } from '../../../../layout/component-header/component-header.module';
import { ComingSoonModule } from '../../../../shared/coming-soon/coming-soon.module';

import { NgxMaskModule } from 'ngx-mask';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';

import { NgSelectModule } from '@ng-select/ng-select';

import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { InterstitialRetrievalPageModule } from '../../../../shared/interstitial-retrieval-page/interstitial-retrieval-page.module';
import { FinancesSummaryModalComponent } from './finances-summary-modal/finances-summary-modal.component';
import { SmartyStreetsService } from '../../company-info/smartyStreets.service';
import { ToastrService } from 'ngx-toastr';
import { AccordionModule } from 'ngx-bootstrap/accordion';
import { NLevelCategoryModalComponent } from './nlevelCategory-modal/nlevelCategory-modal.component';

@NgModule({
  declarations: [PersonalComponent,
    NLevelCategoryModalComponent,
    FinancesSummaryModalComponent],
  imports: [
    CommonModule,
    FormsModule,
    InterstitialRetrievalPageModule,
    ReactiveFormsModule,
    ComingSoonModule,
    PersonalRoutingModule,
    ComponentHeaderModule,
    TypeaheadModule.forRoot(),
    NgSelectModule,
    BsDatepickerModule.forRoot(),
    NgxMaskModule.forRoot(),
    AccordionModule.forRoot()
  ],
  exports: [PersonalComponent, FinancesSummaryModalComponent],
  providers: [SmartyStreetsService, ToastrService]
})
export class PersonalModule { }
