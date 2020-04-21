import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UserLaneDetailContainerComponent } from './user-lane-detail-container.component';

describe('UserLaneDetailContainerComponent', () => {
  let component: UserLaneDetailContainerComponent;
  let fixture: ComponentFixture<UserLaneDetailContainerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UserLaneDetailContainerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserLaneDetailContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
