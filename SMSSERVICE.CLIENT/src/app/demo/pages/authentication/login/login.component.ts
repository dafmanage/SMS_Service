import { Component, OnInit, OnDestroy, NgZone } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { UserService } from 'src/app/services/user.service';
import { MessageService } from 'primeng/api';
import { UserView } from 'src/models/auth/userDto';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ButtonModule } from 'primeng/button';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, HttpClientModule, ButtonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  animations: [
    trigger('fadeInOut', [
      state(
        'void',
        style({
          opacity: 0
        })
      ),
      transition('void <=> *', animate(300))
    ])
  ]
})
export default class LoginComponent implements OnInit, OnDestroy {
  loginForm!: FormGroup;
  errorMessage: string = '';
  showForceLogoutPrompt: boolean = false;
  errorCode: number | null = null;
  remainingTime: number = 0;
  private countdownSubscription: Subscription | null = null;
  private forceLogoutSubscription: Subscription;
  hubConnection:any
  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private userService: UserService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {
    this.forceLogoutSubscription = this.userService.forceLogout$.subscribe(() => {
      this.handleForceLogout();
    });
  }

  ngOnInit(): void {
    this.loginForm = this.formBuilder.group({
      userName: ['', Validators.required],
      password: ['', Validators.required],
      forceLogout: [false]
    });
  }

  onSubmit() {
    this.login(false);
  }

  login(forceLogout: boolean) {
    if (this.loginForm.valid) {
      this.loginForm.patchValue({ forceLogout: forceLogout });
      this.userService.login(this.loginForm.value).subscribe({
        next: (res) => {
          if (res.success) {
            this.messageService.add({ severity: 'success', summary: 'Successful', detail: res.message });
            sessionStorage.setItem('token', res.data);
            this.userService.initializeSignalRConnection(res.data)
            .then(() => {
              this.hubConnection.on('ForceLogout', () => {
                this.handleForceLogout();
              });
            })
            .catch(err => {
              console.error('Failed to initialize SignalR connection:', err);
            });
            this.router.navigateByUrl('/');
          } else if (res.errorCode === 5232 && res.data?.requireForceLogout) {
            this.showForceLogoutPrompt = true;
            this.errorMessage = res.message;
          } 
         else if (res.errorCode === 5234 ) {
          console.log(res)
          this.handleLockOutError(res)
        } 
          
          else {
            this.messageService.add({ severity: 'error', summary: 'Authentication failed.', detail: res.message });
          }
        },
        error: (err) => {
          console.error(err);
          this.messageService.add({ severity: 'error', summary: 'Something went wrong!!!', detail: err.message });
        }
      });
    }
  }

  onForceLogout() {
    this.login(true);
  }

  cancelLogin() {
    this.showForceLogoutPrompt = false;
    this.errorMessage = '';
  }

  ngOnDestroy() {
    this.stopCountdown();
    if (this.forceLogoutSubscription) {
      this.forceLogoutSubscription.unsubscribe();
    }
  }

  private handleForceLogout() {
    // Clear session storage
    sessionStorage.removeItem('token');
    // Call logout API to invalidate session on server
    this.userService.logout().subscribe({
      next: (res) => {
        if (res.success) {
          this.messageService.add({
            severity: 'info',
            summary: 'Logged Out',
            detail: 'You have been logged out due to a new login on another device.'
          });
          this.router.navigateByUrl('/auth/login');
        }
      },
      error: (err) => {
        console.error(err);
        this.messageService.add({ severity: 'error', summary: 'Logout failed.', detail: err.message });
      }
    });
  }

  private startCountdown() {
    this.stopCountdown(); // Ensure any existing countdown is stopped
    this.countdownSubscription = interval(1000).subscribe(() => {
     
        if (this.remainingTime > 0) {
          this.remainingTime-=1;
          this.updateErrorMessage();
        } else {
          this.stopCountdown();
          this.clearErrorMessage(); // Optionally clear the error message when countdown ends
        }
      ;
    });
  }
  
  private updateErrorMessage() {
    const minutes = Math.floor(this.remainingTime / 60);
    const seconds = this.remainingTime % 60;
    this.errorMessage = `Account is locked out. Please try again after ${minutes} minutes and ${seconds} seconds.`;
    
    // Force change detection to update the view
    this.cdr.detectChanges();
  }

  private stopCountdown() {
    if (this.countdownSubscription) {
      this.countdownSubscription.unsubscribe();
      this.countdownSubscription = null;
    }
  }


  private clearErrorMessage() {
    this.errorMessage = '';
    this.errorCode = null;
    this.stopCountdown();
  }

  private handleLockOutError(res: any) {
    this.errorCode = res.errorCode;
    
    // Extract remaining time from API response
    const remainingTimeStr = res.data?.remainingLockoutTime;
    if (remainingTimeStr) {
      // Parse the time string to seconds
      this.remainingTime = this.parseRemainingTime(remainingTimeStr);
      
      if (this.remainingTime > 0) {
        this.startCountdown();
      } else {
        this.clearErrorMessage();
      }
      
      // Set the error message from the API response
      this.errorMessage = res.message;
      
      // Update the message displayed by the message service
      this.messageService.add({
        severity: 'error',
        summary: 'Account Locked',
        detail: this.errorMessage
      });
    } else {
      console.error('Unexpected remainingLockoutTime format');
    }
  }
  
  private parseRemainingTime(timeStr: string): number {
    // Split the time string into hours, minutes, and seconds
    const timeParts = timeStr.split(':');
    
    if (timeParts.length !== 3) {
      console.error('Unexpected time format');
      return 0;
    }
  
    const hours = parseInt(timeParts[0], 10) || 0;
    const minutes = parseInt(timeParts[1], 10) || 0;
  
    // Handle seconds and milliseconds
    const secondsAndMillis = timeParts[2].split('.');
    const seconds = parseInt(secondsAndMillis[0], 10) || 0;
    //const milliseconds = parseInt(secondsAndMillis[1] || '0', 10);
  
    // Calculate total seconds
    const totalSeconds = (hours * 3600) + (minutes * 60) + seconds ;
  
    console.log(`Parsed time: ${hours} hours, ${minutes} minutes, ${seconds} seconds milliseconds`);
    console.log(`Total seconds: ${totalSeconds}`);
    
    return totalSeconds;
  }
  
  
  
    
  
}
