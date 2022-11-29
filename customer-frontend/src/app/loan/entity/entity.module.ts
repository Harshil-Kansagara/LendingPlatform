import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityComponent } from './entity.component';
import { EntityRoutingModule } from './entity-routing.module';


@NgModule({
  declarations: [EntityComponent],
  imports: [
    CommonModule,
    EntityRoutingModule
  ]
})
export class EntityModule { }
