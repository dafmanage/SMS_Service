// import { HttpClient } from '@angular/common/http';
// import { Injectable } from '@angular/core';
// import { environment } from 'src/environments/environment';
// import { ResponseMessage } from 'src/models/ResponseMessage.Model';
// import { IEmployeeGetDto } from 'src/models/hrm/IEmployeeDto';

// @Injectable({
//   providedIn: 'root'
// })
// export class HrmService {

//   constructor(private http: HttpClient) { }
//   readonly baseUrl = environment.baseUrl;

//   getEmployees() {
//     return this.http.get<IEmployeeGetDto[]>(this.baseUrl + "/Employee")
//   }
//   addEmployee(fromData: FormData) {
//     return this.http.post<ResponseMessage>(this.baseUrl + '/Employee', fromData);
//   }

//   updateEmployee(fromData : FormData){
//     return this.http.put<ResponseMessage>(this.baseUrl + '/Employee', fromData);
//   }

//   getEmployeesNoUserSelectList(){

//     return this.http.get<IEmployeeGetDto[]>(this.baseUrl + "/Employee/getEmployeeNoUser")
//   }

// }
