import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import {
  LoadAuditLogData
} from 'src/app/shared/models';

@Component({
  selector: 'kbxl-shipping-load-audit-log-grid',
  templateUrl: './shipping-load-audit-log-grid.component.html',
  styleUrls: ['./shipping-load-audit-log-grid.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShippingLoadAuditLogGridComponent {
  @Input() loadAuditLogs: LoadAuditLogData[];
  @Input() loadingAuditLogs: boolean;

  first = 0;

  constructor() { }

  onPage($event) {
    if ($event && $event.first) {
      this.first = $event.first;
    }
  }
}
