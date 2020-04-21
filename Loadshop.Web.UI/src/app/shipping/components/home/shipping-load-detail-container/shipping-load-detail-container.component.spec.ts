import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingLoadDetailContainerComponent } from './shipping-load-detail-container.component';

describe('ShippingLoadDetailContainerComponent', () => {
  let component: ShippingLoadDetailContainerComponent;
  let fixture: ComponentFixture<ShippingLoadDetailContainerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ShippingLoadDetailContainerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingLoadDetailContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
