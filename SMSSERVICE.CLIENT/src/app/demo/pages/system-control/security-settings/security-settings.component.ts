import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonService } from '../../../../services/common.service';
import { MessageService } from 'primeng/api';
import { UserService } from '../../../../services/user.service';
import { UserView } from 'src/models/auth/userDto';

@Component({
  selector: 'app-security-settings',
  templateUrl: './security-settings.component.html',
  styleUrls: ['./security-settings.component.scss']
})
export class SecuritySettingsComponent implements OnInit {
  securityForm: FormGroup;
  isLoading = false;
  settings: any = {};
  currentUser: UserView | null = null;
  canEdit = false;

  constructor(
    private fb: FormBuilder,
    private commonService: CommonService,
    private messageService: MessageService,
    private userService: UserService
  ) {
    this.securityForm = this.fb.group({
      passwordMinLength: [8, [Validators.required, Validators.min(6), Validators.max(20)]],
      requireUppercase: [true],
      requireLowercase: [true],
      requireNumbers: [true],
      requireSpecialChars: [true],
      passwordExpiryDays: [90, [Validators.required, Validators.min(30), Validators.max(365)]],
      maxLoginAttempts: [5, [Validators.required, Validators.min(3), Validators.max(10)]],
      lockoutDuration: [15, [Validators.required, Validators.min(5), Validators.max(60)]],
      enableTwoFactor: [false],
      sessionTimeout: [15, [Validators.required, Validators.min(5), Validators.max(120)]],
      enableAuditLog: [true],
      enableIpWhitelist: [false],
      allowedIpAddresses: [''],
      enableHttpsOnly: [true],
      enableSecurityHeaders: [true],
      enableRateLimiting: [true],
      rateLimitRequests: [100, [Validators.required, Validators.min(10), Validators.max(1000)]],
      rateLimitWindow: [60, [Validators.required, Validators.min(1), Validators.max(3600)]]
    });
  }

  ngOnInit(): void {
    this.currentUser = this.userService.getCurrentUser();
    this.canEdit = this.userService.roleMatch(['SuperAdmin']);
    this.loadSecuritySettings();
  }

  loadSecuritySettings(): void {
    this.isLoading = true;
    this.commonService.getSecuritySettings().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.settings = response.data;
          this.securityForm.patchValue(this.settings);
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading security settings:', error);
        this.isLoading = false;
      }
    });
  }

  saveSecuritySettings(): void {
    if (this.securityForm.valid) {
      this.isLoading = true;
      const settingsData = this.securityForm.value;
      
      this.commonService.updateSecuritySettings(settingsData).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.messageService.add({ 
              severity: 'success', 
              summary: 'Success', 
              detail: 'Security settings updated successfully!' 
            });
            this.loadSecuritySettings();
          } else {
            this.messageService.add({ 
              severity: 'error', 
              summary: 'Error', 
              detail: 'Error updating settings: ' + response.message 
            });
          }
          this.isLoading = false;
        },
            error: (error) => {
              console.error('Error updating security settings:', error);
              let errorMessage = 'Error updating settings. Please try again.';
              
              if (error.status === 403) {
                errorMessage = 'Access denied. Only SuperAdmin users can update security settings.';
              } else if (error.status === 401) {
                errorMessage = 'Session expired. Please login again.';
              } else if (error.error?.message) {
                errorMessage = error.error.message;
              }
              
              this.messageService.add({ 
                severity: 'error', 
                summary: 'Error', 
                detail: errorMessage
              });
              this.isLoading = false;
            }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  resetForm(): void {
    this.securityForm.reset();
    this.loadSecuritySettings();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.securityForm.controls).forEach(key => {
      const control = this.securityForm.get(key);
      control?.markAsTouched();
    });
  }
}
