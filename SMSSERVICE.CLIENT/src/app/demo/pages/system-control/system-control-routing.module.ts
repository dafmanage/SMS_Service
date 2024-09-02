import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import ScsDataComponent from './scs-data/scs-data.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'data',
        component:ScsDataComponent
      },
   
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SystemControlRoutingModule { }
