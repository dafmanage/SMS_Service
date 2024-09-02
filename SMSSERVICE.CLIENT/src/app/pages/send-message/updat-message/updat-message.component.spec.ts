import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdatMessageComponent } from './updat-message.component';

describe('UpdatMessageComponent', () => {
  let component: UpdatMessageComponent;
  let fixture: ComponentFixture<UpdatMessageComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [UpdatMessageComponent]
    });
    fixture = TestBed.createComponent(UpdatMessageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
