import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LoadCarrierGroupDetailComponent } from './load-carrier-group-detail.component';

describe('LoadCarrierGroupDetailComponent', () => {
  let component: LoadCarrierGroupDetailComponent;
  let fixture: ComponentFixture<LoadCarrierGroupDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LoadCarrierGroupDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LoadCarrierGroupDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
