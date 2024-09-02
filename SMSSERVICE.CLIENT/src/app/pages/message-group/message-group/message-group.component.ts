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
  constructor(private modalService:NgbModal,
    private msgService: MessageGroupService,
    private userService:UserService,
    private orgService: OrganizationService){}
  ngOnInit(): void {
    this.user= this.userService.getCurrentUser()
    if(this.allowedRoles(['Admin'])){this.getOrganizationsSelectList()}
    if(this.allowedRoles(['Organization'])){
      this.getMessageGroups()}
  }
  addGroup(){
    let modalRef = this.modalService.open(AddGroupsComponent, { size: 'lg', backdrop: 'static' })

    modalRef.result.then(() => {
      //this.getOrganizations()
    })
  }
  getMessageGroups(){

    this.msgService.getMessageGroups(this.user.organizationId).subscribe({
      next: (res) => {

        this.messagegroups = res
        console.log(this.messagegroups)

      },
      error: (err) => {
        console.log(err)
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
    return this.userService.roleMatch(allowedRoles)
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
