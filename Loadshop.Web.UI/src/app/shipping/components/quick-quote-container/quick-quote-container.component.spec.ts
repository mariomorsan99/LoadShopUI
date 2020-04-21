import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { QuickQuoteContainerComponent } from './quick-quote-container.component';

describe('QuickQuoteContainerComponent', () => {
  let component: QuickQuoteContainerComponent;
  let fixture: ComponentFixture<QuickQuoteContainerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ QuickQuoteContainerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(QuickQuoteContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
