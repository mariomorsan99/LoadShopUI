import { ChangeDetectionStrategy, Component } from '@angular/core';
import { select, Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { UserCommunication } from '../../../shared/models';
import {
  getUserCommunications,
  getUserCommunicatonAcknowledgementPosting,
  SharedState,
  UserCommunicationDisplayAcknowledgeAction,
} from '../../store';

@Component({
  selector: 'kbxl-user-communication-display',
  templateUrl: './user-communication-display.component.html',
  styleUrls: ['./user-communication-display.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserCommunicationDisplayComponent {
  userCommunications$: Observable<UserCommunication[]>;
  processing$: Observable<boolean>;

  constructor(private sharedStore: Store<SharedState>) {
    this.userCommunications$ = this.sharedStore.pipe(select(getUserCommunications));

    this.processing$ = this.sharedStore.pipe(select(getUserCommunicatonAcknowledgementPosting));
  }

  onAcknowledge(communication: UserCommunication) {
    this.sharedStore.dispatch(new UserCommunicationDisplayAcknowledgeAction(communication));
  }
}
