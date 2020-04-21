import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingLoadCardsComponent } from './shipping-load-cards.component';

describe('ShippingLoadCardsComponent', () => {
  let component: ShippingLoadCardsComponent;
  let fixture: ComponentFixture<ShippingLoadCardsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ShippingLoadCardsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingLoadCardsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
