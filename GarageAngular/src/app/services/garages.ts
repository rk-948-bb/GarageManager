
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class GaragesService {
  private serverApiUrl = 'https://localhost:7034/api/Garages';

  constructor(private http: HttpClient) {}


  getGaragesFromGov(): Observable<any> {
    return this.http.get<any>(this.serverApiUrl + '/gov');
  }

  getGaragesFromDb(): Observable<any> {
    return this.http.get<any>(this.serverApiUrl);
  }

  addGaragesToDb(garages: any[]): Observable<any> {
    return this.http.post<any>(this.serverApiUrl + '/bulk', garages);
  }

  addGarageToDb(garage: any): Observable<any> {
    return this.http.post<any>(this.serverApiUrl, garage);
  }
}
