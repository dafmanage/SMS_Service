export interface IMessagGetDto {

  id :string,
  content:string,
  messageStatus:string,
  language:string,
  messageGroup:string,
  isApproved:boolean,
  textSize:number,
  numberOfCustomer:number

}
export interface IMessagPostDto {

  //id :string,
  content:string,
  language:string,
  messageGroupId:string,
  createdById:string,
  organizationId:string

}
