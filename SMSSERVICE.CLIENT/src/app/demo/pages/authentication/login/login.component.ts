import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserService } from 'src/app/services/user.service';
import { MessageService } from 'primeng/api';
import {ButtonModule} from 'primeng/button'
import { UserView } from 'src/models/auth/userDto';
import { HttpClientModule } from '@angular/common/http';
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule,ReactiveFormsModule,HttpClientModule,ButtonModule],
  providers:[],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export default class LoginComponent implements OnInit {

  loginForm !: FormGroup
  user!: UserView
  constructor(
    private formBuilder: FormBuilder, 
    private router: Router, 
    private userService: UserService, 
    
  private messageService : MessageService) { }

  ngOnInit(): void {

    this.loginForm = this.formBuilder.group({

      userName: ['', Validators.required],
      password: ['', Validators.required]

    });
  }

  login() {
    if (this.loginForm.valid) {
      this.userService.login(this.loginForm.value).subscribe({
        next: (res) => {

          if (res.success) {
            this.messageService.add({ severity: 'success', summary: 'Successfull', detail: res.message });              
          
            sessionStorage.setItem('token', res.data);
            this.router.navigateByUrl('/');
          }
          else {

            this.messageService.add({ severity: 'error', summary: 'Authentication failed.', detail: res.message });
           
          }

        },
        error: (err) => {
          this.messageService.add({ severity: 'error', summary: 'Something went wrong!!!', detail: err.message });              
          
          console.log(err)

        }
      })
    }
  }
}