import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ScsDataComponent } from './scs-data.component';

describe('ScsDataComponent', () => {
  let component: ScsDataComponent;
  let fixture: ComponentFixture<ScsDataComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ScsDataComponent]
    });
    fixture = TestBed.createComponent(ScsDataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
