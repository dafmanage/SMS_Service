import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { IUnsentGetDto } from 'src/models/unsent/unsent.model';

@Injectable({
  providedIn: 'root'
})
export class UnsentService {

  constructor(private http: HttpClient) { }
  readonly baseUrl = environment.baseUrl;

  getUnsentMessages() {
    return this.http.get<IUnsentGetDto[]>(this.baseUrl + "/Message/GetUnsentMessages" )
  }
  getUnsentMessage(id:string) {
    return this.http.get<IUnsentGetDto[]>(this.baseUrl + "/Message/GetUnsentMessages?organizationId=" + id)
  }
}
