import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SystemControlRoutingModule } from './system-control-routing.module';
import { MeterSizeComponent } from './scs-data/meter/meter-size/meter-size.component';
import ScsDataComponent from './scs-data/scs-data.component';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { TabViewModule } from 'primeng/tabview';
import { MeterConfigComponent } from './scs-data/meter/meter-config.component';
import { GeneralSettingsComponent } from './general-settings/general-settings.component';
import { SmsConfigurationComponent } from './sms-configuration/sms-configuration.component';
import { SecuritySettingsComponent } from './security-settings/security-settings.component';

@NgModule({
  declarations: [
    MeterSizeComponent,
    ScsDataComponent,
    MeterConfigComponent,
    GeneralSettingsComponent,
    SmsConfigurationComponent,
    SecuritySettingsComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SystemControlRoutingModule,
    SharedModule,
    TableModule,
    TabViewModule
  ]
})
export class SystemControlModule { }
