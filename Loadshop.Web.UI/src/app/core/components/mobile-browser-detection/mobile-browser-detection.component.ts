import { AfterViewInit, Component, ElementRef, HostListener } from '@angular/core';
import { Store } from '@ngrx/store';
import { BrowserSetIsMobileAction, CoreState } from '../../store';

@Component({
  selector: 'kbxl-mobile-browser-detection',
  template: '<div class="d-block d-lg-none"></div>',
})
export class MobileBrowserDetectionComponent implements AfterViewInit {
  constructor(private elementRef: ElementRef, private store: Store<CoreState>) {}

  @HostListener('window:resize', [])

  // tslint:disable-next-line:no-unused-variable
  private onResize() {
    this.detectScreenSize();
  }

  ngAfterViewInit() {
    this.detectScreenSize();
  }

  private detectScreenSize() {
    // check its display property value
    const isMobileElementVisible = window.getComputedStyle(this.elementRef.nativeElement.childNodes[0]).display !== 'none';
    this.store.dispatch(new BrowserSetIsMobileAction({ isMobile: isMobileElementVisible }));
  }
}
