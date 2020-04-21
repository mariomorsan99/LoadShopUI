import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingLoadCreateContainerComponent } from './shipping-load-create-container.component';

describe('ShippingLoadCreateContainerComponent', () => {
  let component: ShippingLoadCreateContainerComponent;
  let fixture: ComponentFixture<ShippingLoadCreateContainerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ShippingLoadCreateContainerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingLoadCreateContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
