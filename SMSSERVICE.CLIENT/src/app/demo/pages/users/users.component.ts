import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { DropdownModule } from 'primeng/dropdown';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AddUserComponent } from './add-user/add-user.component';
import { UserList } from 'src/models/auth/userDto';
import { UserService } from 'src/app/services/user.service';
import { UserRoleComponent } from './user-role/user-role.component';
import { OrganizationService } from 'src/app/services/organization.service';
import { SelectItem } from 'primeng/api';
@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule, SharedModule, TableModule, DropdownModule],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export default class UsersComponent implements OnInit {
  users: UserList[] = [];
  filteredUsers: UserList[] = [];
  isLoading = false;
  selectedOrganization: string = '';
  organizationOptions: SelectItem[] = [];
  showOrganizationFilter = true; // Control visibility of organization filter

  ngOnInit(): void {
    this.getUsers();
    this.getOrganizations();
    
    // Check user role and set organization filter accordingly
    const currentUser = this.userService.getCurrentUser();
    if (currentUser && currentUser.role && !currentUser.role.includes('SuperAdmin')) {
      // Admin users should only see their organization's users
      this.selectedOrganization = currentUser.organizationId;
      this.showOrganizationFilter = false; // Hide organization filter for Admin users
    } else {
      // SuperAdmin users can see all organizations
      this.selectedOrganization = '';
      this.showOrganizationFilter = true; // Show organization filter for SuperAdmin users
    }
  }

  constructor( 
    private modalService: NgbModal,
    private userService: UserService,
    private organizationService: OrganizationService
  ) {}

  getUsers(): void {
    this.isLoading = true;
    console.log("DEBUG: Calling getUserList API...");
    this.userService.getUserList().subscribe({
      next: (res) => {
        console.log("DEBUG: API Response received:", res);
        console.log("DEBUG: Response type:", typeof res);
        console.log("DEBUG: Response length:", Array.isArray(res) ? res.length : 'Not an array');
        
        // Convert backend response to frontend format
        this.users = (res || []).map((user: any) => {
          console.log('DEBUG: Converting user:', user);
          const convertedUser = {
            ...user,
            organizationId: user.organizationId?.toString() || '', // Convert Guid to string
            roles: user.roles || [], // Ensure roles is always an array
            phoneNumber: user.phoneNumber || 'N/A', // Handle empty phone numbers
            email: user.email || 'N/A', // Handle empty emails
            name: user.name || `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.userName || 'Unknown'
          };
          console.log('DEBUG: Converted user:', convertedUser);
          return convertedUser;
        });
        
        this.filteredUsers = [...this.users];
        
        // Auto-filter based on user role
        this.onOrganizationFilter();
        
        console.log("DEBUG: Users array length:", this.users.length);
        console.log("DEBUG: Filtered users array length:", this.filteredUsers.length);
        console.log("DEBUG: First user data:", this.users[0]);
        this.isLoading = false;
      },
      error: (error) => {
        console.error("DEBUG: Error loading users:", error);
        console.error("DEBUG: Error status:", error.status);
        console.error("DEBUG: Error message:", error.message);
        this.isLoading = false;
      }
    });
  }

  getOrganizations(): void {
    console.log('DEBUG: Loading organizations...');
    this.organizationService.getOrganizationsNoUserSelectList().subscribe({
      next: (organizations: any[]) => {
        console.log('DEBUG: Organizations received:', organizations);
        this.organizationOptions = [
          { label: 'All Organizations', value: '' },
          ...organizations.map(org => {
            console.log(`DEBUG: Mapping org ${org.name} with id ${org.id}`);
            return { label: org.name, value: org.id };
          })
        ];
        console.log('DEBUG: Organization options:', this.organizationOptions);
      },
      error: (err) => {
        console.error('Error loading organizations:', err);
      }
    });
  }

  onOrganizationFilter(): void {
    console.log('DEBUG: Organization filter changed to:', this.selectedOrganization);
    console.log('DEBUG: Available users:', this.users.length);
    console.log('DEBUG: User organization IDs:', this.users.map(u => u.organizationId));
    
    if (!this.selectedOrganization || this.selectedOrganization === '') {
      this.filteredUsers = [...this.users];
      console.log('DEBUG: Showing all users:', this.filteredUsers.length);
    } else {
      this.filteredUsers = this.users.filter(user => {
        const matches = user.organizationId === this.selectedOrganization;
        console.log(`DEBUG: User ${user.userName} orgId ${user.organizationId} matches ${this.selectedOrganization}: ${matches}`);
        return matches;
      });
      console.log('DEBUG: Filtered users count:', this.filteredUsers.length);
    }
  }

  getOrganizationName(organizationId: string): string {
    console.log(`DEBUG: Getting organization name for ID: ${organizationId}`);
    console.log('DEBUG: Available organization options:', this.organizationOptions);
    const org = this.organizationOptions.find(option => option.value === organizationId);
    const result = org ? org.label : organizationId;
    console.log(`DEBUG: Organization name result: ${result}`);
    return result;
  }

  formatDate(date: Date | string | null | undefined): string {
    if (!date) return 'Never';
    try {
      return new Date(date).toLocaleDateString();
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }


  addUser(): void {
    const modalRef = this.modalService.open(AddUserComponent, { size: 'lg', backdrop: 'static' });
    modalRef.result.then(() => {
      this.getUsers();
    }).catch(() => {
      // Modal dismissed
    });
  }

}
