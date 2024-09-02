import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MeterSizeComponent } from './meter-size.component';

describe('MeterSizeComponent', () => {
  let component: MeterSizeComponent;
  let fixture: ComponentFixture<MeterSizeComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MeterSizeComponent]
    });
    fixture = TestBed.createComponent(MeterSizeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
