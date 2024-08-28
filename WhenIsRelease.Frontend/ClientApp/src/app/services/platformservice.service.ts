import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Platform } from '../models';

@Injectable()
export class PlatformService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  getAllPlatforms(): Observable<Platform[]> {
    return this.http.get<Platform[]>(this.baseUrl + 'api/GameService/allplatforms');
  }

  getPlatforms(): Observable<Platform[]> {
    return this.http.get<Platform[]>(this.baseUrl + 'api/GameService/platforms');
  }
}
