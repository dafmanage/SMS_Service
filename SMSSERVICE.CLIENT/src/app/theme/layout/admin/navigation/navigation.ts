import { Injectable } from '@angular/core';

export interface NavigationItem {
  id: string;
  title: string;
  type: string;
  icon?: string;
  classes?: string;
  url?: string;
  breadcrumbs?: boolean;
  children?: NavigationItem[];
  roleMatch?: Function;
  external?: boolean;
  target?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NavigationService {
  get(): NavigationItem[] {
    return NavigationItems;
  }
}

const NavigationItems: NavigationItem[] = [
  {
    id: 'dashboard',
    title: 'Dashboard',
    type: 'group',
    icon: 'icon-navigation',
    children: [
      {
        id: 'default',
        title: 'A2P SMS Overview',
        type: 'item',
        classes: 'nav-item',
        url: '/default',
        icon: 'ti ti-dashboard',
        breadcrumbs: false
      }
    ]
  },
  {
    id: 'sms-service',
    title: 'SMS Service Management',
    type: 'group',
    icon: 'icon-navigation',
    children: [
      {
        id: 'message-management',
        title: 'Message Management',
        type: 'collapse',
        icon: 'ti ti-message',
        children: [
          {
            id: 'message-groups',
            title: 'Message Groups',
            type: 'item',
            url: '/message',
            breadcrumbs: false,
            roleMatch: (user: any) => user && user.role && (user.role.includes('Admin') || user.role.includes('SuperAdmin'))
          },
          {
            id: 'phone-numbers',
            title: 'Phone Numbers',
            type: 'item',
            url: '/phone',
            breadcrumbs: false,
            roleMatch: (user: any) => user && user.role && (user.role.includes('Admin') || user.role.includes('SuperAdmin'))
          },
          {
            id: 'send-message',
            title: 'Send SMS',
            type: 'item',
            url: '/send-message',
            breadcrumbs: false,
            roleMatch: (user: any) => user && user.role && user.role.includes('Admin')
          },
          {
            id: 'pending-messages',
            title: 'Pending Messages',
            type: 'item',
            url: '/unsent',
            breadcrumbs: false,
            roleMatch: (user: any) => user && user.role && user.role.includes('Admin')
          },
          {
            id: 'message-approval',
            title: 'Message Approval',
            type: 'item',
            url: '/message-approval',
            breadcrumbs: false,
            roleMatch: (user: any) => user && user.role && user.role.includes('SuperAdmin')
          },
        ]
      },
      {
        id: 'analytics',
        title: 'Analytics & Reports',
        type: 'collapse',
        icon: 'ti ti-chart-bar',
        children: [
          {
            id: 'sms-reports',
            title: 'SMS Reports',
            type: 'item',
            url: '/report/sms-reports',
            breadcrumbs: false,
            roleMatch: (user: any) => user && user.role && (user.role.includes('Admin') || user.role.includes('SuperAdmin'))
          },
          {
            id: 'delivery-status',
            title: 'Delivery Status',
            type: 'item',
            url: '/report/delivery-status',
            breadcrumbs: false,
            roleMatch: (user: any) => user && user.role && (user.role.includes('Admin') || user.role.includes('SuperAdmin'))
          }
        ]
      }
    ]
  },
  {
    id: 'organization',
    title: 'Organization Management',
    type: 'group',
    icon: 'icon-navigation',
    roleMatch: (user: any) => user && user.role && user.role.includes('SuperAdmin'),
    children: [
      {
        id: 'organizations',
        title: 'Organizations',
        type: 'item',
        url: '/organizations',
        icon: 'ti ti-building',
        breadcrumbs: false
      },
      {
        id: 'users',
        title: 'User Management',
        type: 'item',
        url: '/users',
        icon: 'ti ti-users',
        breadcrumbs: false
      }
    ]
  },
  {
    id: 'system',
    title: 'System Configuration',
    type: 'group',
    icon: 'icon-navigation',
    roleMatch: (user: any) => user && user.role && user.role.includes('SuperAdmin'),
    children: [
      {
        id: 'system-control',
        title: 'System Control',
        type: 'collapse',
        icon: 'ti ti-settings',
        children: [
          {
            id: 'general-settings',
            title: 'General Settings',
            type: 'item',
            url: '/system-control/general-settings',
            breadcrumbs: false
          },
          {
            id: 'sms-settings',
            title: 'SMS Configuration',
            type: 'item',
            url: '/system-control/sms-configuration',
            breadcrumbs: false
          },
          {
            id: 'security-settings',
            title: 'Security Settings',
            type: 'item',
            url: '/system-control/security-settings',
            breadcrumbs: false
          }
        ]
      }
    ]
  }
];

export default NavigationItems;
