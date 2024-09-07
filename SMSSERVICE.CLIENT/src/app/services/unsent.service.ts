import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { IUnsentGetDto } from 'src/models/unsent/unsent.model';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class UnsentService {

  constructor(private http: HttpClient,private userService: UserService) { }
  readonly baseUrl = environment.baseUrl;

  headers = new HttpHeaders({
    'Authorization': `Bearer ${this.userService.getToken()}`,
    'Content-Type': 'application/json'
  });

  getUnsentMessages() {
    return this.http.get<IUnsentGetDto[]>(this.baseUrl + "/Message/GetUnsentMessages" );
  }
  getUnsentMessage(id:string) {
    return this.http.get<IUnsentGetDto[]>(this.baseUrl + "/Message/GetUnsentMessages?organizationId=" + id)
  }
}
