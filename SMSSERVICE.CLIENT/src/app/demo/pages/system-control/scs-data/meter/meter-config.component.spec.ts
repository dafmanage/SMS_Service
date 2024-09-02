import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MeterConfigComponent } from './meter-config.component';

describe('MeterConfigComponent', () => {
  let component: MeterConfigComponent;
  let fixture: ComponentFixture<MeterConfigComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MeterConfigComponent]
    });
    fixture = TestBed.createComponent(MeterConfigComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
