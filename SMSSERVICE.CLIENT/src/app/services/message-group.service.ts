import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { ResponseMessage } from 'src/models/ResponseMessage.Model';
import { IMessagGroupGetDto, IMessagGroupPostDto, IMessagGroupPutDto } from 'src/models/msg/msg.model';

@Injectable({
  providedIn: 'root'
})
export class MessageGroupService {

  constructor(private http: HttpClient) { }
  readonly baseUrl = environment.baseUrl;

  getMessageGroups(id:string) {
    return this.http.get<IMessagGroupGetDto[]>(this.baseUrl + "/MessageGroup?OrganizationId=" + id)
  }
  addMessageGroup(value: IMessagGroupPostDto) {
    return this.http.post<ResponseMessage>(this.baseUrl + '/MessageGroup', value);
  }
  updateMessageGroup(value : IMessagGroupPutDto){
    return this.http.put<ResponseMessage>(this.baseUrl + '/MessageGroup', value);
  }
}
