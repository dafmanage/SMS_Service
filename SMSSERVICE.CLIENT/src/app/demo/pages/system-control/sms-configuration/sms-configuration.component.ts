import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonService } from '../../../../services/common.service';
import { MessageService } from 'primeng/api';
import { UserService } from '../../../../services/user.service';
import { UserView } from 'src/models/auth/userDto';

@Component({
  selector: 'app-sms-configuration',
  templateUrl: './sms-configuration.component.html',
  styleUrls: ['./sms-configuration.component.scss']
})
export class SmsConfigurationComponent implements OnInit {
  smsConfigForm: FormGroup;
  isLoading = false;
  config: any = {};
  currentUser: UserView | null = null;
  canEdit = false;

  constructor(
    private fb: FormBuilder,
    private commonService: CommonService,
    private messageService: MessageService,
    private userService: UserService
  ) {
    this.smsConfigForm = this.fb.group({
      providerName: ['', [Validators.required, Validators.minLength(2)]],
      apiUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]],
      apiKey: ['', [Validators.required, Validators.minLength(10)]],
      apiSecret: ['', [Validators.required, Validators.minLength(10)]],
      senderId: ['', [Validators.required, Validators.minLength(3)]],
      maxMessageLength: [160, [Validators.required, Validators.min(1), Validators.max(1600)]],
      dailyMessageLimit: [1000, [Validators.required, Validators.min(1)]],
      costPerMessage: [0.5, [Validators.required, Validators.min(0.01)]],
      enableSandbox: [true],
      sandboxPhoneNumber: ['', [Validators.pattern(/^\+?[1-9]\d{1,14}$/)]],
      enableDeliveryReport: [true],
      enableUnicode: [true],
      retryAttempts: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
      retryDelay: [30, [Validators.required, Validators.min(10), Validators.max(300)]]
    });
  }

  ngOnInit(): void {
    this.currentUser = this.userService.getCurrentUser();
    this.canEdit = this.userService.roleMatch(['SuperAdmin']);
    this.loadSmsConfiguration();
  }

  loadSmsConfiguration(): void {
    this.isLoading = true;
    this.commonService.getSmsConfiguration().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.config = response.data;
          this.smsConfigForm.patchValue(this.config);
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading SMS configuration:', error);
        this.isLoading = false;
      }
    });
  }

  saveSmsConfiguration(): void {
    if (this.smsConfigForm.valid) {
      this.isLoading = true;
      const configData = this.smsConfigForm.value;
      
      this.commonService.updateSmsConfiguration(configData).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.messageService.add({ 
              severity: 'success', 
              summary: 'Success', 
              detail: 'SMS configuration updated successfully!' 
            });
            this.loadSmsConfiguration();
          } else {
            this.messageService.add({ 
              severity: 'error', 
              summary: 'Error', 
              detail: 'Error updating configuration: ' + response.message 
            });
          }
          this.isLoading = false;
        },
            error: (error) => {
              console.error('Error updating SMS configuration:', error);
              let errorMessage = 'Error updating configuration. Please try again.';
              
              if (error.status === 403) {
                errorMessage = 'Access denied. Only SuperAdmin users can update SMS configuration.';
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

  testConnection(): void {
    if (this.smsConfigForm.get('apiUrl')?.valid && this.smsConfigForm.get('apiKey')?.valid) {
      this.isLoading = true;
      const testData = {
        apiUrl: this.smsConfigForm.get('apiUrl')?.value,
        apiKey: this.smsConfigForm.get('apiKey')?.value
      };
      
      this.commonService.testSmsConnection(testData).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.messageService.add({ 
              severity: 'success', 
              summary: 'Success', 
              detail: 'SMS connection test successful!' 
            });
          } else {
            this.messageService.add({ 
              severity: 'error', 
              summary: 'Error', 
              detail: 'SMS connection test failed: ' + response.message 
            });
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error testing SMS connection:', error);
          this.messageService.add({ 
            severity: 'error', 
            summary: 'Error', 
            detail: 'Error testing connection. Please check your configuration.' 
          });
          this.isLoading = false;
        }
      });
    } else {
      this.messageService.add({ 
        severity: 'warn', 
        summary: 'Warning', 
        detail: 'Please fill in API URL and API Key before testing connection.' 
      });
    }
  }

  resetForm(): void {
    this.smsConfigForm.reset();
    this.loadSmsConfiguration();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.smsConfigForm.controls).forEach(key => {
      const control = this.smsConfigForm.get(key);
      control?.markAsTouched();
    });
  }
}
