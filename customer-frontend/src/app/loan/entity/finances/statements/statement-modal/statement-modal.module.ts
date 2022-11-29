import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CollapseModule } from 'ngx-bootstrap/collapse';
import { UploadFileModule } from '../../taxes/upload-file/upload-file.module';
import { NgxMaskModule } from 'ngx-mask'
import { StatementModalComponent } from './statement-modal.component';
import { AccordionModule } from 'ngx-bootstrap/accordion';
@NgModule({
  declarations: [StatementModalComponent],
  imports: [
    CommonModule,
    FormsModule, ReactiveFormsModule,
    CollapseModule,
    UploadFileModule,
    AccordionModule,
    NgxMaskModule.forRoot()
  ],
  exports: [StatementModalComponent]
})
export class StatementModalModule { }
