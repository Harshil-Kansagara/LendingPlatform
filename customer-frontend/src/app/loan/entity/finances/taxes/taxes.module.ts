import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaxesComponent } from './taxes.component';
import { TaxesRoutingModule } from './taxes-routing.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UploadFileModule } from './upload-file/upload-file.module';
import { NgxMaskModule } from 'ngx-mask';
import { NgxFileDropModule } from 'ngx-file-drop';
import { NgSelectModule } from '@ng-select/ng-select';
import { ComponentHeaderModule } from '../../../../layout/component-header/component-header.module';
import { InterstitialRetrievalPageModule } from '../../../../shared/interstitial-retrieval-page/interstitial-retrieval-page.module';
@NgModule({
    declarations: [TaxesComponent],
    imports: [
        FormsModule,
        ReactiveFormsModule,
        CommonModule,
        TaxesRoutingModule,
        UploadFileModule,
        NgxMaskModule.forRoot(),
        NgxFileDropModule,
      NgSelectModule,
      InterstitialRetrievalPageModule,
        ComponentHeaderModule
    ]
})
export class TaxesModule { }
