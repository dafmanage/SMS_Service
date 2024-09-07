// Angular import
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Subscription } from 'rxjs';
import { AuthGuard } from 'src/app/auth/auth.guard';
import { UserService } from 'src/app/services/user.service';
import { UserView } from 'src/models/auth/userDto';

@Component({
  selector: 'app-nav-right',
  templateUrl: './nav-right.component.html',
  styleUrls: ['./nav-right.component.scss']
})
export class NavRightComponent implements OnInit {
  currentUser:UserView
  private forceLogoutSubscription: Subscription;
  hubConnection:any

  ngOnInit(): void {
    this.currentUser = this.userService.getCurrentUser();

    var token = sessionStorage.getItem('token')

    this.userService.initializeSignalRConnection(token)
    .then(() => {
      this.hubConnection.on('ForceLogout', () => {
        this.handleForceLogout();
      });
    })
    .catch(err => {
      console.error('Failed to initialize SignalR connection:', err);
    });

  
  }
 
  constructor( private authGuard: AuthGuard,private userService : UserService,private messageService : MessageService,
    private router :Router
  ){

    var token = sessionStorage.getItem('token')

    this.userService.initializeSignalRConnection(token)
    .then(() => {
      this.hubConnection.on('ForceLogout', () => {
        this.handleForceLogout();
      });
    })
    .catch(err => {
      console.error('Failed to initialize SignalR connection:', err);
    });
  }

  menuVisible = false;

  toggleMenu() {
      this.menuVisible = !this.menuVisible;
  } 

  
   handleForceLogout() {

    this.userService.logout().subscribe({

      next:(res)=>{
        if(res.success){
          this.messageService.add({
            severity: 'info',
            summary: 'Logged Out',
            detail: 'You have been logged out from this device.'
          });
          this.router.navigateByUrl('/auth/login');
        }
      }
    })
    
  }
}
