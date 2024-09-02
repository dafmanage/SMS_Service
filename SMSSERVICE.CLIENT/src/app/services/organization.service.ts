import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { ResponseMessage } from 'src/models/ResponseMessage.Model';
import { IOrganizationGetDto } from 'src/models/hrm/IEmployeeDto';

@Injectable({
  providedIn: 'root'
})
export class OrganizationService {

  constructor(private http: HttpClient) { }
  readonly baseUrl = environment.baseUrl;

  getOrganizations() {
    return this.http.get<IOrganizationGetDto[]>(this.baseUrl + "/Organization")
  }
  addOrganiztion(fromData: FormData) {
    return this.http.post<ResponseMessage>(this.baseUrl + '/Organization', fromData);
  }
  updateOrganization(fromData : FormData){
    return this.http.put<ResponseMessage>(this.baseUrl + '/Organization', fromData);
  }

  getOrganizationsNoUserSelectList(){

    return this.http.get<IOrganizationGetDto[]>(this.baseUrl + "/Organization/getOrganizationNoUser")
  }
  getOrganizationsSelectList(){

    return this.http.get<IOrganizationGetDto[]>(this.baseUrl + "/Organization/getorganizationsSelectList")
  }



}
