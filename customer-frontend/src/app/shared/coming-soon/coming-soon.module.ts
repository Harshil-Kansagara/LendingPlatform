import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ComingSoonComponent } from './coming-soon.component';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [ComingSoonComponent],
  imports: [
    CommonModule,
    RouterModule
  ],
  exports:[ComingSoonComponent]
})
export class ComingSoonModule { }
