import { Component, OnInit, Input, ChangeDetectionStrategy, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'kbxl-button-toggle',
    templateUrl: './button-toggle.component.html',
    styleUrls: ['./button-toggle.component.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ButtonToggleComponent implements OnInit {
    @Input() onIcon: string;
    @Input() offIcon: string;
    @Input() val: boolean;
    @Output() valChange: EventEmitter<boolean> = new EventEmitter();
    @Output() toggle: EventEmitter<boolean> = new EventEmitter();

    ngOnInit() {
        if (this.val === undefined || this.val == null) {
            this.val = true;
        }
    }

    HandleIconClick() {
        this.val = !this.val;
        this.valChange.emit(this.val);
        this.toggle.emit(this.val);
    }
}
