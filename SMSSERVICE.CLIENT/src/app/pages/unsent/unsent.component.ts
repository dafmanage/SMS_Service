import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { UnsentService } from 'src/app/services/unsent.service';
import { UserService } from 'src/app/services/user.service';
import { UserView } from 'src/models/auth/userDto';
import { IUnsentGetDto } from 'src/models/unsent/unsent.model';
import { RejectComponent } from './reject/reject.component';

@Component({
  selector: 'app-unsent',
  templateUrl: './unsent.component.html',
  styleUrls: ['./unsent.component.scss']
})
export class UnsentComponent implements OnInit{
  unsentmessages: IUnsentGetDto[]
  user !: UserView

  constructor(private userService:UserService,
    private usntService:UnsentService,
    private modalService:NgbModal,){}

  ngOnInit(): void {
    this.user= this.userService.getCurrentUser()

    if(this.allowedRoles(['Admin'])){this.getUnsentMessages()}
    if(this.allowedRoles(['Organization'])){this.getUnsentMessage(this.user.organizationId)}
  }

  getUnsentMessages() {

    this.usntService.getUnsentMessages().subscribe({
      next: (res) => {

        this.unsentmessages = res
        console.log(this.unsentmessages)

      },
      error: (err) => {
        console.log(err)
      }
    })
  }
  getUnsentMessage(id:string ) {

    this.usntService.getUnsentMessage(id).subscribe({
      next: (res) => {

        this.unsentmessages = res
        console.log(this.unsentmessages)

      },
      error: (err) => {
        console.log(err)
      }
    })
  }
  allowedRoles(allowedRoles: any)
  {
    return this.userService.roleMatch(allowedRoles)
  }
  rejectMessage(){
    let modalRef = this.modalService.open(RejectComponent, { size: 'lg', backdrop: 'static' })

    modalRef.result.then(() => {
    })
  }
}
