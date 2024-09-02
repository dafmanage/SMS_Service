import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ResponseMessage } from 'src/models/ResponseMessage.Model';
import { IMessagGetDto, IMessagPostDto } from 'src/models/s-msg/s-msg.model';

@Injectable({
  providedIn: 'root'
})
export class SendMessageService {

  constructor(private http: HttpClient) { }
  readonly baseUrl = environment.baseUrl;

  getMessage(id:string) {
    return this.http.get<IMessagGetDto[]>(this.baseUrl + "/Message/GetMessages?MessageGroupId=" + id)
  }
  addMessage(value: IMessagPostDto) {
    return this.http.post<ResponseMessage>(this.baseUrl + '/Message/AddMessages', value);
  }
  getGroupName(groupId: number): Observable<string> {
    return this.http.get<string>(`Message/AddMessages/${groupId}/name`);
  }
  // getMessageGroupNameById(messageGroupId: number): Observable<string> {
  //   return this.http.get(`/api/Message/GetMessages?MessageGroupId=${messageGroupId}`);
  // }
}
