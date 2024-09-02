import { Component, Input } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService, SelectItem } from 'primeng/api';
import { MessageGroupService } from 'src/app/services/message-group.service';
import { OrganizationService } from 'src/app/services/organization.service';
import { ReportService } from 'src/app/services/report.service';
import { SendMessageService } from 'src/app/services/send-message.service';
import { UserService } from 'src/app/services/user.service';
import { UserView } from 'src/models/auth/userDto';
import { IMessagGroupGetDto } from 'src/models/msg/msg.model';
import { IReportGetDto } from 'src/models/report/report.model';

@Component({
  selector: 'app-report',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.scss']
})
export class ReportComponent {
  reports:IReportGetDto[]
  user !: UserView
  messagegroups:IMessagGroupGetDto[]
  groupOptions: SelectItem[] = []
  selectedGroup: string = null;
  selectedOrganization: string = null;
  organizationSelectList: SelectItem[] = []
  isFirstSelectOptionSelected = false;


  constructor(private userService:UserService,
    private msgService: MessageGroupService,
    private sndMessage: SendMessageService,
    private reportService:ReportService,
    private orgService: OrganizationService,){}

  ngOnInit(): void {
    this.user= this.userService.getCurrentUser()

    if(this.allowedRoles(['Admin'])){this.getOrganizationsSelectList()}
    if(this.allowedRoles(['Organization'])){this.getGroup(this.user.organizationId)}
  }

  getGroup(Organization: string): void {
    this.isFirstSelectOptionSelected = true;
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
  getReport(id:string){

    this.reportService.getReport(id).subscribe({
      next: (res) => {

        this.reports = res
        console.log(this.reports)

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
  getOrganizationsSelectList(){
    this.orgService.getOrganizationsSelectList().subscribe({
      next: (res) => {
        this.organizationSelectList = res.map(item => ({ value: item.id, label: item.name}));
      }
    })
  }
}
