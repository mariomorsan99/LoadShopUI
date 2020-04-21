import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { UnitOfMeasure } from '../../models';

@Component({
  selector: 'kbxl-load-line-item-form',
  templateUrl: './load-line-item-form.component.html',
  styleUrls: ['./load-line-item-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.Default,
})
export class LoadLineItemFormComponent {
  @Input() lineItem: FormGroup;
  @Input() index: number;
  @Input() errorLabels: any;
  @Input() unitsOfMeasure: UnitOfMeasure[];
  @Input() loadingUnitsOfMeasure: boolean;

  private _pickupStopNumbers;
  @Input() set pickupStopNumbers(value: { stopNbr: number }[]) {
    this._pickupStopNumbers = value;
    this.displayPickupStopSelection = (value || []).length > 1;
  }
  get pickupStopNumbers() {
    return this._pickupStopNumbers;
  }

  displayPickupStopSelection = false;
}
