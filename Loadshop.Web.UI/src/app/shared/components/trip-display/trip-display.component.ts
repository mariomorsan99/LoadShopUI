import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { LoadStop } from '../../models';

@Component({
  selector: 'kbxl-trip-display',
  templateUrl: './trip-display.component.html',
  styleUrls: ['./trip-display.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TripDisplayComponent {
  private _loadStops: LoadStop[];
  @Input() set loadStops(value: LoadStop[]) {
    this._loadStops = value ? value.sort((a, b) => (a.stopNbr > b.stopNbr ? 1 : a.stopNbr === b.stopNbr ? 0 : -1)) : null;
  }

  constructor() {}

  get tripOrigin(): LoadStop {
    return this._loadStops && this._loadStops.length > 0 ? this._loadStops[0] : null;
  }

  get tripDestination(): LoadStop {
    return this._loadStops.length > 1 ? this._loadStops[this._loadStops.length - 1] : null;
  }
}
