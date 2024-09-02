import { Component } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageGroupService } from 'src/app/services/message-group.service';

@Component({
  selector: 'app-reject',
  templateUrl: './reject.component.html',
  styleUrls: ['./reject.component.scss']
})
export class RejectComponent {
  rejectForm:FormGroup;
  constructor(private activeModal : NgbActiveModal,
    private msgService: MessageGroupService
    ) { }

  onSubmit(){}

  closeModal(){

    this.activeModal.close()
  }

}
