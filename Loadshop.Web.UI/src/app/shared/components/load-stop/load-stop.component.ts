import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { LoadStop } from '../../../shared/models';

@Component({
  selector: 'kbxl-load-stop',
  templateUrl: './load-stop.component.html',
  styleUrls: ['./load-stop.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadStopComponent {
  @Input() stop: LoadStop;
  @Input() total: number;
}
