import { Component, OnInit } from '@angular/core';
import { Constant } from '../../shared/constant';

@Component({
  selector: 'app-banner-sidebar',
  templateUrl: './banner-sidebar.component.html',
  styleUrls: ['./banner-sidebar.component.scss']
})
export class BannerSidebarComponent implements OnInit {
  paymentCaption = Constant.paymentCaption;
  constructor() { }

  ngOnInit(): void {
  }

}
