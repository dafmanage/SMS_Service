import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AdminComponent } from './theme/layout/admin/admin.component';
import { NavigationItem } from './theme/layout/admin/navigation/navigation';
import { NavBarComponent } from './theme/layout/admin/nav-bar/nav-bar.component';
import { NavLeftComponent } from './theme/layout/admin/nav-bar/nav-left/nav-left.component';
import { NavRightComponent } from './theme/layout/admin/nav-bar/nav-right/nav-right.component';
import { NavigationComponent } from './theme/layout/admin/navigation/navigation.component';
import { NavLogoComponent } from './theme/layout/admin/nav-bar/nav-logo/nav-logo.component';
import { NavContentComponent } from './theme/layout/admin/navigation/nav-content/nav-content.component';
import { NavGroupComponent } from './theme/layout/admin/navigation/nav-content/nav-group/nav-group.component';
import { NavCollapseComponent } from './theme/layout/admin/navigation/nav-content/nav-collapse/nav-collapse.component';
import { NavItemComponent } from './theme/layout/admin/navigation/nav-content/nav-item/nav-item.component';
import { SharedModule } from './theme/shared/shared.module';
import { ConfigurationComponent } from './theme/layout/admin/configuration/configuration.component';
import { GuestComponent } from './theme/layout/guest/guest.component';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AuthInterceptor } from './http-interceptors/auth-header-interceptor';
import { MessageService } from 'primeng/api';
import { SpinnerComponent } from './components/spinner/spinner.component';
import { ToastModule } from 'primeng/toast';
import { DropdownModule } from 'primeng/dropdown';

import { AddOrganizationComponent } from './demo/pages/organizations/add-organization/add-organization.component';
import { UpdateOrganizationComponent } from './demo/pages/organizations/update-organization/update-organization.component';
import { AddUserComponent } from './demo/pages/users/add-user/add-user.component';
import { UserRoleComponent } from './demo/pages/users/user-role/user-role.component';
import { AutoCompleteComponent } from './components/auto-complete/auto-complete.component';
import { CustomerCategoryComponent } from './demo/pages/system-control/scs-data/customer-category/customer-category.component';
import { MessageGroupComponent } from './pages/message-group/message-group/message-group.component';
import { TableModule } from 'primeng/table';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BreadcrumbModule } from "./theme/shared/components/breadcrumb/breadcrumb.module";
import { NgScrollbarModule } from 'ngx-scrollbar';

import { AddGroupsComponent } from './pages/message-group/message-group/add-groups/add-groups.component';
import { UpdateGroupComponent } from './pages/message-group/message-group/update-group/update-group.component';
import { AddPhoneComponent } from './pages/group-phones/add-phone/add-phone.component';
import { GroupPhonesComponent } from './pages/group-phones/group-phones.component';
import { UpdatePhonesComponent } from './pages/group-phones/update-phones/update-phones/update-phones.component';
import { SendMessageComponent } from './pages/send-message/send-message.component';
import { CreateMessageComponent } from './pages/send-message/create-message/create-message/create-message.component';
import { UpdatMessageComponent } from './pages/send-message/updat-message/updat-message.component';
import { UnsentComponent } from './pages/unsent/unsent.component';
import { RejectComponent } from './pages/unsent/reject/reject.component';
import { ReportComponent } from './pages/report/report/report.component';








@NgModule({
    declarations: [
        AppComponent,
        AdminComponent,
        NavBarComponent,
        NavLeftComponent,
        NavRightComponent,
        NavigationComponent,
        NavLogoComponent,
        NavContentComponent,
        NavGroupComponent,
        NavItemComponent,
        NavCollapseComponent,
        ConfigurationComponent,
        GuestComponent,
        SpinnerComponent,
        AddOrganizationComponent,
        UpdateOrganizationComponent,
        AddUserComponent,
        UserRoleComponent,
        AutoCompleteComponent,
        CustomerCategoryComponent, MessageGroupComponent,AddGroupsComponent, UpdateGroupComponent, AddPhoneComponent,GroupPhonesComponent, UpdatePhonesComponent, SendMessageComponent, CreateMessageComponent, UpdatMessageComponent, UnsentComponent, RejectComponent, ReportComponent

    ],
    providers: [
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true,
        },
        MessageService,
        NavigationItem
    ],
    bootstrap: [AppComponent],
    imports: [BrowserModule, AppRoutingModule, TableModule, BrowserAnimationsModule, HttpClientModule, ToastModule, DropdownModule, ReactiveFormsModule, BreadcrumbModule,NgScrollbarModule,FormsModule]
})
export class AppModule { }
