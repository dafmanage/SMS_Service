import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import ScsDataComponent from './scs-data/scs-data.component';
import { GeneralSettingsComponent } from './general-settings/general-settings.component';
import { SmsConfigurationComponent } from './sms-configuration/sms-configuration.component';
import { SecuritySettingsComponent } from './security-settings/security-settings.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'data',
        component: ScsDataComponent
      },
      {
        path: 'general-settings',
        component: GeneralSettingsComponent
      },
      {
        path: 'sms-configuration',
        component: SmsConfigurationComponent
      },
      {
        path: 'security-settings',
        component: SecuritySettingsComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SystemControlRoutingModule { }
