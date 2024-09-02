export interface IMessagGroupGetDto {

  id :string,
  groupName:string,
  groupCode:string,
  remark:string,
  organizationId:string,
  organizationName:string

}
export interface IMessagGroupPostDto {


  groupName:string,
  groupCode:string,
  remark:string,
  organizationId: string,
  createdById: string

}
export interface IMessagGroupPutDto {

  Id:string,
  GroupName:string,
  GroupCode:string,
  Remark:string,
  //createdById: string
  OrganizationId:string,
  OrganizationName:string


}
