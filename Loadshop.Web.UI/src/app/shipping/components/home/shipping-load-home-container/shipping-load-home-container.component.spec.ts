import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingLoadHomeContainerComponent } from './shipping-load-home-container.component';

describe('ShippingLoadHomeContainerComponent', () => {
  let component: ShippingLoadHomeContainerComponent;
  let fixture: ComponentFixture<ShippingLoadHomeContainerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ShippingLoadHomeContainerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingLoadHomeContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
