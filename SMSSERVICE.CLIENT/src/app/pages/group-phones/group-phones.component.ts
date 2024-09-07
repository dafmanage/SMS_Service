import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { GroupPhonesService } from 'src/app/services/group-phones.service';
import { UserService } from 'src/app/services/user.service';
import { AddPhoneComponent } from './add-phone/add-phone.component';
import { UserView } from 'src/models/auth/userDto';
import { IPhoneGroupGetDto } from 'src/models/phn/phn.model';
import { MessageService, SelectItem } from 'primeng/api';
import { OrganizationService } from 'src/app/services/organization.service';
import { MessageGroupService } from 'src/app/services/message-group.service';
import { IMessagGroupGetDto } from 'src/models/msg/msg.model';
import * as XLSX from 'xlsx';
import { UpdatePhonesComponent } from './update-phones/update-phones/update-phones.component';


@Component({
  selector: 'app-group-phones',
  templateUrl: './group-phones.component.html',
  styleUrls: ['./group-phones.component.scss']
})
export class GroupPhonesComponent implements OnInit{
  errorData: any[] = []
  excelPath: any
  fileGH!:File
  isFirstSelectOptionSelected = false;
  selectedOrganization: string = null;
  selectedGroup: string = null;
  organizationSelectList: SelectItem[] = []
  messagegroups:IMessagGroupGetDto[]
  groupOptions: SelectItem[] = []
  user !: UserView
  groupphones:IPhoneGroupGetDto[]

  @ViewChild('fileInput', { static: false }) fileInput!: ElementRef;
  activeModal: any;

  constructor(private modalService:NgbModal,
    private messageService:MessageService,
    private phnService:GroupPhonesService,
    private userService:UserService,
    private orgService: OrganizationService,
    private msgService: MessageGroupService){}
  ngOnInit(): void {
    this.user= this.userService.getCurrentUser()

    if(this.allowedRoles(['Admin'])){this.getOrganizationsSelectList()}
    if(this.allowedRoles(['Organization'])){this.getGroup(this.user.organizationId)}
  }
  openFilePicker() {
    this.fileInput.nativeElement.click();
  }
  addGroup(){
    let modalRef = this.modalService.open(AddPhoneComponent, { size: 'lg', backdrop: 'static' })

    modalRef.result.then(() => {

    })
  }
  getGroupPhonez(id: string){

    this.phnService.getGroupPhones(id).subscribe({
      next: (res) => {
        this.groupphones = res

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

  getGroup(selectedOrganization: string): void {
    this.isFirstSelectOptionSelected = true;
    this.msgService.getMessageGroups(selectedOrganization).subscribe({
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
  updateGroupPhone(groupphone: IPhoneGroupGetDto){
    console.log(groupphone)

    let modalRef = this.modalService.open(UpdatePhonesComponent,{size:'lg',backdrop:'static'})

    modalRef.componentInstance.groupphones = groupphone
    modalRef.result.then(()=>{
      this.getGroupPhonez(groupphone.messageGroupId)
    })
  }

  onUpload(event: any) {

    var file: File = event.target.files[0];
    this.fileGH = file
    var myReader: FileReader = new FileReader();
    myReader.onloadend = (e) => {
      this.excelPath = myReader.result;
    }
    myReader.readAsDataURL(file);


    const formData = new FormData();

      formData.append("ExcelFile", this.fileGH);
      console.log(formData,this.fileGH);


      this.phnService.addGroupPhoneExcel(formData,this.user.userId).subscribe({
        next: (res) => {

          if (res.success) {
            this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });
            this.messageService.add({ severity: 'info', summary: 'Invalid Phone Numbers', detail: 'Check Your Download Folder for Invalid Phone Numbers' , sticky:true});
            this.errorData = res.data
            this.createExcelFile()
            console.log(this.errorData)



          }
          else {
            this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: res.message });

          }

        }, error: (err) => {
          this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: err });
        }
      })


  }
  closeModal(){
    this.activeModal.close()
  }




  createExcelFile() {
    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(this.errorData.map(item => ({
      FullName: item.fullName,
      PhoneNumber: item.phoneNumber,
      MessageGroup: item.messageGrpup,
      ErrorMessage: item.errorMessage,

    })));


    worksheet['E1'] = { t: 's', v: 'Notice: If you want to re-submit this file, remove the Notice & Error Message columns and the row that holds the titles.' };

    const workbook: XLSX.WorkBook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };

    const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });

    this.saveExcelFile(excelBuffer, 'data.xlsx');
  }

  saveExcelFile(buffer: any, fileName: string) {
    const data: Blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    console.log( "data",data)
    if (window.navigator && 'msSaveBlob' in window.navigator) {
      // For Internet Explorer
      (window.navigator as any).msSaveBlob(data, fileName);
    } else {
      // For other browsers
      const downloadLink = document.createElement('a');
      downloadLink.href = window.URL.createObjectURL(data);
      downloadLink.setAttribute('download', fileName);
      document.body.appendChild(downloadLink);
      downloadLink.click();
      document.body.removeChild(downloadLink);
    }
  }
}
