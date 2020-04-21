import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LoadCarrierGroupContainerComponent } from './load-carrier-group-container.component';

describe('LoadCarrierGroupContainerComponent', () => {
  let component: LoadCarrierGroupContainerComponent;
  let fixture: ComponentFixture<LoadCarrierGroupContainerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LoadCarrierGroupContainerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LoadCarrierGroupContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
