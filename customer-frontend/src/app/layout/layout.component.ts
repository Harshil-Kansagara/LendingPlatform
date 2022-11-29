import { Component, OnInit, ViewChildren, QueryList, ElementRef } from '@angular/core';
import { AppService } from '../services/app.service';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html'
})
export class LayoutComponent implements OnInit {

  constructor(private readonly appService:AppService) { 
    this.appService.hideFooter.subscribe(val => this.hideFooter = val);
  }
  hideFooter = false;
  ngOnInit(): void {
  }
  @ViewChildren('viewOnly') viewMode: QueryList<ElementRef>;
  
  ngAfterViewInit() {
    this.appService.currentMode.subscribe(val => {
      if (val === true) {
        this.viewMode.forEach(el => el.nativeElement.classList.add('view-only'));
      } else {
        this.viewMode.forEach(el => el.nativeElement.classList.remove('view-only'));
      }
    });
  }
}
