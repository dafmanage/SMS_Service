import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GroupPhonesComponent } from './group-phones.component';

describe('GroupPhonesComponent', () => {
  let component: GroupPhonesComponent;
  let fixture: ComponentFixture<GroupPhonesComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [GroupPhonesComponent]
    });
    fixture = TestBed.createComponent(GroupPhonesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
