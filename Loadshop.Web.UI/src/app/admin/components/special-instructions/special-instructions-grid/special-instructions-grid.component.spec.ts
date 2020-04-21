import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SpecialInstructionsGridComponent } from './special-instructions-grid.component';

describe('SpecialInstructionsGridComponent', () => {
  let component: SpecialInstructionsGridComponent;
  let fixture: ComponentFixture<SpecialInstructionsGridComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SpecialInstructionsGridComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SpecialInstructionsGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
