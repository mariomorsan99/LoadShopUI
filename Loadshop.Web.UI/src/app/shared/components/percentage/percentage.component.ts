import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'kbxl-percentage',
    template: '<input type="number" [(ngModel)]="adjustedModel" [name]="inputName" [class]="inputClass" step="0.01" />'
})
export class PercentageComponent {
    @Input() inputName: string;
    @Input() inputClass: string;
    @Input() percentage: number;
    @Output() percentageChange: EventEmitter<number> = new EventEmitter<number>();

    set adjustedModel(value: number) {
        if (value != null) {
            this.percentageChange.emit(value / 100);
        } else {
            this.percentageChange.emit(null);
        }
    }

    get adjustedModel() {
        return this.percentage ? (this.percentage * 100) : null;
    }
}
