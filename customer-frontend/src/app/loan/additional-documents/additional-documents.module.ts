import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdditionalDocumentsComponent } from './additional-documents.component';
import { AdditionalDocumentsRoutingModule } from './additional-documents-routing.module';
import { ComingSoonModule } from '../../shared/coming-soon/coming-soon.module';
import { ComponentHeaderModule } from '../../layout/component-header/component-header.module';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxMaskModule } from 'ngx-mask';
import { NgxFileDropModule } from 'ngx-file-drop';
import { NgSelectModule } from '@ng-select/ng-select';


@NgModule({
  declarations: [AdditionalDocumentsComponent],
  imports: [
    CommonModule,

    FormsModule,
    ReactiveFormsModule,
    AdditionalDocumentsRoutingModule,
    ComingSoonModule,
    ComponentHeaderModule,
    NgxMaskModule.forRoot(),
    NgxFileDropModule,
    NgSelectModule,
  ]
})
export class AdditionalDocumentsModule { }
