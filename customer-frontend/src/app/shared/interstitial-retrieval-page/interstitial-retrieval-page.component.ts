import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-retrieve-existing',
  templateUrl: './interstitial-retrieval-page.component.html',
  styleUrls: ['./interstitial-retrieval-page.component.scss']
})
export class InterstitialRetrievalPageComponent implements OnInit {

  
  @Input() moduleName: string;
  @Input() retrievalDate: Date;
  @Output() retrieveDataEvent = new EventEmitter<boolean>();
  constructor() { }

  ngOnInit(): void {
    
  }

  retainOrRefresh(doRetrieve: boolean) {
    this.retrieveDataEvent.emit(doRetrieve);
  }
}
