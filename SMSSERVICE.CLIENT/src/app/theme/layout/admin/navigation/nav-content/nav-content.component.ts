// Angular import
import { Component, EventEmitter, NgZone, OnInit, Output } from '@angular/core';
import { Location, LocationStrategy } from '@angular/common';
import { environment } from 'src/environments/environment';

// project import
import { NavigationService } from '../navigation';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-nav-content',
  templateUrl: './nav-content.component.html',
  styleUrls: ['./nav-content.component.scss']
})
export class NavContentComponent implements OnInit {
  // public props
  @Output() NavCollapsedMob: EventEmitter<any> = new EventEmitter();

  // version
  currentApplicationVersion = environment.appVersion;

  navigation: any;
  windowWidth = window.innerWidth;

  // Constructor
  constructor(
    private navigationService: NavigationService,
    private zone: NgZone,
    private location: Location,
    private locationStrategy: LocationStrategy,
    private userService: UserService
  ) {
    this.filterNavigationByRole();
  }

  // Life cycle events
  ngOnInit() {
    if (this.windowWidth < 1025) {
      (document.querySelector('.coded-navbar') as HTMLDivElement).classList.add('menupos-static');
    }
  }

  fireOutClick() {
    let current_url = this.location.path();
    const baseHref = this.locationStrategy.getBaseHref();
    if (baseHref) {
      current_url = baseHref + this.location.path();
    }
    const link = "a.nav-link[ href='" + current_url + "' ]";
    const ele = document.querySelector(link);
    if (ele !== null && ele !== undefined) {
      const parent = ele.parentElement;
      const up_parent = parent?.parentElement?.parentElement;
      const last_parent = up_parent?.parentElement;
      if (parent?.classList.contains('coded-hasmenu')) {
        parent.classList.add('coded-trigger');
        parent.classList.add('active');
      } else if (up_parent?.classList.contains('coded-hasmenu')) {
        up_parent.classList.add('coded-trigger');
        parent.classList.add('active');
      } else if (last_parent?.classList.contains('coded-hasmenu')) {
        last_parent.classList.add('coded-trigger');
        last_parent.classList.add('active');
      }
    }
  }

  navMob() {
    if (this.windowWidth < 1025 && document.querySelector('app-navigation.coded-navbar').classList.contains('mob-open')) {
      this.NavCollapsedMob.emit();
    }
  }

  filterNavigationByRole() {
    const currentUser = this.userService.getCurrentUser();
    const allNavigation = this.navigationService.get();
    
    console.log('DEBUG: NavContentComponent - Current user:', currentUser);
    console.log('DEBUG: NavContentComponent - All navigation items:', allNavigation);
    
    this.navigation = allNavigation.filter((item: any) => {
      console.log('DEBUG: NavContentComponent - Checking item:', item.id, 'roleMatch:', !!item.roleMatch);
      
      // If no roleMatch function, show the item
      if (!item.roleMatch) {
        console.log('DEBUG: NavContentComponent - No roleMatch for item:', item.id, '- showing');
        return true;
      }
      
      // If roleMatch function exists, check if user matches
      const matches = item.roleMatch(currentUser);
      console.log('DEBUG: NavContentComponent - RoleMatch result for item:', item.id, ':', matches);
      return matches;
    }).map((item: any) => {
      // Recursively filter children
      if (item.children) {
        console.log('DEBUG: NavContentComponent - Filtering children for item:', item.id);
        item.children = item.children.filter((child: any) => {
          console.log('DEBUG: NavContentComponent - Checking child:', child.id, 'roleMatch:', !!child.roleMatch);
          
          if (!child.roleMatch) {
            console.log('DEBUG: NavContentComponent - No roleMatch for child:', child.id, '- showing');
            return true;
          }
          
          const childMatches = child.roleMatch(currentUser);
          console.log('DEBUG: NavContentComponent - RoleMatch result for child:', child.id, ':', childMatches);
          return childMatches;
        }).map((child: any) => {
          // Filter nested children
          if (child.children) {
            child.children = child.children.filter((nestedChild: any) => {
              if (!nestedChild.roleMatch) {
                return true;
              }
              return nestedChild.roleMatch(currentUser);
            });
          }
          return child;
        });
      }
      return item;
    });
    
    console.log('DEBUG: NavContentComponent - Final filtered navigation:', this.navigation);
  }
}
