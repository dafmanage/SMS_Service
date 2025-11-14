import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DeliveryStatusComponent } from './delivery-status/delivery-status.component';
import { SmsReportsComponent } from './sms-reports/sms-reports.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        redirectTo: 'sms-reports',
        pathMatch: 'full'
      },
      {
        path: 'sms-reports',
        component: SmsReportsComponent
      },
      {
        path: 'delivery-status',
        component: DeliveryStatusComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReportRoutingModule { }
