import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService, SelectItem } from 'primeng/api';
import { GroupPhonesService } from 'src/app/services/group-phones.service';
import { MessageGroupService } from 'src/app/services/message-group.service';
import { UserService } from 'src/app/services/user.service';
import { UserView } from 'src/models/auth/userDto';
import { IPhoneGroupPostDto } from 'src/models/phn/phn.model';

@Component({
  selector: 'app-add-phone',
  templateUrl: './add-phone.component.html',
  styleUrls: ['./add-phone.component.scss']
})
export class AddPhoneComponent {
  groupSelectList: SelectItem[] = []

  groupphonesForm!: FormGroup;
  user !: UserView



  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private messageService: MessageService,
    private activeModal : NgbActiveModal,
    private phnService: GroupPhonesService,
    private msgService: MessageGroupService
    ) { }

  ngOnInit(): void {

    this.user = this.userService.getCurrentUser()

    this.groupphonesForm = this.formBuilder.group({
      fullName: [null, Validators.required],
      phoneNumber: [null, Validators.required],
      remark: [null, Validators.required],
      groupName: [null, Validators.required]


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

    console.log(this.groupphonesForm.value)
    if (this.groupphonesForm.valid) {


      const value : IPhoneGroupPostDto = {

        fullName: this.groupphonesForm.value.fullName,
        phoneNumber: this.groupphonesForm.value.phoneNumber,
        remark: this.groupphonesForm.value.remark,
        createdById: this.user.userId,
        messageGroupId: this.groupphonesForm.value.groupName

      }





      this.phnService.addGroupPhone(value).subscribe({
        next: (res) => {

          if (res.success) {
            this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });

            this.groupphonesForm.reset();

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
