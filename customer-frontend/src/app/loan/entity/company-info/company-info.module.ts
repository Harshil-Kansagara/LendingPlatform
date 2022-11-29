import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CompanyInfoComponent } from './company-info.component';
import { CompanyInfoRoutingModule } from './company-info-routing.module';
import { EntityService, GlobalService } from '../../../utils/serviceNew';
import { NgSelectModule } from '@ng-select/ng-select';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';
import { NgxMaskModule } from 'ngx-mask';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { ModalModule, BsModalService } from 'ngx-bootstrap/modal';
import { ComponentHeaderModule } from '../../../layout/component-header/component-header.module';
import { SmartyStreetsService } from './smartyStreets.service';
import { ToastrService } from 'ngx-toastr';
@NgModule({
  declarations: [CompanyInfoComponent],
    imports: [
        FormsModule,
        ReactiveFormsModule,
        CommonModule,
        ComponentHeaderModule,
        CompanyInfoRoutingModule,
        NgSelectModule,
        TypeaheadModule.forRoot(),
        NgxMaskModule.forRoot(),
        ModalModule.forRoot(),
        BsDatepickerModule.forRoot(),
    ],
    providers: [
      EntityService,
      GlobalService,
      DatePipe,
      SmartyStreetsService,
      ToastrService,
      BsModalService
    ]
})
export class CompanyInfoModule { }
