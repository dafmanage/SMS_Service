import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonService } from '../../../../services/common.service';
import { MessageService } from 'primeng/api';
import { UserService } from '../../../../services/user.service';
import { UserView } from 'src/models/auth/userDto';

@Component({
  selector: 'app-general-settings',
  templateUrl: './general-settings.component.html',
  styleUrls: ['./general-settings.component.scss']
})
export class GeneralSettingsComponent implements OnInit {
  generalSettingsForm: FormGroup;
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
    this.generalSettingsForm = this.fb.group({
      applicationName: ['', [Validators.required, Validators.minLength(2)]],
      companyName: ['', [Validators.required, Validators.minLength(2)]],
      companyAddress: ['', [Validators.required]],
      companyPhone: ['', [Validators.required]],
      companyEmail: ['', [Validators.required, Validators.email]],
      sessionTimeout: [15, [Validators.required, Validators.min(5), Validators.max(60)]],
      maxLoginAttempts: [5, [Validators.required, Validators.min(3), Validators.max(10)]],
      enableNotifications: [true],
      enableAuditLog: [true],
      defaultLanguage: ['en', [Validators.required]],
      timeZone: ['UTC', [Validators.required]]
    });
  }

  ngOnInit(): void {
    console.log('DEBUG: GeneralSettingsComponent ngOnInit called');
    
    // Debug sessionStorage content
    console.log('DEBUG: SessionStorage keys:', Object.keys(sessionStorage));
    console.log('DEBUG: SessionStorage token:', sessionStorage.getItem('token'));
    console.log('DEBUG: SessionStorage token length:', sessionStorage.getItem('token')?.length);
    
    this.currentUser = this.userService.getCurrentUser();
    console.log('DEBUG: Current user:', this.currentUser);
    
    // Debug token content
    const token = sessionStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(window.atob(token.split('.')[1]));
        console.log('DEBUG: Full JWT payload:', payload);
        console.log('DEBUG: Role in token:', payload.role);
        console.log('DEBUG: Role type:', typeof payload.role);
      } catch (error) {
        console.error('DEBUG: Error decoding token:', error);
      }
    }
    
    this.canEdit = this.userService.roleMatch(['SuperAdmin']);
    console.log('DEBUG: Can edit (SuperAdmin):', this.canEdit);
    this.loadGeneralSettings();
  }

  loadGeneralSettings(): void {
    this.isLoading = true;
    this.commonService.getGeneralSettings().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.settings = response.data;
          this.generalSettingsForm.patchValue(this.settings);
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading general settings:', error);
        this.isLoading = false;
      }
    });
  }

  saveGeneralSettings(): void {
    if (this.generalSettingsForm.valid) {
      this.isLoading = true;
      const settingsData = this.generalSettingsForm.value;
      
      this.commonService.updateGeneralSettings(settingsData).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.messageService.add({ 
              severity: 'success', 
              summary: 'Success', 
              detail: 'General settings updated successfully!' 
            });
            this.loadGeneralSettings();
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
          console.error('Error updating general settings:', error);
          let errorMessage = 'Error updating settings. Please try again.';
          
          if (error.status === 403) {
            errorMessage = 'Access denied. Only SuperAdmin users can update general settings.';
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
    this.generalSettingsForm.reset();
    this.loadGeneralSettings();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.generalSettingsForm.controls).forEach(key => {
      const control = this.generalSettingsForm.get(key);
      control?.markAsTouched();
    });
  }
}
