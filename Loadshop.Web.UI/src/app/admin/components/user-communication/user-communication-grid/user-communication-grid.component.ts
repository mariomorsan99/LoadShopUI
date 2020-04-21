import { ChangeDetectionStrategy, Component, Input, EventEmitter, Output } from '@angular/core';
import { UserCommunication } from 'src/app/shared/models';

@Component({
  selector: 'kbxl-user-communication-grid',
  templateUrl: './user-communication-grid.component.html',
  styleUrls: ['./user-communication-grid.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserCommunicationGridComponent {
  @Input() processing: boolean;
  @Input() allUserCommunications: UserCommunication[];
  @Output() userCommunicationSelected = new EventEmitter<UserCommunication>();
  @Output() createNotification = new EventEmitter();

  first = 0;

  onRowSelect(selectedCommunication: UserCommunication) {
    this.userCommunicationSelected.emit(selectedCommunication);
  }

  onPage($event) {
    if ($event && $event.first) {
      this.first = $event.first;
    }
  }

  addClick() {
    this.createNotification.emit();
  }
}
