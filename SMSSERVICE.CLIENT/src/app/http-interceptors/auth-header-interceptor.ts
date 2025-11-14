import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { SpinnerService } from '../components/spinner/spinner.service';
import { SessionTimeoutService } from '../services/session-timeout.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private spinnerService: SpinnerService,
    private sessionTimeoutService: SessionTimeoutService
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = sessionStorage.getItem('token');
    
    console.log('DEBUG: Interceptor called for URL:', request.url);
    console.log('DEBUG: Token exists:', !!token);
    console.log('DEBUG: Token value:', token ? token.substring(0, 20) + '...' : 'null');

    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      console.log('DEBUG: Authorization header added to request');
      
      // Extend session on each API call
      this.sessionTimeoutService.extendSession();
    } else {
      console.log('DEBUG: No token found, request sent without Authorization header');
    }

    this.spinnerService.requestStarted();

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        console.log('DEBUG: HTTP Error received:', error.status, error.url);
        console.log('DEBUG: Error details:', error);
        console.log('DEBUG: Error message:', error.message);
        console.log('DEBUG: Error error:', error.error);
        
        if (error.status === 0) {
          console.log('DEBUG: Network error - backend may not be running');
          // Don't clear token for network errors
        } else if (error.status === 401) {
          console.log('DEBUG: 401 Unauthorized - token may be invalid or expired');
          sessionStorage.removeItem('token');
          this.router.navigate(['/auth/login']);
        } else if (error.status === 403) {
          console.log('DEBUG: 403 Forbidden - insufficient permissions');
          // Don't remove token for 403 errors - user is authenticated but lacks permission
          // Only redirect if it's a critical operation
          if (error.url?.includes('/SystemConfiguration')) {
            console.log('DEBUG: 403 on SystemConfiguration - user lacks SuperAdmin role');
            // Don't logout, just show error message
          } else {
            sessionStorage.removeItem('token');
            this.router.navigateByUrl('/auth/login');
          }
        }
        return throwError(() => error);
      }),
      finalize(() => {
        this.spinnerService.requestEnded();
      })
    );
  }
}