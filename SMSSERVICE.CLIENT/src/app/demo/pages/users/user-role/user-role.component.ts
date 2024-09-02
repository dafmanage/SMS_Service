import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService } from 'primeng/api';
import { UserService } from 'src/app/services/user.service';
import { SelectList } from 'src/models/ResponseMessage.Model';
import { UserList } from 'src/models/auth/userDto';

@Component({
  selector: 'app-user-role',
  templateUrl: './user-role.component.html',
  styleUrls: ['./user-role.component.scss']
})
export class UserRoleComponent implements OnInit {

  @Input() user : UserList
  assignedRoles : SelectList[]
  notAssignedRoles : SelectList[]
  selectedCategory !: string

  ngOnInit(): void {

    this.getAssignedRoles()
    this.getNotAssignedRoles()
    
  }

  constructor(private userService: UserService,private activeModal:NgbActiveModal,private messageService : MessageService){


  }

  onRoleCategorySelected(value: string) {
    this.selectedCategory = value
    this.getAssignedRoles()
    this.getNotAssignedRoles()
  }

  getAssignedRoles(){

    this.userService.getAssignedRole(this.user.id).subscribe(
      {
        next:(res)=>{
          this.assignedRoles = res
        }
      }
    )

  }

  assignRole(roles: any) {

console.log({  userId: this.user.id,
  roleName: roles})

    this.userService.assignRole({
      userId: this.user.id,
      roleName: roles
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });

          this.onRoleCategorySelected(this.selectedCategory)

        }
        else {
          this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: res.message });
        }
        this.closeModal();
      }
      , error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: err });

      }
    })
  }



  getSelectedRoleValues(selectElement: HTMLSelectElement): string[] {
    const selectedValues = [];
    for (let i = 0; i < selectElement.selectedOptions.length; i++) {
      selectedValues.push(selectElement.selectedOptions[i].value);
    }


    console.log(selectedValues)
    return selectedValues;
  }
  removeRole(roles: any) {

    this.userService.revokeRole({
      userId: this.user.id,
      roleName: roles
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });
          this.onRoleCategorySelected(this.selectedCategory)
        }
        else {
          this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: res.message });
        }
        this.closeModal();
      }
      , error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: err });

      }
    })
  }
  getNotAssignedRoles(){
    this.userService.getNotAssignedRole(this.user.id).subscribe(
      {
        next:(res)=>{
          this.notAssignedRoles = res
        }
      }
    )
  }

  closeModal(){
    this.activeModal.close()
  }
}
