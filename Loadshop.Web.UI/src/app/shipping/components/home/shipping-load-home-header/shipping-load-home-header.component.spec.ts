import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingLoadHomeHeaderComponent } from './shipping-load-home-header.component';

describe('ShippingLoadHomeHeaderComponent', () => {
  let component: ShippingLoadHomeHeaderComponent;
  let fixture: ComponentFixture<ShippingLoadHomeHeaderComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ShippingLoadHomeHeaderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingLoadHomeHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
