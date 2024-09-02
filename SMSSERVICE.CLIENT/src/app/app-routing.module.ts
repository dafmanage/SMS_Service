import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminComponent } from './theme/layout/admin/admin.component';
import { GuestComponent } from './theme/layout/guest/guest.component';
import { AuthGuard } from './auth/auth.guard';
import { MessageGroupComponent } from './pages/message-group/message-group/message-group.component';
import { GroupPhonesComponent } from './pages/group-phones/group-phones.component';
import { SendMessageComponent } from './pages/send-message/send-message.component';
import { UnsentComponent } from './pages/unsent/unsent.component';
import { ReportComponent } from './pages/report/report/report.component';

const routes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    component: AdminComponent,
    children: [
      {
        path: '',
        redirectTo: '/default',
        pathMatch: 'full'
      },
      {
        path: 'default',
        loadComponent: () => import('./demo/default/default.component')
      },
      {
        path: 'typography',
        loadComponent: () => import('./demo/elements/typography/typography.component')
      },
      {
        path: 'color',
        loadComponent: () => import('./demo/elements/element-color/element-color.component')
      },
      {
        path: 'sample-page',
        loadComponent: () => import('./demo/sample-page/sample-page.component')
      },
      {
        path: 'organizations',
        loadComponent: () => import('./demo/pages/organizations/organizations.component')

      },
      {
        path: 'users',
        loadComponent: () => import('./demo/pages/users/users.component')

      },

      {
        path: 'system-control',
        loadChildren: () => import('./demo/pages/system-control/system-control.module').then((m) => m.SystemControlModule)


      },
      {
        path: 'message', component:MessageGroupComponent
      },
      {
        path: 'phone', component:GroupPhonesComponent
      },
      {
        path: 'send-message', component:SendMessageComponent
      },
      {
        path: 'unsent', component:UnsentComponent
      },
      {
        path: 'report', component:ReportComponent
      }

    ]
  },
  {
    path: '',
    component: GuestComponent,
    children: [
      {
        path: 'auth',
        loadChildren: () => import('./demo/pages/authentication/authentication.module').then((m) => m.AuthenticationModule)
      }
    ]
  },




];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
