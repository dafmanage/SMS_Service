import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService } from 'primeng/api';
import { OrganizationService } from 'src/app/services/organization.service';
import { UserService } from 'src/app/services/user.service';
import { SelectList } from 'src/models/ResponseMessage.Model';
import { UserPost } from 'src/models/auth/userDto';

@Component({
  selector: 'app-add-user',
  templateUrl: './add-user.component.html',
  styleUrls: ['./add-user.component.scss']
})
export class AddUserComponent implements OnInit {

  userForm!: FormGroup;
  organizationList: any[] = [];
  roleList: any[] = [];
  organization !: string;
  
  ngOnInit(): void {
    this.userForm = this.formBuilder.group({
      OrganizationId : ['',Validators.required],
      UserName: ['', Validators.required],
      Password: ['', [Validators.required, Validators.minLength(12)]],
      ConfirmPassword: ['', Validators.required],
      Role: ['Admin', Validators.required], // Default to Admin
      FirstName: ['', Validators.required],
      LastName: ['', Validators.required],
      Email: ['', [Validators.required, Validators.email]],
      PhoneNumber: ['', [Validators.required, Validators.pattern(/^(\+251|0)?[0-9]{9}$/)]]
    });

    // Add password confirmation validation
    this.userForm.get('ConfirmPassword')?.valueChanges.subscribe(() => {
      this.userService.comparePasswords(this.userForm);
    });

    this.getOrganizations();
    this.getRoles();
  }

  constructor(
    private orgService: OrganizationService,
    private userService: UserService,
    private formBuilder: FormBuilder,
    private activeModal: NgbActiveModal,
    private messageService: MessageService
  ){

  }
  
  getOrganizations() {
    console.log('DEBUG: AddUserComponent - Loading organizations...');
    
    // Get current user to determine which organizations they can see
    const currentUser = this.userService.getCurrentUser();
    console.log('DEBUG: AddUserComponent - Current user:', currentUser);
    
    if (currentUser && currentUser.userId) {
      this.orgService.getOrganizationsForUser(currentUser.userId).subscribe({
        next: (res) => {
          this.organizationList = res || [];
          console.log('DEBUG: AddUserComponent - Organizations loaded successfully:', this.organizationList);
          if (this.organizationList.length === 0) {
            this.messageService.add({ 
              severity: 'warn', 
              summary: 'No Organizations', 
              detail: 'No organizations found. Please create an organization first.' 
            });
          }
        },
        error: (err) => {
          console.error('DEBUG: AddUserComponent - Error loading organizations:', err);
          this.messageService.add({ 
            severity: 'error', 
            summary: 'Error', 
            detail: 'Failed to load organizations. Please try again.' 
          });
        }
      });
    } else {
      console.log('DEBUG: AddUserComponent - No current user found, using fallback method');
      // Fallback to all organizations if user info is not available
      this.orgService.getOrganizationsNoUserSelectList().subscribe({
        next: (res) => {
          this.organizationList = res || [];
          console.log('DEBUG: AddUserComponent - Organizations loaded successfully (fallback):', this.organizationList);
          if (this.organizationList.length === 0) {
            this.messageService.add({ 
              severity: 'warn', 
              summary: 'No Organizations', 
              detail: 'No organizations found. Please create an organization first.' 
            });
          }
        },
        error: (err) => {
          console.error('DEBUG: AddUserComponent - Error loading organizations (fallback):', err);
          this.messageService.add({ 
            severity: 'error', 
            summary: 'Error', 
            detail: 'Failed to load organizations. Please try again.' 
          });
        }
      });
    }
  }

  getRoles() {
    // Define available roles (only Admin allowed)
    this.roleList = [
      { label: 'Admin', value: 'Admin' }
    ];
  }

  onSubmit() {
    console.log(this.userForm.value);
    
    if (this.userForm.valid) {
      const userData: UserPost = {
        organizationId: this.userForm.value.OrganizationId,
        userName: this.userForm.value.UserName,
        password: this.userForm.value.Password,
        email: this.userForm.value.Email,
        firstName: this.userForm.value.FirstName,
        lastName: this.userForm.value.LastName,
        phoneNumber: this.userForm.value.PhoneNumber,
        Roles: this.userForm.value.Role
      };

      this.userService.createUser(userData).subscribe({
        next: (res) => {
          if (res.success) {
            this.messageService.add({ 
              severity: 'success', 
              summary: 'Success', 
              detail: res.message 
            });
            this.userForm.reset();
            this.closeModal();
          } else {
            this.messageService.add({ 
              severity: 'error', 
              summary: 'Something went Wrong', 
              detail: res.message 
            });
          }
        },
        error: (err) => {
          console.error('Error:', err);
          let errorMessage = 'An unexpected error occurred';
          
          // Handle validation errors from backend
          if (err.error && err.error.message) {
            errorMessage = err.error.message;
            if (err.error.data && Array.isArray(err.error.data)) {
              errorMessage = err.error.data.join(', ');
            }
          } else if (err.message) {
            errorMessage = err.message;
          } else if (err.status === 400) {
            errorMessage = 'Validation failed. Please check your input.';
          } else if (err.status === 0) {
            errorMessage = 'Unable to connect to server. Please check your internet connection.';
          }
          
          this.messageService.add({ 
            severity: 'error', 
            summary: 'Validation Error', 
            detail: errorMessage 
          });
        }
      });
    } else {
      this.messageService.add({ 
        severity: 'error', 
        summary: 'Form Submit failed.', 
        detail: "Please fill required inputs !!" 
      });
    }
  }

  closeModal(){
    this.activeModal.close();
  }

  // Password validation methods
  hasUppercase(): boolean {
    const password = this.userForm.get('Password')?.value;
    return password && /[A-Z]/.test(password);
  }

  hasLowercase(): boolean {
    const password = this.userForm.get('Password')?.value;
    return password && /[a-z]/.test(password);
  }

  hasNumber(): boolean {
    const password = this.userForm.get('Password')?.value;
    return password && /[0-9]/.test(password);
  }

  hasSpecialChar(): boolean {
    const password = this.userForm.get('Password')?.value;
    return password && /[^A-Za-z0-9]/.test(password);
  }
}
