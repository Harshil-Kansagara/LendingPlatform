import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Observable, from } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class SmartyStreetsService {

  fetchSuggestions(address: string): Observable<any> {

    const smartyStreetsSDK = require('smartystreets-javascript-sdk');
    const smartyStreetsCore = smartyStreetsSDK.core;
    const Lookup = smartyStreetsSDK.usAutocomplete.Lookup;

    const websiteKey = environment.smartyStreetsAutoCompleteKey;
    const credentials = new smartyStreetsCore.SharedCredentials(websiteKey);

    const client = smartyStreetsCore.buildClient.usAutocomplete(credentials);

    const lookup = new Lookup(address);

    const resp = client.send(lookup);
    return from(resp);
  }
}
