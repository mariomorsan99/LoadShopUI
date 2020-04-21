import { TitleCasePipe } from '@angular/common';
import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { defaultUserAdminData, UserAdminData } from 'src/app/shared/models';
import { UserAdminService } from '../../services';
import {
  UserAdminActionTypes,
  UserAdminCreateUserAction,
  UserAdminCreateUserFailureAction,
  UserAdminCreateUserSuccessAction,
  UserAdminLoadAdminUsersAction,
  UserAdminLoadAdminUsersFailureAction,
  UserAdminLoadAdminUsersSuccessAction,
  UserAdminLoadAuthorizedCarrierScacsAction,
  UserAdminLoadAuthorizedCarrierScacsFailureAction,
  UserAdminLoadAuthorizedCarrierScacsSuccessAction,
  UserAdminLoadAuthorizedSecurityRolesAction,
  UserAdminLoadAuthorizedSecurityRolesFailureAction,
  UserAdminLoadAuthorizedSecurityRolesSuccessAction,
  UserAdminLoadAuthorizedShippersAction,
  UserAdminLoadAuthorizedShippersFailureAction,
  UserAdminLoadAuthorizedShippersSuccessAction,
  UserAdminLoadIdentityUserAction,
  UserAdminLoadIdentityUserFailureAction,
  UserAdminLoadIdentityUserNotFoundAction,
  UserAdminLoadIdentityUserSuccessAction,
  UserAdminLoadUserAction,
  UserAdminLoadUserFailureAction,
  UserAdminLoadUsersAction,
  UserAdminLoadUsersFailureAction,
  UserAdminLoadUsersSuccessAction,
  UserAdminLoadUserSuccessAction,
  UserAdminUpdateUserAction,
  UserAdminUpdateUserFailureAction,
  UserAdminUpdateUserSuccessAction,
  UserAdminLoadAuthorizedCarriersAction,
  UserAdminLoadAuthorizedCarriersSuccessAction,
  UserAdminLoadAuthorizedCarriersFailureAction,
  UserAdminLoadCarrierUsersAction,
  UserAdminLoadCarrierUsersSuccessAction,
  UserAdminLoadCarrierUsersFailureAction,
} from '../actions';

@Injectable()
export class UserAdminEffects {
  @Effect()
  $loadUsers: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminLoadUsersAction>(UserAdminActionTypes.Load_Users),
    mapToPayload<{ query: string }>(),
    switchMap((payload: { query: string }) => {
      return this.userAdminService.getUsers(payload.query).pipe(
        map(data => new UserAdminLoadUsersSuccessAction(data)),
        catchError(err => of(new UserAdminLoadUsersFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadAdminUsers: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminLoadAdminUsersAction>(UserAdminActionTypes.Load_AdminUsers),
    mapToPayload<{ query: string }>(),
    switchMap((payload: { query: string }) => {
      return this.userAdminService.getAdminUsers().pipe(
        map(data => new UserAdminLoadAdminUsersSuccessAction(data)),
        catchError(err => of(new UserAdminLoadAdminUsersFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadUser: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminLoadUserAction>(UserAdminActionTypes.Load_User),
    mapToPayload<{ userId: string }>(),
    switchMap((payload: { userId: string }) => {
      return this.userAdminService.getUser(payload.userId).pipe(
        map(data => new UserAdminLoadUserSuccessAction(data)),
        catchError(err => of(new UserAdminLoadUserFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadIdentityUser: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminLoadIdentityUserAction>(UserAdminActionTypes.Load_Identity_User),
    mapToPayload<{ username: string }>(),
    switchMap((payload: { username: string }) => {
      return this.userAdminService.getIdentityUser(payload.username).pipe(
        map(data => {
          if (data) {
            return new UserAdminLoadIdentityUserSuccessAction({
              ...defaultUserAdminData,
              identUserId: data.id,
              username: data.userName,
              firstName: data.firstName,
              lastName: data.lastName,
              email: data.email,
              companyName: data.company,
            });
          }
          this.messageService.add({ id: 0, detail: 'User not found. Please check the user name and try again', severity: 'warn' });
          return new UserAdminLoadIdentityUserNotFoundAction();
        }),
        catchError(err => of(new UserAdminLoadIdentityUserFailureAction(err)))
      );
    })
  );

  @Effect()
  $updateUser: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminUpdateUserAction>(UserAdminActionTypes.Update_User),
    mapToPayload<{ user: UserAdminData }>(),
    switchMap((payload: { user: UserAdminData }) => {
      return this.userAdminService.updateUser(payload.user).pipe(
        map(data => new UserAdminUpdateUserSuccessAction(data)),
        catchError(err => of(new UserAdminUpdateUserFailureAction(err)))
      );
    })
  );

  @Effect({ dispatch: false })
  $userUpdated: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminUpdateUserSuccessAction>(UserAdminActionTypes.Update_User_Success),
    tap((action: UserAdminUpdateUserSuccessAction) =>
      this.messageService.add({
        id: 0,
        detail: `${this.titleCasePipe.transform(action.payload.firstName)} ${this.titleCasePipe.transform(
          action.payload.lastName
        )} Updated`,
        severity: 'success',
      })
    )
  );

  @Effect()
  $createUser: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminCreateUserAction>(UserAdminActionTypes.Create_User),
    mapToPayload<{ user: UserAdminData }>(),
    switchMap((payload: { user: UserAdminData }) => {
      return this.userAdminService.createUser(payload.user).pipe(
        map(data => new UserAdminCreateUserSuccessAction(data)),
        catchError(err => of(new UserAdminCreateUserFailureAction(err)))
      );
    })
  );

  @Effect({ dispatch: false })
  $userCreated: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminCreateUserSuccessAction>(UserAdminActionTypes.Create_User_Success),
    tap((action: UserAdminCreateUserSuccessAction) =>
      this.messageService.add({
        id: 0,
        detail: `${this.titleCasePipe.transform(action.payload.firstName)} ${this.titleCasePipe.transform(
          action.payload.lastName
        )} Created`,
        severity: 'success',
      })
    )
  );

  @Effect()
  $loadAllMyAuthorizedShippers: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminLoadAuthorizedShippersAction>(UserAdminActionTypes.Load_Authorized_Shippers),
    switchMap(() => {
      return this.userAdminService.getAllMyAuthorizedShippers().pipe(
        map(data => new UserAdminLoadAuthorizedShippersSuccessAction(data)),
        catchError(err => of(new UserAdminLoadAuthorizedShippersFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadAllMyAuthorizedCarrierScacs: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminLoadAuthorizedCarrierScacsAction>(UserAdminActionTypes.Load_Authorized_CarrierScacs),
    switchMap(() => {
      return this.userAdminService.getAllMyAuthroizedCarrierScacs().pipe(
        map(data => new UserAdminLoadAuthorizedCarrierScacsSuccessAction(data)),
        catchError(err => of(new UserAdminLoadAuthorizedCarrierScacsFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadAllMyAuthorizedSecurityRoles: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminLoadAuthorizedSecurityRolesAction>(UserAdminActionTypes.Load_Authorized_Security_Roles),
    switchMap(() => {
      return this.userAdminService.getAllMyAuthorizedSecurityRoles().pipe(
        map(data => new UserAdminLoadAuthorizedSecurityRolesSuccessAction(data)),
        catchError(err => of(new UserAdminLoadAuthorizedSecurityRolesFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadAllMyAuthorizedCarriers: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminLoadAuthorizedCarriersAction>(UserAdminActionTypes.Load_Authorized_Carriers),
    switchMap(() => {
      return this.userAdminService.getAllMyAuthorizedCarriers().pipe(
        map(data => new UserAdminLoadAuthorizedCarriersSuccessAction(data)),
        catchError(err => of(new UserAdminLoadAuthorizedCarriersFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadCarrierUsers: Observable<Action> = this.actions$.pipe(
    ofType<UserAdminLoadCarrierUsersAction>(UserAdminActionTypes.Load_Carrier_Users),
    map(action => action.payload),
    switchMap((carrierId) => {
      return this.userAdminService.getCarrierUsers(carrierId).pipe(
        map(data => new UserAdminLoadCarrierUsersSuccessAction(data)),
        catchError(err => of(new UserAdminLoadCarrierUsersFailureAction(err)))
      );
    })
  );

  constructor(
    private actions$: Actions,
    private userAdminService: UserAdminService,
    private messageService: MessageService,
    private titleCasePipe: TitleCasePipe
  ) {}
}
