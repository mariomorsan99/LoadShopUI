import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingLoadAuditLogGridComponent } from './shipping-load-audit-log-grid.component';

describe('ShippingLoadAuditLogGridComponent', () => {
  let component: ShippingLoadAuditLogGridComponent;
  let fixture: ComponentFixture<ShippingLoadAuditLogGridComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ShippingLoadAuditLogGridComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingLoadAuditLogGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
