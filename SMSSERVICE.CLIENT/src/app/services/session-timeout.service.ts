import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from './user.service';
import { MessageService } from 'primeng/api';

@Injectable({
  providedIn: 'root'
})
export class SessionTimeoutService {
  private readonly SESSION_TIMEOUT_MINUTES = 15;
  private readonly WARNING_MINUTES = 3;
  private sessionTimer: any;
  private warningTimer: any;
  private lastActivity: Date = new Date();

  constructor(
    private router: Router,
    private userService: UserService,
    private messageService: MessageService
  ) {
    // Don't initialize session tracking immediately
    // Session tracking will be started manually after login
    console.log('DEBUG: SessionTimeoutService initialized, waiting for manual start');
  }

  private checkForValidToken(): void {
    // Check if there's a valid token in sessionStorage
    const token = sessionStorage.getItem('token');
    if (token && token.trim() !== '') {
      console.log('DEBUG: Valid token found, initializing session tracking');
      this.initializeSessionTracking();
    } else {
      console.log('DEBUG: No valid token found, session tracking not started');
      // Check again after a short delay
      setTimeout(() => this.checkForValidToken(), 1000);
    }
  }

  private initializeSessionTracking(): void {
    // Reset last activity to current time when starting session tracking
    this.lastActivity = new Date();
    console.log('DEBUG: Session tracking initialized, lastActivity reset to:', this.lastActivity);
    
    // Track user activity
    document.addEventListener('click', () => this.updateActivity());
    document.addEventListener('keypress', () => this.updateActivity());
    document.addEventListener('mousemove', () => this.updateActivity());
    document.addEventListener('scroll', () => this.updateActivity());

    // Start session timer
    this.startSessionTimer();
  }

  private updateActivity(): void {
    this.lastActivity = new Date();
    this.resetTimers();
  }

  private startSessionTimer(): void {
    // Clear existing timers
    this.clearTimers();

    // Set warning timer (3 minutes before timeout)
    const warningTime = (this.SESSION_TIMEOUT_MINUTES - this.WARNING_MINUTES) * 60 * 1000;
    console.log('DEBUG: Setting warning timer for', warningTime / 1000, 'seconds');
    this.warningTimer = setTimeout(() => {
      this.showWarning();
    }, warningTime);

    // Set session timeout timer
    const timeoutTime = this.SESSION_TIMEOUT_MINUTES * 60 * 1000;
    console.log('DEBUG: Setting session timeout timer for', timeoutTime / 1000, 'seconds');
    this.sessionTimer = setTimeout(() => {
      this.handleSessionTimeout();
    }, timeoutTime);
  }

  private resetTimers(): void {
    this.startSessionTimer();
  }

  private showWarning(): void {
    this.messageService.add({
      severity: 'warn',
      summary: 'Session Warning',
      detail: `Your session will expire in ${this.WARNING_MINUTES} minutes due to inactivity. Please perform an action to continue.`,
      life: 10000
    });
  }

  private handleSessionTimeout(): void {
    console.log('DEBUG: Session timeout triggered!');
    console.log('DEBUG: Current time:', new Date());
    console.log('DEBUG: Last activity:', this.lastActivity);
    
    this.messageService.add({
      severity: 'error',
      summary: 'Session Expired',
      detail: 'Your session has expired due to inactivity. Please login again.',
      life: 5000
    });

    // Clear session and redirect to login
    this.userService.forceLogout();
    this.router.navigate(['/auth/login']);
  }

  private clearTimers(): void {
    if (this.sessionTimer) {
      clearTimeout(this.sessionTimer);
      this.sessionTimer = null;
    }
    if (this.warningTimer) {
      clearTimeout(this.warningTimer);
      this.warningTimer = null;
    }
  }

  // Public method to manually extend session (called on API calls)
  public extendSession(): void {
    this.updateActivity();
  }

  // Public method to start session tracking (called after successful login)
  public startSessionTracking(): void {
    const token = sessionStorage.getItem('token');
    if (token && token.trim() !== '') {
      console.log('DEBUG: Starting session tracking after login - token found');
      this.initializeSessionTracking();
    } else {
      console.log('DEBUG: Cannot start session tracking - no valid token found');
    }
  }

  // Public method to get remaining session time
  public getRemainingTime(): number {
    const now = new Date();
    const timeDiff = now.getTime() - this.lastActivity.getTime();
    const remainingMs = (this.SESSION_TIMEOUT_MINUTES * 60 * 1000) - timeDiff;
    return Math.max(0, remainingMs);
  }

  // Public method to stop session tracking (called on logout)
  public stopSessionTracking(): void {
    console.log('DEBUG: Stopping session tracking');
    this.clearTimers();
  }

  // Cleanup method
  public destroy(): void {
    this.clearTimers();
  }
}
