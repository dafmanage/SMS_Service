import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService } from 'primeng/api';
import { CommonService } from 'src/app/services/common.service';
import { OrganizationService } from 'src/app/services/organization.service';
//import { HrmService } from 'src/app/services/hrm.service';
import { UserService } from 'src/app/services/user.service';
import { UserView } from 'src/models/auth/userDto';
import { IOrganizationGetDto } from 'src/models/hrm/IEmployeeDto';

@Component({
  selector: 'app-update-organization',
  templateUrl: './update-organization.component.html',
  styleUrls: ['./update-organization.component.scss']
})
export class UpdateOrganizationComponent implements OnInit {

// @Input() employee: IEmployeeGetDto
@Input() organization: IOrganizationGetDto


  imagePath: any
  fileGH:File
  user !: UserView

  selectedState: any = null;

  OrganizationForm!: FormGroup;
  organizationStatusDropDownItems = [
    { name: 'ACTIVE', code: 'ACTIVE'},
    { name: 'INACTIVE', code: 'INACTIVE'}
  ]

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private commonService:CommonService,
    private messageService: MessageService,
    private activeModal : NgbActiveModal,
    private orgService: OrganizationService
    //private hrmService: HrmService
    ) { }

  ngOnInit(): void {

    this.user = this.userService.getCurrentUser()


    this.OrganizationForm = this.formBuilder.group({
      organizationName: [this.organization.name, Validators.required],
      nameLocal: [this.organization.nameLocal, Validators.required],
      // Gender: [this.organization.gender, Validators.required],
      phoneNumber: [this.organization.phoneNumber, [Validators.required,Validators.pattern(/^[1-9]+[0-9]*$/)]],
      // BirthDate: [this.organization.birthDate, Validators.required],
      email: [this.organization.email, [Validators.required,Validators.email]],
      address: [this.organization.address, Validators.required],
      organizationStatus: [this.organization.organizationStatus, Validators.required],

    })
    //this.OrganizationForm.controls['FirstName'].setValue(this.organization.FirstName)
  }

  onSubmit() {

    console.log(this.OrganizationForm.value)
    if (this.OrganizationForm.valid) {

      const formData = new FormData();
      formData.append('Id', this.organization.id);
      formData.append("Name", this.OrganizationForm.value.organizationName);
      // formData.append("LastName", this.OrganizationForm.value.LastName);
      // formData.append("Gender", this.OrganizationForm.value.Gender);
      formData.append("PhoneNumber", this.OrganizationForm.value.phoneNumber);
      // formData.append("BirthDate", this.OrganizationForm.value.BirthDate);
      formData.append("Email", this.OrganizationForm.value.email);
      formData.append("Address", this.OrganizationForm.value.address);
      // formData.append("EmploymentDate", this.OrganizationForm.value.EmploymentDate);
      // formData.append("EmploymentPosition", this.OrganizationForm.value.EmploymentPosition);
      formData.append("Image", this.fileGH);
      formData.append("CreatedById", this.user.userId);
      formData.append("NameLocal", this.OrganizationForm.value.nameLocal);
      formData.append("OrganizationStatus", this.OrganizationForm.value.organizationStatus);


      // this.hrmService.updateEmployee(formData).subscribe({
      //   next: (res) => {

      //     if (res.success) {
      //       this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });

      //       this.OrganizationForm.reset();

      //       this.closeModal()


      //     }
      //     else {
      //       this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: res.message });

      //     }

      //   }, error: (err) => {
      //     this.messageService.add({ severity: 'error', summary: 'Something went Wrong', detail: err });
      //   }
      // })
      this.orgService.updateOrganization(formData).subscribe({
        next: (res) => {

          if (res.success) {
            this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });

            this.OrganizationForm.reset();

            this.closeModal()


          }
          else {
            // Handle validation errors properly
            let errorMessage = res.message;
            if (res.data && Array.isArray(res.data)) {
              errorMessage = res.data.join(', ');
            }
            this.messageService.add({ severity: 'error', summary: 'Validation Error', detail: errorMessage });

          }

        }, error: (err) => {
          console.error('Error:', err);
          let errorMessage = 'An unexpected error occurred';
          
          // Handle validation errors from backend
          if (err.error && err.error.message) {
            errorMessage = err.error.message;
            if (err.error.data && Array.isArray(err.error.data)) {
              errorMessage = err.error.data.join(', ');
            }
          } else if (err.message) {
            errorMessage = err.message;
          } else if (err.status === 400) {
            errorMessage = 'Validation failed. Please check your input.';
          } else if (err.status === 0) {
            errorMessage = 'Unable to connect to server. Please check your internet connection.';
          }
          
          this.messageService.add({ severity: 'error', summary: 'Validation Error', detail: errorMessage });
        }
      })



    }
    else {
      this.messageService.add({ severity: 'error', summary: 'Form Submit failed.', detail: "Please fil required inputs !!" });
    }


  }
  onUpload(event: any) {
    const file: File = event.target.files[0];
    const allowedTypes = ['image/jpeg', 'image/png', 'image/svg+xml'];
    const maxSizeInMB = 5;
    const maxSizeInBytes = maxSizeInMB * 1024 * 1024;

    if (!allowedTypes.includes(file.type)) {
      this.messageService.add({ severity: 'error', summary: 'Invalid file type.', detail: 'Only JPEG, PNG, and sanitized SVG files are allowed.' });
      return;
    }

    if (file.size > maxSizeInBytes) {
      this.messageService.add({ severity: 'error', summary: 'File too large.', detail: `Maximum file size is ${maxSizeInMB}MB.` });
      return;
    }

    this.fileGH = file;
    const reader = new FileReader();
    reader.onloadend = (e) => {
      this.imagePath = reader.result;
    };
    reader.readAsDataURL(file);
  }

  getImage(url:string){
    return this.commonService.createImgPath(url)
  }

  getImage2() {


    if (this.imagePath != null) {
      return this.imagePath
    }
    if (this.organization.imagePath != "") {

      return this.getImage(this.organization.imagePath!)
    }
    else {
      return 'assets/images/profile.jpg'
    }
  }

  closeModal(){

    this.activeModal.close()
  }
}
