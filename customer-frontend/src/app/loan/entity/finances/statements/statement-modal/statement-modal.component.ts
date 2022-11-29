import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-statement-modal',
  templateUrl: './statement-modal.component.html'
})
export class StatementModalComponent implements OnInit {

  constructor() { }

  @Input() data;
  @Input() periodList;
  @Input() showDollarInThousand;

  ngOnInit() {
    
  }

  // Print zeros or empty in table
  printZerosOrEmpty(dataValue) {
    return dataValue.amount.some(x => x !== 0);
  }
}
