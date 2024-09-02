import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService, SelectItem } from 'primeng/api';
import { MessageGroupService } from 'src/app/services/message-group.service';
import { UserService } from 'src/app/services/user.service';
import { IMessagGetDto } from 'src/models/s-msg/s-msg.model';
import { CreateMessageComponent } from './create-message/create-message/create-message.component';
import { SendMessageService } from 'src/app/services/send-message.service';
import { UpdatMessageComponent } from './updat-message/updat-message.component';
import { UserView } from 'src/models/auth/userDto';
import { IMessagGroupGetDto } from 'src/models/msg/msg.model';
import { __values } from 'tslib';

@Component({
  selector: 'app-send-message',
  templateUrl: './send-message.component.html',
  styleUrls: ['./send-message.component.scss']
})
export class SendMessageComponent implements OnInit{
  sendmessages:IMessagGetDto[]
  selectedGroupData: any;
  user !: UserView
  isFirstSelectOptionSelected = false;
  messagegroups:IMessagGroupGetDto[]
  groupOptions: SelectItem[] = []
  selectedGroup: string = null;
  

  constructor(private modalService:NgbModal,
    private messageService:MessageService,
    private userService:UserService,
    private msgService: MessageGroupService,
    private sndMessage: SendMessageService){}

  ngOnInit(): void {
    this.user= this.userService.getCurrentUser()
    this.getGroup(this.user.organizationId)
  }

  sendMessage(){
    let modalRef = this.modalService.open(CreateMessageComponent, { size: 'lg', backdrop: 'static' })

    modalRef.result.then(() => {

    })
  }
  getMessage(id:string){

    this.sndMessage.getMessage(id).subscribe({
      next: (res) => {

        this.sendmessages = res
        console.log(this.sendmessages)

      },
      error: (err) => {
        console.log(err)
      }
    })
  //   const selectedGroupObject = this.groupOptions.find(option => option.value === this.selectedGroup);
  // if (selectedGroupObject) {
  //   this.selectedGroupData = selectedGroupObject.label;
  // }
  }
  updateMessage(sendmessages: IMessagGetDto){
    console.log(sendmessages)

    let modalRef = this.modalService.open(UpdatMessageComponent,{size:'lg',backdrop:'static'})

    modalRef.componentInstance.sendmessages = sendmessages
    modalRef.result.then(()=>{
      //this.getMessage()
    })
  }

  getGroup(Organization: string): void {
    //this.isFirstSelectOptionSelected = true;
    this.msgService.getMessageGroups(Organization).subscribe({
      next: (res) => {
        this.messagegroups = res;
        this.groupOptions = this.messagegroups.map(group => ({
          value: group.id,
          label: group.groupName
        }));
        console.log(this.messagegroups);
      },
      error: (err) => {
        console.log(err);
      }
    });
  }

}
