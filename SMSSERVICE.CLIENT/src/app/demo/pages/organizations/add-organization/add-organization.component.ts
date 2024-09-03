import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MessageService } from 'primeng/api';
import { OrganizationService } from 'src/app/services/organization.service';
import { UserService } from 'src/app/services/user.service';
import { UserView } from 'src/models/auth/userDto';

@Component({
  selector: 'app-add-organization',
  templateUrl: './add-organization.component.html',
  styleUrls: ['./add-organization.component.scss']
})
export class AddOrganizationComponent implements OnInit {


  imagePath: any
  fileGH!: File
  user !: UserView

  selectedState: any = null;

  OrganizationForm!: FormGroup;
  organizationStatusDropDownItems = [
    { name: 'ACTIVE', code: 'ACTIVE' },
    { name: 'INACTIVE', code: 'INACTIVE' }
  ]

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private messageService: MessageService,
    private activeModal: NgbActiveModal,
    private orgService: OrganizationService,
  ) { }

  ngOnInit(): void {

    this.user = this.userService.getCurrentUser()

    this.OrganizationForm = this.formBuilder.group({
      organizationName: [null, Validators.required],
      phoneNumber: [null, Validators.required],
      email: [null, Validators.required],
      address: ['', Validators.required],
      nameLocal: ['', Validators.required],
      organizationStatus: ['', Validators.required]

    });
  }

  onSubmit() {

    console.log(this.OrganizationForm.value)
    if (this.OrganizationForm.valid) {

      const formData = new FormData();
      formData.append("Name", this.OrganizationForm.value.organizationName);
      formData.append("PhoneNumber", this.OrganizationForm.value.phoneNumber);
      formData.append("Email", this.OrganizationForm.value.email);
      formData.append("Address", this.OrganizationForm.value.address);
      formData.append("Image", this.fileGH);
      formData.append("CreatedById", this.user.userId);
      formData.append("NameLocal", this.OrganizationForm.value.nameLocal);
      formData.append("OrganizationStatus", this.OrganizationForm.value.organizationStatus);
      console.log(formData, this.fileGH);


      this.orgService.addOrganiztion(formData).subscribe({
        next: (res) => {

          if (res.success) {
            this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });

            this.OrganizationForm.reset();

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
      this.messageService.add({ severity: 'error', summary: 'Form Submit failed.', detail: "Please fill required inputs !!" });
    }


  }
  // onUpload(event: any) {

  //   var file: File = event.target.files[0];
  //   this.fileGH = file
  //   var myReader: FileReader = new FileReader();
  //   myReader.onloadend = (e) => {
  //     this.imagePath = myReader.result;
  //   }
  //   myReader.readAsDataURL(file);
  // }
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



  closeModal() {

    this.activeModal.close()
  }
}
