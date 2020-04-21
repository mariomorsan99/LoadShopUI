import { ChangeDetectionStrategy, Component, OnDestroy } from '@angular/core';
import { Observable, combineLatest, Subscription } from 'rxjs';
import { Store, select } from '@ngrx/store';
import {
  AdminState,
  UserCommunicationLoadAction,
  getSelectedUserCommunciation,
  getAllCarriers,
  CarrierProfileLoadAllAction,
  UserAdminLoadAuthorizedShippersAction,
  getAllAuthorizedShippers,
  UserAdminLoadAuthorizedSecurityRolesAction,
  getAllAuthorizedSecurityRoles,
  UserAdminLoadUsersAction,
  getUsers,
  UserCommunicationUpdateAction,
  getAllCarriersLoading,
  getAdminUsersLoading,
  getSecurityRolesLoading,
  getSelectedUserCommunciationLoading,
  getShippersLoading,
  getUserCommunicationCreateMode,
  UserCommunicationCreateAction,
  getSelectedUserCommunciationUpdating,
} from 'src/app/admin/store';
import { map } from 'rxjs/operators';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { UserCommunicationDetail, Carrier, Customer, ISecurityAccessRoleData, UserAdminData } from 'src/app/shared/models';

@Component({
  selector: 'kbxl-user-communication-form-container',
  templateUrl: './user-communication-form-container.component.html',
  styleUrls: ['./user-communication-form-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserCommunicationFormContainerComponent implements OnDestroy {
  userCommunication$: Observable<UserCommunicationDetail>;
  allCarriers$: Observable<Carrier[]>;
  allShippers$: Observable<Customer[]>;
  allSecurityRoles$: Observable<ISecurityAccessRoleData[]>;
  allUsers$: Observable<UserAdminData[]>;
  processing$: Observable<boolean>;
  createMode$: Observable<boolean>;
  routeSubcription: Subscription;
  routeInit = false;

  constructor(
    private adminStore: Store<AdminState>,
    private route: ActivatedRoute,
    private messageService: MessageService,
    private router: Router
  ) {
    const routeId$ = this.route.params.pipe(map(p => p.id));
    this.createMode$ = this.adminStore.pipe(select(getUserCommunicationCreateMode));

    this.routeSubcription = combineLatest(routeId$, this.createMode$)
      .pipe(
        map(args => {
          const routeId = args[0];
          const createMode = args[1];
          return { routeId, createMode };
        })
      )
      .subscribe(routeData => {
        if (!routeData.createMode && !this.routeInit) {
          if (!routeData.routeId) {
            this.messageService.add({ detail: 'Invalid User Communication Id provided.', severity: 'error' });
          } else if (routeData.routeId) {
            this.adminStore.dispatch(new UserCommunicationLoadAction({ userCommunicationId: routeData.routeId }));
          }
        }

        this.routeInit = true;
      });

    this.userCommunication$ = this.adminStore.pipe(select(getSelectedUserCommunciation));

    this.adminStore.dispatch(new UserAdminLoadAuthorizedShippersAction());
    this.allShippers$ = this.adminStore.pipe(select(getAllAuthorizedShippers));

    this.adminStore.dispatch(new CarrierProfileLoadAllAction());
    this.allCarriers$ = this.adminStore.pipe(select(getAllCarriers));

    this.adminStore.dispatch(new UserAdminLoadAuthorizedSecurityRolesAction());
    this.allSecurityRoles$ = this.adminStore.pipe(select(getAllAuthorizedSecurityRoles));

    this.adminStore.dispatch(new UserAdminLoadUsersAction({ query: '' }));
    this.allUsers$ = this.adminStore.pipe(select(getUsers));

    this.processing$ = combineLatest(
      this.adminStore.pipe(map(getSelectedUserCommunciationLoading)),
      this.adminStore.pipe(map(getAllCarriersLoading)),
      this.adminStore.pipe(map(getAdminUsersLoading)),
      this.adminStore.pipe(map(getSecurityRolesLoading)),
      this.adminStore.pipe(map(getShippersLoading)),
      this.adminStore.pipe(map(getSelectedUserCommunciationUpdating))
    ).pipe(map(args => args[0] || args[1] || args[2] || args[3] || args[4] || args[5]));
  }

  userCommunicationUpdate(userCommunication: UserCommunicationDetail) {
    this.adminStore.dispatch(new UserCommunicationUpdateAction(userCommunication));
  }

  userCommunicationCreate(userCommunication: UserCommunicationDetail) {
    this.adminStore.dispatch(new UserCommunicationCreateAction(userCommunication));
  }

  userCommunicationCancel() {
    this.router.navigate(['maint/user-communications']);
  }

  ngOnDestroy(): void {
    this.routeSubcription.unsubscribe();
  }
}
