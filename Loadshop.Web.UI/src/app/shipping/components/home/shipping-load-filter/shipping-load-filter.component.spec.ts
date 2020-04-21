import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingLoadFilterComponent } from './shipping-load-filter.component';

describe('ShippingLoadAuditLogGridComponent', () => {
  let component: ShippingLoadFilterComponent;
  let fixture: ComponentFixture<ShippingLoadFilterComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ShippingLoadFilterComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingLoadFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
