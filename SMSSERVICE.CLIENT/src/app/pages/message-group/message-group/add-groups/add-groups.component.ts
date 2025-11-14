import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService } from 'primeng/api';
import { MessageGroupService } from 'src/app/services/message-group.service';
import { UserService } from 'src/app/services/user.service';
import { UserView } from 'src/models/auth/userDto';
import { IMessagGroupPostDto } from 'src/models/msg/msg.model';

@Component({
  selector: 'app-add-group',
  templateUrl: './add-groups.component.html',
  styleUrls: ['./add-groups.component.scss']
})
export class AddGroupsComponent implements OnInit{
  messagegroupsForm!: FormGroup;
  user !: UserView



  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private messageService: MessageService,
    private activeModal : NgbActiveModal,
    private msgService : MessageGroupService
    ) { }

  ngOnInit(): void {

    this.user = this.userService.getCurrentUser()

    this.messagegroupsForm = this.formBuilder.group({
      groupName: [null, Validators.required],
      groupCode: [null, Validators.required],
      remark: [null, Validators.required]


    });
  }

  onSubmit() {

    console.log(this.messagegroupsForm.value)
    if (this.messagegroupsForm.valid) {

      // const formData = new FormData();
      // formData.append("groupName", this.messagegroupsForm.value.groupName);
      // formData.append("groupCode", this.messagegroupsForm.value.groupCode);
      // formData.append("remark", this.messagegroupsForm.value.remark);
      // formData.append("createdById", this.user.userId);
      // formData.append("organizationId", this.user.organizationId);
      const value : IMessagGroupPostDto = {
        groupName: this.messagegroupsForm.value.groupName,
        groupCode: this.messagegroupsForm.value.groupCode,
        remark: this.messagegroupsForm.value.remark,
        organizationId: this.user.organizationId,
        createdById: this.user.userId

      }





      this.msgService.addMessageGroup(value).subscribe({
        next: (res) => {

          if (res.success) {
            this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });

            this.messagegroupsForm.reset();

            this.closeModal()


          }
          else {
            // Handle validation errors properly
            let errorMessage = res.message;
            if (res.data && Array.isArray(res.data)) {
              errorMessage = res.data.join(', ');
            }
            this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });

          }

        }, error: (err) => {
          console.error('Error:', err);
          let errorMessage = 'An unexpected error occurred';
          if (err.error && err.error.message) {
            errorMessage = err.error.message;
            if (err.error.data && Array.isArray(err.error.data)) {
              errorMessage += ': ' + err.error.data.join(', ');
            }
          } else if (err.message) {
            errorMessage = err.message;
          }
          this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
        }
      })



    }
    else {
      this.messageService.add({ severity: 'error', summary: 'Form Submit failed.', detail: "Please fil required inputs !!" });
    }


  }


  closeModal(){

    this.activeModal.close()
  }

}
