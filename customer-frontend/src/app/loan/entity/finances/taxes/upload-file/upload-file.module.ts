import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UploadFileComponent } from './upload-file.component';


@NgModule({
  declarations: [UploadFileComponent],
  imports: [
    CommonModule
  ],
  exports:[UploadFileComponent]
})
export class UploadFileModule { }
