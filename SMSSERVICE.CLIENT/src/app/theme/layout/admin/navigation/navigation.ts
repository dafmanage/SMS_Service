import { Injectable } from '@angular/core';
import { UserService } from 'src/app/services/user.service';

export interface NavigationItem {
  id: string;
  title: string;
  type: 'item' | 'collapse' | 'group';
  icon?: string;
  url?: string;
  classes?: string;
  external?: boolean;
  target?: boolean;
  breadcrumbs?: boolean;
  children?: Navigation[];
  roleMatch?: Function
}

export interface Navigation extends NavigationItem {
  children?: NavigationItem[];

}
// @Injectable()
// export class NavigationService {
//   constructor(private userService:UserService){}
//   roleMatch(value:string[]){
//     return this.userService.roleMatch(value)
// }
// }


const NavigationItems = [
  {
    id: 'dashboard',
    title: 'Dashboard',
    type: 'group',
    icon: 'icon-navigation',
    children: [
      {
        id: 'default',
        title: 'Default',
        type: 'item',
        classes: 'nav-item',
        url: '/default',
        icon: 'ti ti-dashboard',
        breadcrumbs: false
      }
    ]
  },
  {
    id: 'page2',
    title: 'SMS Service',
    type: 'group',
    icon: 'icon-navigation',
    children: [
      {
        id: 'Authentication',
        title: 'Message Service',
        type: 'collapse',
        icon: 'ti ti-message',
        children: [
          {
            id: 'data',
            title: 'Message Group',
            type: 'item',
            url: '/message',
            breadcrumbs: false
          },



          {
            id: 'data',
            title: 'Add Phones',
            type: 'item',

            url: '/phone',

            breadcrumbs: false
          },

          {
            id: 'setup',
            title: 'Send Message',
            type: 'item',
            url: '/send-message',
            breadcrumbs: false
          },
          {
            id: 'data',
            title: 'Pending Message',
            type: 'item',
            url: '/unsent',
            breadcrumbs: false
          },

          {
            id: 'setting',
            title: 'Report',
            type: 'item',
            url: '/report',
            breadcrumbs: false
          }
        ]
      }
    ]
  },
  {
    id: 'page',
    title: 'Configuration',
    type: 'group',
    icon: 'icon-navigation',


    children: [
      {
        id: 'Authentication',
        title: 'System Users',
        type: 'collapse',
        icon: 'ti ti-user',

        children: [
          {
            id: 'organization',
            title: 'Organizations',
            type: 'item',
            url: '/organizations',
            breadcrumbs: false
          },
          {
            id: 'user',
            title: 'Users',
            type: 'item',
            url: '/users',

            breadcrumbs: false
          }
        ]
      }
    ]
  },


];


@Injectable()
export class NavigationItem {
  get() {
    return NavigationItems;
  }
}
