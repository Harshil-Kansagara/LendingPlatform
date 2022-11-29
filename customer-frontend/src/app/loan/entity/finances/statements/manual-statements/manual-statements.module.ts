import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ManualStatementsComponent } from './manual-statements.component'
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CollapseModule } from 'ngx-bootstrap/collapse';
import { UploadFileModule } from '../../taxes/upload-file/upload-file.module';
import { NgxMaskModule } from 'ngx-mask'
@NgModule({
  declarations: [ManualStatementsComponent],
  imports: [
    CommonModule,
    FormsModule, ReactiveFormsModule,
    CollapseModule,
    UploadFileModule,
    NgxMaskModule.forRoot()
  ],
  exports:[ManualStatementsComponent]
})
export class ManualStatementsModule { }
