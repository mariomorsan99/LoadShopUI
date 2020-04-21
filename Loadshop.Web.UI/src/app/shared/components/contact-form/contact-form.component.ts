import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'kbxl-contact-form',
    templateUrl: './contact-form.component.html',
    styleUrls: ['./contact-form.component.scss'],
    changeDetection: ChangeDetectionStrategy.Default
})
export class ContactFormComponent {
    @Input() contact: FormGroup;
    @Input() index: number;
}
