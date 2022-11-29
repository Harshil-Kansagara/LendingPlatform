import { Component, Input, OnInit } from '@angular/core';
import { Constant } from '../../shared/constant';

@Component({
  selector: 'app-item-not-found',
  templateUrl: './item-not-found.component.html',
  styleUrls: ['./item-not-found.component.scss']
})
export class ItemNotFoundComponent implements OnInit {
  @Input() title:string;
  constructor() { }
  supportTitle = Constant.supportTitle;
  supportMail = Constant.supportMail;
  supportPhone = Constant.supportPhone;
  ngOnInit(): void {
  }

}
