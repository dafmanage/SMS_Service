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
    id :string;
    organizationId :string
    name : string
    userName : string
    status : string
    imagePath : string
    department : string
    position : string
    email : string
    phoneNumber : string
    roles : string[]
}
export interface UserPost {
    organizationId:string,
    userName :string,
    password:string

}
