import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService } from 'primeng/api';
import { OrganizationService } from 'src/app/services/organization.service';
//import { HrmService } from 'src/app/services/hrm.service';
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
  organizationList: any;
  organization !: string;
  ngOnInit(): void {

    this.userForm = this.formBuilder.group({
      OrganizationId : ['',Validators.required],
      UserName: ['', Validators.required],
      Password: ['', Validators.required],
      ConfirmPassword: ['', Validators.required],
    });

    this.getOrganizations()
  }

  constructor(
    private orgService: OrganizationService,
    //private hrmService: HrmService,
    private userService: UserService,
    private formBuilder: FormBuilder,
    private activeModal: NgbActiveModal,
    private messageService: MessageService
  ){

  }
  getOrganizations() {

    this.orgService.getOrganizationsNoUserSelectList().subscribe({
      next: (res) => {
        this.organizationList = res
        console.log(res)
      }
      , error: (err) => {
        console.error(err)
      }
    })

  }
  // getEmployees() {

  //   this.hrmService.getEmployeesNoUserSelectList().subscribe({
  //     next: (res) => {
  //       this.employeeList = res
  //       console.log(res)
  //     }
  //     , error: (err) => {
  //       console.error(err)
  //     }
  //   })

  // }

  submit() {

    if (this.userForm.valid) {
      if (this.userForm.value.Password === this.userForm.value.ConfirmPassword) {
        let user: UserPost = {
          organizationId: this.userForm.value.OrganizationId,
          password: this.userForm.value.Password,
          userName: this.userForm.value.UserName
        }


        this.userService.createUser(user).subscribe({
          next: (res) => {
            if(res.success){
            this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });
                }
          else{
            this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: res.message });
          }
            this.closeModal();
          }
          , error: (err) => {
            this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail:err });

          }
        })


      }
      else {

      }
    }
    else {
    }

  }
  selectOrganization(event: string) {
    this.organization = event

  }
  closeModal(){

    this.activeModal.close()

  }

}
