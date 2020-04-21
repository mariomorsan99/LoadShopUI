import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { SelectItem } from 'primeng/api';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { UserAdminData } from 'src/app/shared/models';
import {
  AdminState,
  getAllAuthorizedCarrierScacs,
  getAllAuthorizedSecurityRoles,
  getAllAuthorizedShippers,
  getCreateMode,
  getLoadingIdentityUser,
  getLoadingSelectedUser,
  getSavingUser,
  getSelectedUser,
  getUsers,
  UserAdminCreateUserAction,
  UserAdminLoadAuthorizedCarrierScacsAction,
  UserAdminLoadAuthorizedSecurityRolesAction,
  UserAdminLoadAuthorizedShippersAction,
  UserAdminLoadIdentityUserAction,
  UserAdminLoadUserAction,
  UserAdminLoadUsersAction,
  UserAdminUpdateUserAction,
  UserAdminLoadAuthorizedCarriersAction,
  getAllAuthorizedCarriers,
  getCarriersLoading,
  getCarrierUsers,
  UserAdminLoadCarrierUsersAction,
  getLoadingCarrierUsers,
  UserAdminClearSelectedUserAction,
} from '../../../store';

@Component({
  selector: 'kbxl-users-container',
  templateUrl: './users-container.component.html',
  styleUrls: ['./users-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersContainerComponent implements OnInit {
  constructor(private adminStore: Store<AdminState>) {}

  public createMode$: Observable<boolean>;
  public authorizedShippers$: Observable<SelectItem[]>;
  public authorizedCarrierScacs$: Observable<SelectItem[]>;
  public authorizedSecurityRoles$: Observable<SelectItem[]>;
  public userResults$: Observable<UserAdminData[]>;
  public updatingUser$: Observable<UserAdminData>;
  public sortedAuthorizedCarrierScacs$: Observable<SelectItem[]>;
  public processing$: Observable<boolean>;
  public carrierScacsChangedBehavior = new BehaviorSubject<boolean>(true);
  public carrierScacsChanged$ = this.carrierScacsChangedBehavior.asObservable();

  loadingCarriers$: Observable<boolean>;
  authorizedCarriers$: Observable<SelectItem[]>;
  loadingCarrierUsers$: Observable<boolean>;
  carrierUsers$: Observable<UserAdminData[]>;

  ngOnInit(): void {
    // Populate Dropdown Data
    this.adminStore.dispatch(new UserAdminLoadAuthorizedShippersAction());
    this.authorizedShippers$ = this.adminStore.pipe(
      map(getAllAuthorizedShippers),
      map(items =>
        this.toSelectItems(
          items,
          x => x.name,
          x => x.customerId
        )
      )
    );

    this.adminStore.dispatch(new UserAdminLoadAuthorizedCarrierScacsAction());
    this.authorizedCarrierScacs$ = this.adminStore.pipe(
      map(getAllAuthorizedCarrierScacs),
      map(items =>
        this.toSelectItems(
          items,
          x => x.carrierId + '-' + x.scac,
          x => x.scac
        )
      )
    );

    this.adminStore.dispatch(new UserAdminLoadAuthorizedCarriersAction());
    this.loadingCarriers$ = this.adminStore.pipe(map(getCarriersLoading));
    this.authorizedCarriers$ = this.adminStore.pipe(
      map(getAllAuthorizedCarriers),
      map(items =>
        this.toSelectItems(
          items,
          x => x.carrierName,
          x => x.carrierId
        )
      )
    );
    this.loadingCarrierUsers$ = this.adminStore.pipe(map(getLoadingCarrierUsers));
    this.carrierUsers$ = this.adminStore.pipe(map(getCarrierUsers));

    this.adminStore.dispatch(new UserAdminLoadAuthorizedSecurityRolesAction());
    this.authorizedSecurityRoles$ = this.adminStore.pipe(
      map(getAllAuthorizedSecurityRoles),
      map(items =>
        this.toSelectItems(
          items,
          x => x.accessRoleName,
          x => x.accessRoleId
        )
      )
    );

    this.userResults$ = this.adminStore.pipe(map(getUsers));

    this.updatingUser$ = this.adminStore.pipe(map(getSelectedUser));

    this.createMode$ = this.adminStore.pipe(map(getCreateMode));

    this.sortedAuthorizedCarrierScacs$ = combineLatest(this.authorizedCarrierScacs$, this.updatingUser$, this.carrierScacsChanged$).pipe(
      map(args => {
        if (args[2]) {
          return args[0].sort((a, b) => this.carrierSort(a, b, args[1]));
        }

        return args[0];
      })
    );

    this.processing$ = combineLatest(
      this.adminStore.pipe(map(getLoadingSelectedUser)),
      this.adminStore.pipe(map(getSavingUser)),
      this.adminStore.pipe(map(getLoadingIdentityUser))
    ).pipe(map(args => args[0] || args[1] || args[2]));
  }

  searchUsers(query: string) {
    this.adminStore.dispatch(new UserAdminLoadUsersAction({ query }));
  }

  userSelected(user: UserAdminData) {
    this.adminStore.dispatch(new UserAdminLoadUserAction({ userId: user.userId }));
  }

  carrierSelected(carrierId: string) {
    this.adminStore.dispatch(new UserAdminLoadCarrierUsersAction(carrierId));
    this.adminStore.dispatch(new UserAdminClearSelectedUserAction());
  }

  toSelectItems<T>(items: T[], getLabel: (item: T) => any, getValue: (item: T) => any): SelectItem[] {
    return items.map(item => {
      return { label: getLabel(item), value: getValue(item) };
    });
  }

  carrierSort(a: SelectItem, b: SelectItem, u: UserAdminData): number {
    if (u && u.carrierScacs.length > 0) {
      const selectedA = u.carrierScacs.indexOf(a.value) >= 0;
      const selectedB = u.carrierScacs.indexOf(b.value) >= 0;

      if (selectedA !== selectedB) {
        return selectedA ? -1 : 1;
      }
    }

    return a.label.localeCompare(b.label);
  }

  carrierScacSelected() {
    this.carrierScacsChangedBehavior.next(true);
  }

  saveUser(payload: { user: UserAdminData; createMode: boolean }) {
    if (payload.createMode) {
      this.adminStore.dispatch(new UserAdminCreateUserAction({ user: payload.user }));
    } else {
      this.adminStore.dispatch(new UserAdminUpdateUserAction({ user: payload.user }));
    }
  }

  loadIdentityUser(username: string) {
    this.adminStore.dispatch(new UserAdminLoadIdentityUserAction({ username }));
  }
}
