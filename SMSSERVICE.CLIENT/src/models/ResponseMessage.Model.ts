export interface ResponseMessage{
    success : boolean;
    message: string;
    data: any;
    errorCode:number
}

export interface SelectList {

    id: string;
    name: string;
    // employeeId ?: string 
    // reason?:string
    // photo ?:string
    // commiteeStatus?:string

}
