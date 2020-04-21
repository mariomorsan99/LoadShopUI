import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LoadCarrierGroupGridComponent } from './load-carrier-group-grid.component';

describe('LoadCarrierGroupGridComponent', () => {
  let component: LoadCarrierGroupGridComponent;
  let fixture: ComponentFixture<LoadCarrierGroupGridComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LoadCarrierGroupGridComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LoadCarrierGroupGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
