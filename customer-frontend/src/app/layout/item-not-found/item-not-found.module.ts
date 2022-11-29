import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ItemNotFoundComponent } from './item-not-found.component';
@NgModule({
    declarations: [ItemNotFoundComponent],
    imports: [
        CommonModule
    ],
    exports:[ItemNotFoundComponent]
})
export class ItemNotFoundModule { }
