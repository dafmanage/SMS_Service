import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { IMessagGroupGetDto } from 'src/models/msg/msg.model';
import { MessageGroupService } from 'src/app/services/message-group.service';

import { AddGroupsComponent } from './add-groups/add-groups.component';
import { UserView } from 'src/models/auth/userDto';
import { UserService } from 'src/app/services/user.service';
import { UpdateGroupComponent } from './update-group/update-group.component';
import { SelectItem } from 'primeng/api';
import { OrganizationService } from 'src/app/services/organization.service';

@Component({
  selector: 'app-message-group',
  templateUrl: './message-group.component.html',
  styleUrls: ['./message-group.component.scss']
})
export class MessageGroupComponent implements OnInit {
  selectedOrganization: string = null;
  selectedOption:string
  user !: UserView
  messagegroups:IMessagGroupGetDto[]
  messagegroupSelectList: SelectItem[] = []
  showOrganizationFilter = false; 
  isAdmin: boolean = false; 
  constructor(private modalService:NgbModal,
    private msgService: MessageGroupService,
    private userService:UserService,
    private orgService: OrganizationService){}
  ngOnInit(): void {
    this.user = this.userService.getCurrentUser()
    console.log('DEBUG: MessageGroupComponent - Current user:', this.user);
    console.log('DEBUG: MessageGroupComponent - User organizationId:', this.user?.organizationId);
    console.log('DEBUG: MessageGroupComponent - User roles:', this.user?.role);
    
    if (!this.user) {
      console.error('DEBUG: MessageGroupComponent - No current user found');
      return;
    }
    
    // Check user role and load appropriate data
    console.log('DEBUG: MessageGroupComponent - Checking roles...');
    console.log('DEBUG: MessageGroupComponent - Is SuperAdmin?', this.allowedRoles(['SuperAdmin']));
    console.log('DEBUG: MessageGroupComponent - Is Admin?', this.allowedRoles(['Admin']));
    console.log('DEBUG: MessageGroupComponent - User roles:', this.user.role);
    console.log('DEBUG: MessageGroupComponent - Role details:', JSON.stringify(this.user.role));
    
    if(this.allowedRoles(['SuperAdmin'])){
      // SuperAdmin: View only, filter by organization
      console.log('DEBUG: MessageGroupComponent - SuperAdmin user: View only with organization filter');
      this.getOrganizationsSelectList()
      this.showOrganizationFilter = true
      this.isAdmin = false // SuperAdmin is read-only
    } else if(this.allowedRoles(['Admin'])){
      // Admin: Full access to their organization (add/edit message groups)
      console.log('DEBUG: MessageGroupComponent - Admin user: Full access to their organization');
      this.getMessageGroups()
      this.showOrganizationFilter = false
      this.isAdmin = true // Admin can add/edit
    } else {
      // Fallback for any other authenticated user
      console.log('DEBUG: MessageGroupComponent - User has no specific role, loading default view');
      this.getMessageGroups()
      this.showOrganizationFilter = false
      this.isAdmin = false // Default to read-only
    }
  }
  addGroup(){
    let modalRef = this.modalService.open(AddGroupsComponent, { size: 'lg', backdrop: 'static' })

    modalRef.result.then(() => {
      // Refresh the message groups list after adding
      this.getMessageGroups()
    })
  }

  createSampleGroups(){
    console.log('DEBUG: Creating sample message groups...');
    console.log('DEBUG: Current user:', this.user);
    console.log('DEBUG: User organizationId:', this.user?.organizationId);
    
    this.msgService.createSampleMessageGroups().subscribe({
      next: (res) => {
        console.log('DEBUG: Sample groups created:', res);
        if (res.success) {
          // Refresh the message groups list
          console.log('DEBUG: Refreshing message groups after creating samples...');
          this.getMessageGroups();
          alert('Sample message groups created successfully!');
        } else {
          console.error('DEBUG: Sample groups creation failed:', res.message);
          alert('Error: ' + res.message);
        }
      },
      error: (err) => {
        console.error('DEBUG: Error creating sample groups:', err);
        console.error('DEBUG: Error details:', err.error);
        console.error('DEBUG: Error status:', err.status);
        alert('Error creating sample groups: ' + (err.error?.message || err.message));
      }
    })
  }
  getMessageGroups(){
    console.log('DEBUG: MessageGroupComponent - getMessageGroups() called');
    console.log('DEBUG: MessageGroupComponent - User object:', this.user);
    console.log('DEBUG: MessageGroupComponent - User organizationId:', this.user?.organizationId);
    
    if (!this.user || !this.user.organizationId) {
      console.error('DEBUG: MessageGroupComponent - No user or organizationId found');
      console.error('DEBUG: MessageGroupComponent - User:', this.user);
      console.error('DEBUG: MessageGroupComponent - OrganizationId:', this.user?.organizationId);
      return;
    }

    console.log('DEBUG: MessageGroupComponent - Getting message groups for organization:', this.user.organizationId);
    console.log('DEBUG: MessageGroupComponent - API URL will be:', '/MessageGroup?OrganizationId=' + this.user.organizationId);
    
    this.msgService.getMessageGroups(this.user.organizationId).subscribe({
      next: (res) => {
        this.messagegroups = res
        console.log('DEBUG: MessageGroupComponent - Message groups loaded:', this.messagegroups)
        console.log('DEBUG: MessageGroupComponent - Number of groups:', this.messagegroups?.length || 0)
      },
      error: (err) => {
        console.error('DEBUG: MessageGroupComponent - Error loading message groups:', err)
        console.error('DEBUG: MessageGroupComponent - Error details:', err.error)
        console.error('DEBUG: MessageGroupComponent - Error status:', err.status)
      }
    })
  }
  updateGroup(messagegroup: IMessagGroupGetDto){
    console.log(messagegroup)

    let modalRef = this.modalService.open(UpdateGroupComponent,{size:'lg',backdrop:'static'})

    modalRef.componentInstance.messagegroup = messagegroup
    modalRef.result.then(()=>{
      this.getMessageGroups()
    })
  }
  getOrganizationsSelectList(){
    this.orgService.getOrganizationsSelectList().subscribe({
      next: (res) => {
        this.messagegroupSelectList = res.map(item => ({ value: item.id, label: item.name}));
      }
    })
  }
  allowedRoles(allowedRoles: any)
  {
    const result = this.userService.roleMatch(allowedRoles);
    console.log('DEBUG: allowedRoles called with:', allowedRoles, 'result:', result);
    return result;
  }
  getGroups(value:string){

    this.msgService.getMessageGroups(value).subscribe({
      next: (res) => {

        this.messagegroups = res
        console.log(this.messagegroups)

      },
      error: (err) => {
        console.log(err)
      }
    })
  }
}
