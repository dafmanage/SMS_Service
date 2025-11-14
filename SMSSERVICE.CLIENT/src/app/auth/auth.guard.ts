
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { UserService } from '../services/user.service';
import { BrowserSecurityService } from '../services/browser-security.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private router: Router, 
    private service: UserService,
    private browserSecurity: BrowserSecurityService
  ) {
  }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): boolean {
    
    console.log('DEBUG: AuthGuard checking route:', state.url);
    console.log('DEBUG: Page refresh detected:', performance.navigation.type === 1);
    console.log('DEBUG: SessionStorage keys:', Object.keys(sessionStorage));
    
    // Get token from session storage
    const token = sessionStorage.getItem('token');
    console.log('DEBUG: Token exists:', !!token);
    console.log('DEBUG: Token value:', token ? 'Present' : 'Missing');
    console.log('DEBUG: Token length:', token ? token.length : 'N/A');
    
    // Simple check - just verify token exists
    if (token && token.trim() !== '') {
      console.log('DEBUG: Token found, allowing access');
      return true;
    } else {
      console.log('DEBUG: No token found, redirecting to login');
      console.log('DEBUG: Current URL:', window.location.href);
      this.router.navigate(['/auth/login']).then(success => {
        console.log('DEBUG: Redirect to login result:', success);
        console.log('DEBUG: New URL after redirect:', window.location.href);
      });
      return false;
    }
  }

  private isValidTokenFormat(token: string): boolean {
    // Basic JWT format validation (3 parts separated by dots)
    const parts = token.split('.');
    return parts.length === 3;
  }

  private isTokenExpired(token: string): boolean {
    try {
      // Decode JWT token to check expiration
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp < currentTime;
    } catch (error) {
      console.log('DEBUG: Error decoding token:', error);
      return true; // If we can't decode, consider it expired
    }
  }

  private clearSessionAndRedirect(): void {
    // Use the browser security service for secure redirect
    // this.browserSecurity.secureRedirectToLogin();
    // Temporary simple redirect for debugging
    sessionStorage.clear();
    this.router.navigate(['/auth/login'], { replaceUrl: true });
  }

  logout() {
    // Use the browser security service for secure logout
    this.browserSecurity.secureRedirectToLogin();
  }
}
