import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Region, Platform, Release } from '../models';

@Injectable()
export class SearchService {

  releases: Release[];

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  search(searchQuery: string, selectedRegions: Region[], selectedPlatforms: Platform[]): Observable<Release[]> {
    var filter = '';

    if (selectedRegions.length > 0) {
      filter += '?regions=' + selectedRegions.map(r => r.id).join('&regions=');
    }

    if (selectedPlatforms.length > 0) {
      filter += filter.indexOf('?') > -1 ? '&' : '?';
      filter += 'platforms=' + selectedPlatforms.map(p => p.id).join('&platforms=');
    }

    return this.http.get<Release[]>(this.baseUrl + 'api/GameService/search/' + searchQuery + filter);
  }
}
