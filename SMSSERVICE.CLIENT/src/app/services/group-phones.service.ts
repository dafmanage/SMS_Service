import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { ResponseMessage } from 'src/models/ResponseMessage.Model';
import { IPhoneGroupGetDto, IPhoneGroupPostDto, IPhoneGroupPutDto } from 'src/models/phn/phn.model';

@Injectable({
  providedIn: 'root'
})
export class GroupPhonesService {

  constructor(private http: HttpClient) { }
  readonly baseUrl = environment.baseUrl;

  getGroupPhones(id: string) {
    return this.http.get<IPhoneGroupGetDto[]>(this.baseUrl + "/GroupPhone/GetGroupPhones?MessageGroupId=" +  id)
  }
  addGroupPhone(value: IPhoneGroupPostDto) {
    return this.http.post<ResponseMessage>(this.baseUrl + '/GroupPhone/AddGroupPhone', value);
  }
  addGroupPhoneExcel(fromData: FormData,userId:string) {
    return this.http.post<ResponseMessage>(this.baseUrl + `/GroupPhone/AddGroupPhoneFromExcel?createdById=${userId}`, fromData);
  }
  updateGroupPhone(value : IPhoneGroupPutDto){
    return this.http.put<ResponseMessage>(this.baseUrl + '/GroupPhone/UpdateGroupPhone', value);
  }
  // getgroupSelectList(){
  //   return this.http.get<any>(this.baseUrl +"/api/Employee/getEmployeesSelectList")
  // }


}
