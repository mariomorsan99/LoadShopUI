import { ChangeDetectorRef, Directive, ElementRef, EventEmitter, OnDestroy, OnInit } from '@angular/core';
import { Dropdown } from 'primeng/dropdown';
import { MultiSelect } from 'primeng/multiselect';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

class CloseOnScrollDirectiveBase implements OnInit, OnDestroy {
  private destroyed$ = new Subject<boolean>();
  private listener = () => this.ancestorScrolled();

  constructor(
    private el: ElementRef,
    private ref: ChangeDetectorRef,
    private onHide: EventEmitter<any>,
    private onShow: EventEmitter<any>
  ) {}

  ngOnInit() {
    this.onHide.pipe(takeUntil(this.destroyed$)).subscribe(() => this.removeScrollListeners(this.el.nativeElement.parentElement));
    this.onShow.pipe(takeUntil(this.destroyed$)).subscribe(() => this.addScrollListeners(this.el.nativeElement.parentElement));
  }

  private isScrollable(element) {
    const styles = getComputedStyle(element);
    return styles.overflowX === 'auto' || styles.overflowY === 'auto' || styles.overflowX === 'scroll' || styles.overflowY === 'scroll';
  }

  addScrollListeners(element) {
    if (element) {
      if (this.isScrollable(element)) {
        // minimize the number of elements we listen on
        element.addEventListener('scroll', this.listener);
      }

      this.addScrollListeners(element.parentElement);
    }
  }

  removeScrollListeners(element) {
    if (element) {
      element.removeEventListener('scroll', this.listener);

      this.removeScrollListeners(element.parentElement);
    }
  }

  // This should be overriden in the derived classes
  ancestorScrolled() {
    this.closeOnScroll(false);
  }

  closeOnScroll(closed: boolean) {
    this.removeScrollListeners(this.el.nativeElement.parentElement);
    if (closed) {
      this.ref.detectChanges();
    } // have to trigger detection for those controls that use the push strategy
  }

  ngOnDestroy() {
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }
}

@Directive({
  selector: 'p-multiSelect[kbxlCloseOnScroll]',
})
export class CloseMultiSelectOnScrollDirective extends CloseOnScrollDirectiveBase implements OnInit, OnDestroy {
  constructor(private multiselect: MultiSelect, el: ElementRef, ref: ChangeDetectorRef) {
    super(el, ref, multiselect.onPanelHide, multiselect.onPanelShow);
  }

  ngOnInit() {
    super.ngOnInit();
  }
  ngOnDestroy() {
    super.ngOnDestroy();
  }

  ancestorScrolled() {
    const closed = this.multiselect.overlayVisible;
    if (closed) {
      this.multiselect.hide();
    }
    this.closeOnScroll(closed);
  }
}

@Directive({
  selector: 'p-dropdown[kbxlCloseOnScroll]',
})
export class CloseDropdownOnScrollDirective extends CloseOnScrollDirectiveBase implements OnInit, OnDestroy {
  constructor(private dropdown: Dropdown, el: ElementRef, ref: ChangeDetectorRef) {
    super(el, ref, dropdown.onHide, dropdown.onShow);
  }

  ngOnInit() {
    super.ngOnInit();
  }
  ngOnDestroy() {
    super.ngOnDestroy();
  }

  ancestorScrolled() {
    const closed = this.dropdown.overlayVisible;
    if (closed) {
      this.dropdown.hide();
    }
    this.closeOnScroll(closed);
  }
}
