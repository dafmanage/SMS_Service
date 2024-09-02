import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService, SelectItem } from 'primeng/api';
import { MessageGroupService } from 'src/app/services/message-group.service';
import { SendMessageService } from 'src/app/services/send-message.service';
import { UserService } from 'src/app/services/user.service';
import { UserView } from 'src/models/auth/userDto';
import { IMessagPostDto } from 'src/models/s-msg/s-msg.model';

@Component({
  selector: 'app-create-message',
  templateUrl: './create-message.component.html',
  styleUrls: ['./create-message.component.scss']
})
export class CreateMessageComponent implements OnInit{
  groupSelectList: SelectItem[] = []
  sendmessageForm: FormGroup;
  groupOptions: SelectItem[] = []
  selectedGroup: string = null;
  languageDropDownItem = [
    { name: 'ENGLISH', code: 'ENGLISH'},
    { name: 'AMHARIC', code: 'AMHARIC'}
  ]
  user !: UserView

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private messageService: MessageService,
    private activeModal : NgbActiveModal,
    private sndMessage: SendMessageService,
    private msgService: MessageGroupService
    ) { }

    ngOnInit(): void {

      this.user = this.userService.getCurrentUser()

      this.sendmessageForm = this.formBuilder.group({
        message: [null, Validators.required],
        language: [null, Validators.required],
        group: [null, Validators.required],
        //organizationId: [null,Validators.required]

      });
      this.getMessageGroups()
    }
    getMessageGroups() {
      this.msgService.getMessageGroups(this.user.organizationId).subscribe({
        next: (res) => {
          this.groupSelectList = res.map(item => ({ value: item.id, label: item.groupName}));
        }
      })
    }


    onSubmit() {

      console.log(this.sendmessageForm.value)
      if (this.sendmessageForm.valid) {


        const value : IMessagPostDto = {

          content: this.sendmessageForm.value.message,
          language: this.sendmessageForm.value.language,
          messageGroupId: this.sendmessageForm.value.group,
          createdById: this.user.userId,
          organizationId: this.user.organizationId
        }





        this.sndMessage.addMessage(value).subscribe({
          next: (res) => {

            if (res.success) {
              // this.sndMessage.getMessageGroupNameById(value.messageGroupId).subscribe({
              //   next: (messageGroupName) => {

              //     // Show the message group name in the UI using Angular's data binding
              //     this.messageGroupName = messageGroupName;

              //   },
              //   error: (err) => {
              //     // Handle the error
              //   }
              // });
              this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });

              this.sendmessageForm.reset();

              this.closeModal()


            }
            else {
              this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: res.message });

            }


          }, error: (err) => {
            this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: err });
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
