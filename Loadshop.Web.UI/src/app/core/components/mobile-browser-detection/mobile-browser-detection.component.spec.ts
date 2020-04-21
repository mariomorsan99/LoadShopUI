import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MobileBrowserDetectionComponent } from './mobile-browser-detection.component';

describe('MobileBrowserDetectionComponent', () => {
  let component: MobileBrowserDetectionComponent;
  let fixture: ComponentFixture<MobileBrowserDetectionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MobileBrowserDetectionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MobileBrowserDetectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
