import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { ReportRoutingModule } from './report-routing.module';
import { DeliveryStatusComponent } from './delivery-status/delivery-status.component';
import { SmsReportsComponent } from './sms-reports/sms-reports.component';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';

@NgModule({
  declarations: [
    DeliveryStatusComponent,
    SmsReportsComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ReportRoutingModule,
    SharedModule,
    TableModule
  ]
})
export class ReportModule { }
