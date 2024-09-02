import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomerCategoryComponent } from './customer-category.component';

describe('CustomerCategoryComponent', () => {
  let component: CustomerCategoryComponent;
  let fixture: ComponentFixture<CustomerCategoryComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [CustomerCategoryComponent]
    });
    fixture = TestBed.createComponent(CustomerCategoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
