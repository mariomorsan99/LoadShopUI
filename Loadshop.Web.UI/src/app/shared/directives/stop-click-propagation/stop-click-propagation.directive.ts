import { Directive, HostListener } from '@angular/core';

@Directive({
  selector: '[kbxlStopClickPropagation]',
})
export class StopClickPropagationDirective {
  constructor() {}

  @HostListener('click', ['$event'])
  public onClick(event: any): void {
    event.stopPropagation();
  }
}
