import { Injectable } from '@angular/core';

import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { FormGroup } from '@angular/forms';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { environment } from 'src/environments/environment';
import { User, UserView, ChangePasswordModel, UserList, UserPost } from 'src/models/auth/userDto';
import { ResponseMessage, SelectList } from 'src/models/ResponseMessage.Model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  readonly BaseURI = environment.baseUrl;
  readonly HubUri = environment.assetUrl;

  private hubConnection: signalR.HubConnection;
  private forceLogoutSubject = new Subject<void>();

  forceLogout$ = this.forceLogoutSubject.asObservable();

  constructor(private http: HttpClient) {}

  // In user.service.ts
initializeSignalRConnection(token: string): Promise<void> {
  this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(this.HubUri + '/notificationHub', { accessTokenFactory: () => token })
    .build();

  // Start the connection and return a promise
  return this.hubConnection.start()
    .then(() => {
      console.log('SignalR Connection Established');
    })
    .catch(err => {
      console.error('SignalR Connection Error: ', err);
      throw err; // Re-throw error to be handled in caller
    });
}


  forceLogout() {
    sessionStorage.removeItem('token');
    this.stopSignalRConnection();
    this.forceLogoutSubject.next();
  }

  stopSignalRConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }



  comparePasswords(fb: FormGroup) {
    let confirmPswrdCtrl = fb.get('ConfirmPassword');
    if (confirmPswrdCtrl!.errors == null || 'passwordMismatch' in confirmPswrdCtrl!.errors) {
      if (fb.get('Password')!.value != confirmPswrdCtrl!.value) confirmPswrdCtrl!.setErrors({ passwordMismatch: true });
      else confirmPswrdCtrl!.setErrors(null);
    }
  }

  register(body: User) {
    return this.http.post(this.BaseURI + '/Authentication/Register', body);
  }

  login(formData: User) {
    return this.http.post<ResponseMessage>(this.BaseURI + '/Authentication/Login', formData);
  }
  public getToken(): string | null {
    return sessionStorage.getItem('token');
  }

  logout() {
    this.stopSignalRConnection();
    return this.http.post<ResponseMessage>(this.BaseURI + '/Authentication/Logout', {});
  }

  // getUserProfile() {
  //   return this.http.get(this.BaseURI + '/UserProfile');
  // }

  roleMatch(allowedRoles: any): boolean {
    var isMatch = false;
    var token = sessionStorage.getItem('token');

    //var payLoad = token ? JSON.parse(window.atob(token!.split('.')[1])) : "";
    var payLoad = {
      userId: '7cd878f5-8d25-494d-899c-a9d46ebf12c9',
      organizationId: '5e3167c2-a5ba-42b9-886c-1289d225f054',
      name: 'DAFTech Social ICT Solution ዳፍቴክ ሶሻል',
      photo: 'wwwroot\\Employee\\02956bca-dc74-4a9c-9591-95d8d86accc5.png',
      role: 'Admin',
      nbf: 1699022430,
      exp: 1699026030,
      iat: 1699022430
    };

    var userRole: string[] = payLoad ? payLoad.role.split(',') : [];
    allowedRoles.forEach((element: any) => {
      if (userRole.includes(element)) {
        isMatch = true;
        return false;
      } else {
        return true;
      }
    });
    return isMatch;
  }

  getRoles() {
    return this.http.get<SelectList[]>(this.BaseURI + '/Authentication/getroles');
  }

  getCurrentUser() {
    var token = sessionStorage.getItem('token');

    var payLoad = this.decodeJWT(token);

    if (payLoad) {
      var userValue = payLoad.payload;
      let user: UserView = {
        userId: userValue.userId,
        fullName: userValue.name,
        role: userValue.role.split(','),
        organizationId: userValue.organizationId,
        photo: userValue.photo
      };

      console.log('user', user);

      return user;
    }

    return null;
  }

  decodeJWT(token) {
    // Split the token into its three parts
    const [headerB64, payloadB64, signature] = token.split('.');

    // Decode the header and payload
    function decodeBase64Url(str) {
      str = str.replace(/-/g, '+').replace(/_/g, '/');
      return decodeURIComponent(
        atob(str)
          .split('')
          .map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
          })
          .join('')
      );
    }

    const header = JSON.parse(decodeBase64Url(headerB64));
    const payload = JSON.parse(decodeBase64Url(payloadB64));

    return { header, payload, signature };
  }

  changePassword(formData: ChangePasswordModel) {
    return this.http.post<ResponseMessage>(this.BaseURI + '/Authentication/ChangePassword', formData);
  }

  getUserList() {
    return this.http.get<UserList[]>(this.BaseURI + '/Authentication/GetUserList');
  }

  createUser(body: UserPost) {
    return this.http.post<ResponseMessage>(this.BaseURI + '/Authentication/AddUser', body);
  }

  getRoleCategory() {
    return this.http.get<SelectList[]>(this.BaseURI + '/Authentication/GetRoleCategory');
  }

  getNotAssignedRole(userId: string) {
    return this.http.get<SelectList[]>(this.BaseURI + `/Authentication/GetNotAssignedRole?userId=${userId}`);
  }
  getAssignedRole(userId: string) {
    return this.http.get<SelectList[]>(this.BaseURI + `/Authentication/GetAssignedRoles?userId=${userId}`);
  }
  assignRole(body: any) {
    return this.http.post<ResponseMessage>(this.BaseURI + '/Authentication/AssingRole', body);
  }
  revokeRole(body: any) {
    return this.http.post<ResponseMessage>(this.BaseURI + '/Authentication/RevokeRole', body);
  }
  // getSystemUsers() {
  //   return this.http.get<Employee[]>(this.BaseURI + "/Authentication/users")
  // }

}
