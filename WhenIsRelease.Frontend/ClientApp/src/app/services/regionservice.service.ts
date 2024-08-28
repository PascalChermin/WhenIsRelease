import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Region } from '../models';

@Injectable()
export class RegionService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  getRegions(): Observable<Region[]> {
    return this.http.get<Region[]>(this.baseUrl + 'api/GameService/regions');
  }
}
