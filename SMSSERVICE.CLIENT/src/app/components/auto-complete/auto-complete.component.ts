import { Component, EventEmitter, Input, Output } from '@angular/core';
import { SelectList } from 'src/models/ResponseMessage.Model';

@Component({
  selector: 'app-auto-complete',
  templateUrl: './auto-complete.component.html',
  styleUrls: ['./auto-complete.component.scss']
})
export class AutoCompleteComponent {
@Input() data: SelectList[] = [];
@Input() selectedId!: string;
@Input() placeHolder!:string;
@Input() isDisabled : boolean = false
placeholder!: String;
selectedValue: any



@Output() selectedItem = new EventEmitter<any>();


selectEvent(item: any) {


  this.selectedItem.emit(item.id)
}

onChangeSearch(val: string) {
  // fetch remote data from here
  // And reassign the 'data' which is binded to 'data' property.
}

onFocused(e: any) {
  // do something when input is focused
}
constructor() { }

ngOnInit() {

  let key = this.data.filter(t => t.id === this.selectedId)
  if (key[0] != null) {   
    this.placeHolder = key[0].name
    this.selectEvent(key[0].id)    
  }  
}


}