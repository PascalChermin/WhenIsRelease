import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Region, Platform } from '../models';
import { Observable } from 'rxjs';

@Injectable()
export class CalService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  getCalendarLink(selectedRegions: Region[], selectedPlatforms: Platform[]): string {
    var filter = '';

    if (selectedRegions.length > 0) {
      filter += '?regions=' + selectedRegions.map(r => r.id).join('&regions=');
    }

    if (selectedPlatforms.length > 0) {
      filter += filter.indexOf('?') > -1 ? '&' : '?';
      filter += 'platforms=' + selectedPlatforms.map(p => p.id).join('&platforms=');
    }

    return this.baseUrl + 'api/GameService/ical' + filter;
  }

  getCalendar(selectedRegions: Region[], selectedPlatforms: Platform[]): Observable<Response> {
    var url = this.getCalendarLink(selectedRegions, selectedPlatforms);

    const reqOptions: Object = {
      headers: {
        'Accept': 'text/calendar'
      },
      responseType: 'text/calendar'
    };

    return this.http.get<Response>(url, reqOptions);
  }
}
