import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LoadStatusContainerComponent } from './load-status-container.component';

describe('LoadStatusContainerComponent', () => {
  let component: LoadStatusContainerComponent;
  let fixture: ComponentFixture<LoadStatusContainerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LoadStatusContainerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LoadStatusContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
