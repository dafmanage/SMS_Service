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
    console.log('DEBUG: forceLogout() called - clearing session storage');
    console.log('DEBUG: Call stack:', new Error().stack);
    console.log('DEBUG: SessionStorage before clear:', Object.keys(sessionStorage));
    
    // Clear all session storage
    sessionStorage.clear();
    
    console.log('DEBUG: SessionStorage after clear:', Object.keys(sessionStorage));
    
    // Clear any cached data
    if ('caches' in window) {
      caches.keys().then(names => {
        names.forEach(name => {
          caches.delete(name);
        });
      });
    }
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
    // Clear session storage immediately
    sessionStorage.clear();
    // Clear any cached data
    if ('caches' in window) {
      caches.keys().then(names => {
        names.forEach(name => {
          caches.delete(name);
        });
      });
    }
    return this.http.post<ResponseMessage>(this.BaseURI + '/Authentication/Logout', {});
  }

  // getUserProfile() {
  //   return this.http.get(this.BaseURI + '/UserProfile');
  // }

  roleMatch(allowedRoles: any): boolean {
    console.log('DEBUG: roleMatch called with allowedRoles:', allowedRoles);
    var isMatch = false;
    var token = sessionStorage.getItem('token');

    if (!token) {
      console.log('DEBUG: No token found for role matching');
      return false;
    }
    
    console.log('DEBUG: Token found for role matching, length:', token.length);

    try {
      // Validate JWT token format (should have 3 parts separated by dots)
      const tokenParts = token.split('.');
      if (tokenParts.length !== 3) {
        console.error('DEBUG: Invalid JWT token format - expected 3 parts, got:', tokenParts.length);
        return false;
      }

      // Decode the payload (middle part)
      const payload = tokenParts[1];
      
      // Add padding if needed for base64 decoding
      const paddedPayload = payload + '='.repeat((4 - payload.length % 4) % 4);
      
      // Try to decode the payload
      let decodedPayload;
      try {
        // First try the standard atob
        decodedPayload = window.atob(paddedPayload);
      } catch (atobError) {
        console.error('DEBUG: atob error in roleMatch:', atobError);
        try {
          // Try alternative decoding method
          decodedPayload = atob(paddedPayload);
        } catch (altError) {
          console.error('DEBUG: Alternative decode also failed in roleMatch:', altError);
          // Try URL-safe base64 decoding
          const urlSafePayload = payload.replace(/-/g, '+').replace(/_/g, '/');
          const urlSafePadded = urlSafePayload + '='.repeat((4 - urlSafePayload.length % 4) % 4);
          decodedPayload = window.atob(urlSafePadded);
        }
      }
      
      var payLoad = JSON.parse(decodedPayload);
      console.log('DEBUG: Decoded payload:', payLoad);
      
      // Check for role claim (ASP.NET Core uses ClaimTypes.Role)
      const roleClaim = payLoad['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payLoad.role;
      
      if (!payLoad || !roleClaim) {
        console.log('DEBUG: No role found in payload');
        console.log('DEBUG: Available claims:', Object.keys(payLoad));
        return false;
      }

      var userRole: string[] = Array.isArray(roleClaim) ? roleClaim : roleClaim.split(',');
      console.log('DEBUG: User roles:', userRole);
      console.log('DEBUG: Allowed roles:', allowedRoles);
      console.log('DEBUG: Role claim type:', typeof roleClaim);
      console.log('DEBUG: Role claim value:', roleClaim);
      
      for (const element of allowedRoles) {
        console.log(`DEBUG: Checking if user role includes: ${element}`);
        if (userRole.includes(element)) {
          isMatch = true;
          console.log(`DEBUG: Match found for role: ${element}`);
          break;
        }
      }
      
      console.log('DEBUG: Final role match result:', isMatch);
      return isMatch;
    } catch (error) {
      console.error('DEBUG: Error decoding token:', error);
      console.error('DEBUG: Token that failed:', token);
      
      // Clear invalid token
      sessionStorage.removeItem('token');
      console.log('DEBUG: Cleared invalid token from sessionStorage');
      
      return false;
    }
  }

  getRoles() {
    return this.http.get<SelectList[]>(this.BaseURI + '/Authentication/getroles');
  }

  getCurrentUser() {
    console.log('DEBUG: getCurrentUser called');
    console.log('DEBUG: SessionStorage keys:', Object.keys(sessionStorage));
    console.log('DEBUG: SessionStorage token key exists:', sessionStorage.getItem('token') !== null);
    
    var token = sessionStorage.getItem('token');

    if (!token) {
      console.log('DEBUG: No token found for getCurrentUser');
      console.log('DEBUG: SessionStorage content:', sessionStorage);
      return null;
    }

    console.log('DEBUG: Token length:', token.length);
    console.log('DEBUG: Token preview:', token.substring(0, 50) + '...');

    try {
      // Validate JWT token format (should have 3 parts separated by dots)
      const tokenParts = token.split('.');
      if (tokenParts.length !== 3) {
        console.error('DEBUG: Invalid JWT token format - expected 3 parts, got:', tokenParts.length);
        return null;
      }

      // Decode the payload (middle part)
      const payload = tokenParts[1];
      console.log('DEBUG: Payload part:', payload);
      
      // Add padding if needed for base64 decoding
      const paddedPayload = payload + '='.repeat((4 - payload.length % 4) % 4);
      console.log('DEBUG: Padded payload:', paddedPayload);
      
      // Try to decode the payload
      let decodedPayload;
      try {
        // First try the standard atob
        decodedPayload = window.atob(paddedPayload);
        console.log('DEBUG: Decoded payload string:', decodedPayload);
      } catch (atobError) {
        console.error('DEBUG: atob error:', atobError);
        try {
          // Try alternative decoding method
          decodedPayload = atob(paddedPayload);
          console.log('DEBUG: Alternative decode successful');
        } catch (altError) {
          console.error('DEBUG: Alternative decode also failed:', altError);
          // Try URL-safe base64 decoding
          const urlSafePayload = payload.replace(/-/g, '+').replace(/_/g, '/');
          const urlSafePadded = urlSafePayload + '='.repeat((4 - urlSafePayload.length % 4) % 4);
          decodedPayload = window.atob(urlSafePadded);
          console.log('DEBUG: URL-safe decode successful');
        }
      }
      
      var payLoad = JSON.parse(decodedPayload);
      console.log('DEBUG: Decoded payload for getCurrentUser:', payLoad);
      
      if (!payLoad) {
        console.log('DEBUG: No payload found in token');
        return null;
      }

      // Check for role claim (ASP.NET Core uses ClaimTypes.Role)
      const roleClaim = payLoad['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payLoad.role;
      const roles = roleClaim ? (Array.isArray(roleClaim) ? roleClaim : roleClaim.split(',')) : [];

      let user: UserView = {
        userId: payLoad.userId,
        fullName: payLoad.name,
        role: roles,
        organizationId: payLoad.organizationId,
        photo: payLoad.photo
      };

      console.log('DEBUG: getCurrentUser returning:', user);
      return user;
    } catch (error) {
      console.error('DEBUG: Error decoding token in getCurrentUser:', error);
      console.error('DEBUG: Token that failed:', token);
      
      // Clear invalid token
      sessionStorage.removeItem('token');
      console.log('DEBUG: Cleared invalid token from sessionStorage');
      
      return null;
    }
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
    const token = this.getToken();
    console.log('DEBUG: Token exists:', !!token);
    console.log('DEBUG: Token value:', token ? token.substring(0, 20) + '...' : 'null');
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
