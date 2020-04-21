import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingLoadDetailComponent } from './shipping-load-detail.component';

describe('ShippingLoadDetailComponent', () => {
  let component: ShippingLoadDetailComponent;
  let fixture: ComponentFixture<ShippingLoadDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ShippingLoadDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingLoadDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
