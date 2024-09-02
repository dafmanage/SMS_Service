import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { SharedModule } from 'primeng/api';
import { Table, TableModule } from 'primeng/table';
import { CommonService } from 'src/app/services/common.service';
import { IOrganizationGetDto } from 'src/models/hrm/IEmployeeDto';
import { AddOrganizationComponent } from './add-organization/add-organization.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { UpdateOrganizationComponent } from './update-organization/update-organization.component';
import { OrganizationService } from 'src/app/services/organization.service';

@Component({
  selector: 'app-organizations',
  standalone: true,
  imports: [CommonModule, SharedModule,TableModule],
  templateUrl: './organizations.component.html',
  styleUrls: ['./organizations.component.scss']
})
export default class OrganizationsComponent implements OnInit{


  organizations :IOrganizationGetDto[]



  constructor(
    private commonService : CommonService,
    private modalService:NgbModal,
    private orgService:OrganizationService

  ){}

  ngOnInit(): void {

    this.getOrganizations()
  }

  getOrganizations() {

    this.orgService.getOrganizations().subscribe({
      next: (res) => {

        this.organizations = res
        console.log(this.organizations)

      },
      error: (err) => {
        console.log(err)
      }
    })
  }


  addOrganization() {

    let modalRef = this.modalService.open(AddOrganizationComponent, { size: 'lg', backdrop: 'static' })

    modalRef.result.then(() => {
      this.getOrganizations()
    })
  }
  updateOrganization(organization: IOrganizationGetDto){
    console.log(organization)

    let modalRef = this.modalService.open(UpdateOrganizationComponent,{size:'lg',backdrop:'static'})

    modalRef.componentInstance.organization = organization
    modalRef.result.then(()=>{
      this.getOrganizations()
    })
  }



  getImagePath(url:string){
     return this.commonService.createImgPath(url)
  }
  onGlobalFilter(table: Table, event: Event) {
    table.filterGlobal((event.target as HTMLInputElement).value, 'contains');
  }



}













