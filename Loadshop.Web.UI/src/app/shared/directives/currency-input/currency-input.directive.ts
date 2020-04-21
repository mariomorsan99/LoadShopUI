import { Directive, ElementRef, HostListener, Renderer2, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

@Directive({
  selector: '[kbxlCurrencyInput]',
})
export class CurrencyInputDirective implements ControlValueAccessor {
  private _onChange: Function;
  private _onTouched: Function;

  constructor(@Self() private control: NgControl, private el: ElementRef, private renderer: Renderer2) {
    control.valueAccessor = this;
  }

  // Currently only supporting blur
  @HostListener('blur', ['$event'])
  onBlur(event: any) {
    let value = (this.el.nativeElement as HTMLInputElement).value.trim();
    value = value.indexOf('$') === 0 ? value.substr(1) : value;
    let num = Number(value);
    num = isNaN(num) ? 0 : num;

    this.writeValue(num);
    if (this.control.value !== num) {
      this._onChange(num);
    }
    this._onTouched(event);
  }

  writeValue(value: number): void {
    this.renderer.setProperty(this.el.nativeElement, 'value', value ? '$ ' + value.toFixed(2) : '');
  }
  registerOnTouched(callback: Function): void {
    this._onTouched = callback;
  }
  setDisabledState(isDisabled: boolean): void {
    this.renderer.setProperty(this.el.nativeElement, 'disabled', isDisabled);
  }
  registerOnChange(callback: Function): void {
    this._onChange = callback;
  }
}
