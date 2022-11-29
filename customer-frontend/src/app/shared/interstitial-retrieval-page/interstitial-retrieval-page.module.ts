import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { InterstitialRetrievalPageComponent } from './interstitial-retrieval-page.component';

@NgModule({
  declarations: [InterstitialRetrievalPageComponent],
  imports: [
    CommonModule,
    RouterModule
  ],
  exports: [InterstitialRetrievalPageComponent]
})
export class InterstitialRetrievalPageModule { }
