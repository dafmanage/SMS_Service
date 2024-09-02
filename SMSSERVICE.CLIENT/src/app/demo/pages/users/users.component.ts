import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AddUserComponent } from './add-user/add-user.component';
import { UserList } from 'src/models/auth/userDto';
import { UserService } from 'src/app/services/user.service';
import { UserRoleComponent } from './user-role/user-role.component';
@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, SharedModule,TableModule],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export default class UsersComponent implements OnInit {


  users: UserList[]
  ngOnInit(): void {
    this.getUsers()
    
  }
  constructor( 
    private modalService : NgbModal,
    private userService:UserService){

  }

  getUsers(){

    this.userService.getUserList().subscribe({
      next:(res)=>{
        console.log("current user",res)
        this.users = res 
      }
    })

  }


  addUser(){
    let modalRef = this.modalService.open(AddUserComponent,{size:'lg',backdrop:'static'})

    modalRef.result.then(()=>{
      this.getUsers()
    })

  }

  userRoles(user:UserList){
    let modalRef = this.modalService.open(UserRoleComponent,{size:'lg',backdrop:'static'})
    modalRef.componentInstance.user  = user
    modalRef.result.then(()=>{
      this.getUsers()
    })
  }
}
