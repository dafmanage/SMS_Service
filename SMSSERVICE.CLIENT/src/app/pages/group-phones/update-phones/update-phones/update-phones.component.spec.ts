import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdatePhonesComponent } from './update-phones.component';

describe('UpdatePhonesComponent', () => {
  let component: UpdatePhonesComponent;
  let fixture: ComponentFixture<UpdatePhonesComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [UpdatePhonesComponent]
    });
    fixture = TestBed.createComponent(UpdatePhonesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
