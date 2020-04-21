import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Observable } from 'rxjs';
import { Store, select } from '@ngrx/store';
import {
  AdminState,
  getUserCommunications,
  UserCommunicationLoadAllAction,
  UserCommunicationCreateDefaultAction,
  getAllUserCommunicationsLoading,
} from 'src/app/admin/store';
import { UserCommunication } from 'src/app/shared/models';
import { Router } from '@angular/router';

@Component({
  selector: 'kbxl-user-communication-container',
  templateUrl: './user-communication-container.component.html',
  styleUrls: ['./user-communication-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserCommunicationContainerComponent {
  allUserCommunications$: Observable<UserCommunication[]>;
  processing$: Observable<boolean>;

  constructor(private adminStore: Store<AdminState>, private router: Router) {
    // this.processing$ = combineLatest(
    //   this.adminStore.pipe(map(getCarrierProfileLoading)),
    //   this.adminStore.pipe(map(getAllCarriersLoading)),
    //   this.adminStore.pipe(map(getAdminUsersLoading)),
    //   this.adminStore.pipe(map(getCarrierProfileUpdating))
    // ).pipe(map(args => args[0] || args[1] || args[2] || args[3]));
    this.adminStore.dispatch(new UserCommunicationLoadAllAction());
    this.allUserCommunications$ = this.adminStore.pipe(select(getUserCommunications));

    this.processing$ = this.adminStore.pipe(select(getAllUserCommunicationsLoading));
  }

  userCommunicationSelected(userCommunication: UserCommunication) {
    const path: [string | string] = ['maint/user-communications/detail'];
    if (userCommunication && userCommunication.userCommunicationId) {
      path.push(userCommunication.userCommunicationId);
    }
    this.router.navigate(path);
  }

  createNotification() {
    this.adminStore.dispatch(new UserCommunicationCreateDefaultAction());
    this.router.navigate(['maint/user-communications/detail']);
  }
}
