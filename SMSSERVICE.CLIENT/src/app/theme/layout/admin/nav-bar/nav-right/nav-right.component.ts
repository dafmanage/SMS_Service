// Angular import
import { Component, OnInit } from '@angular/core';
import { AuthGuard } from 'src/app/auth/auth.guard';

@Component({
  selector: 'app-nav-right',
  templateUrl: './nav-right.component.html',
  styleUrls: ['./nav-right.component.scss']
})
export class NavRightComponent implements OnInit {

  ngOnInit(): void {
    
  }

  constructor( private authGuard: AuthGuard,){

  }

  
  logOut() {

    this.authGuard.logout();
  }
}
