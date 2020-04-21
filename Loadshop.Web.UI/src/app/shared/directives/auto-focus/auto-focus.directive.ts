import { AfterContentInit, Directive, ElementRef } from '@angular/core';

@Directive({
  selector: '[kbxlAutoFocus]',
})
export class AutoFocusDirective implements AfterContentInit {
  constructor(private el: ElementRef) {}

  public ngAfterContentInit() {
    setTimeout(() => {
      const input = this.el.nativeElement.querySelector('input');
      (input || this.el.nativeElement).focus();
    }, 200);
  }
}
