import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ComponentHeaderComponent } from './component-header.component';
import { ProgressbarModule } from 'ngx-bootstrap/progressbar';
@NgModule({
    declarations: [ComponentHeaderComponent],
    imports: [
        CommonModule,
        ProgressbarModule.forRoot(),
    ],
    exports:[ComponentHeaderComponent]
})
export class ComponentHeaderModule { }
