export interface User {

    userName: string ;
    password: string ;
}
export interface UserView {
    fullName : string ;
    role: string [];
    userId : string ;
    organizationId:string;
    photo:string;
}
export interface ChangePasswordModel{
    UserId : string
    CurrentPassword :string
    NewPassword :string
   }

export interface UserList {
    id: string;
    organizationId: string; // Will be converted from Guid to string
    name: string;
    userName: string;
    status: string;
    imagePath: string;
    email: string;
    phoneNumber: string;
    roles: RoleDropDown[];
    lastLoginDate?: Date;
}

export interface RoleDropDown {
    id: string;
    name: string;
}
export interface UserPost {
    organizationId: string;
    userName: string;
    password: string;
    firstName?: string;
    lastName?: string;
    email?: string;
    phoneNumber?: string;
    Roles?: string;
}
