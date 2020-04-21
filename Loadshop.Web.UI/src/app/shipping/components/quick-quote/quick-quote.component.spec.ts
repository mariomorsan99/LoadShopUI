import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { QuickQuoteComponent } from './quick-quote.component';

describe('QuickQuoteComponent', () => {
  let component: QuickQuoteComponent;
  let fixture: ComponentFixture<QuickQuoteComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ QuickQuoteComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(QuickQuoteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
