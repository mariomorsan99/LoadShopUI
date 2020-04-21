import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action, select, Store } from '@ngrx/store';
import { AuthLoginActionTypes, AuthLoginSuccessAction, NavigationGoAction } from '@tms-ng/core';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap, withLatestFrom } from 'rxjs/operators';
import { User, UserModel } from '../../../shared/models';
import {
  LoadshopApplicationActionTypes,
  loadshopApplicationReady,
  LoadshopApplicationStartAction,
  SharedState,
} from '../../../shared/store';
import { UserProfileService } from '../../services';
import {
  LoadStatusNotificationsLoadAction,
  LoadStatusNotificationsLoadFailureAction,
  LoadStatusNotificationsLoadSuccessAction,
  LoadStatusNotificationsUpdateAction,
  LoadStatusNotificationsUpdateFailureAction,
  LoadStatusNotificationsUpdateSuccessAction,
  UserProfileActionTypes,
  UserProfileLoadAction,
  UserProfileLoadFailureAction,
  UserProfileLoadSuccessAction,
  UserProfileUpdateAction,
  UserProfileUpdateFailureAction,
  UserProfileUpdateSuccessAction,
} from '../actions';
import { UserState } from '../reducers';
import { getUserProfileModel } from '../selectors';

@Injectable()
export class UserProfileEffects {
  /*
   ** Main startup action for the app.  We need to get the user profile in order to determine if the
   ** account is setup (i.e. agreed to terms)
   */
  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType<AuthLoginSuccessAction>(AuthLoginActionTypes.Login_Success),
    switchMap(() => {
      return this.userProfileService.getCustomerProfile().pipe(
        map(data => new UserProfileLoadSuccessAction(data)),
        catchError(err => {
          if (!(err instanceof HttpErrorResponse)) {
            return of(new NavigationGoAction({ path: ['invalid'] }));
          }
        })
      );
    })
  );

  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<UserProfileLoadAction>(UserProfileActionTypes.Load),
    mapToPayload<string>(),
    switchMap(() => {
      return this.userProfileService.getCustomerProfile().pipe(
        map(data => new UserProfileLoadSuccessAction(data)),
        catchError(err => of(new UserProfileLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadStatusNotifications: Observable<Action> = this.actions$.pipe(
    ofType<LoadStatusNotificationsLoadAction>(UserProfileActionTypes.LoadStatusNotifications_Load),
    switchMap(() => {
      return this.userProfileService.getUserLoadStatusNotifications().pipe(
        map(data => new LoadStatusNotificationsLoadSuccessAction(data)),
        catchError(err => {
          return of(new LoadStatusNotificationsLoadFailureAction(err));
        })
      );
    })
  );

  /*
   ** Checks if the user has agreed to the terms and will kick off the Loadshop start action, which will
   ** kick off background requests to load data for the app
   */
  @Effect()
  $checkTermsAndPolicy: Observable<Action> = this.actions$.pipe(
    ofType<UserProfileLoadSuccessAction>(UserProfileActionTypes.Load_Success),
    withLatestFrom(this.sharedStore$.pipe(select(loadshopApplicationReady))),
    switchMap(([profile, appReady]) => {
      const user = profile.payload;
      if (user && !user.hasAgreedToTerms) {
        if (this.router.url === '/agreements') {
          // the user is view the agreements, let them pass
          return of();
        } else {
          this.router.navigate(['/user-agreement']);
        }
      } else if (user && !appReady) {
        return of(new LoadshopApplicationStartAction());
      }
      return of();
    })
  );

  @Effect()
  $update: Observable<Action> = this.actions$.pipe(
    ofType<UserProfileUpdateAction>(UserProfileActionTypes.Update),
    mapToPayload<User>(),
    switchMap((customer: User) => {
      return this.userProfileService.update(customer).pipe(
        map(data => new UserProfileUpdateSuccessAction(data)),
        catchError(err => {
          if (err && err.error && err.error.title && err.error.detail) {
            this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
          }
          return of(new UserProfileUpdateFailureAction(err));
        })
      );
    })
  );

  @Effect({ dispatch: false })
  $profileMissingContactNumber = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    withLatestFrom(this.userStore$.pipe(select(getUserProfileModel))),
    tap(args => {
      if (args && args[1]) {
        const profile = args[1] as UserModel;
        if (!profile) {
          return;
        }
        if (!this.userProfileService.validateUserNotifications(profile)) {
          this.router.navigate(['/user/profile']);
        }
      }
    })
  );

  @Effect({ dispatch: false })
  $profileUpdate: Observable<Action> = this.actions$.pipe(
    ofType<UserProfileUpdateSuccessAction>(UserProfileActionTypes.Update_Success),
    tap(profile => this.messageService.add({ id: 0, detail: 'Profile Updated', severity: 'success' }))
  );

  @Effect()
  $updateLoadStatusNotifications: Observable<Action> = this.actions$.pipe(
    ofType<LoadStatusNotificationsUpdateAction>(UserProfileActionTypes.LoadStatusNotificationsUpdate),
    switchMap(action => {
      return this.userProfileService.updateLoadStatusNotifications(action.payload).pipe(
        map(data => new LoadStatusNotificationsUpdateSuccessAction(data)),
        catchError(err => {
          if (err && err.error && err.error.title && err.error.detail) {
            this.messageService.add({ summary: err.error.title, detail: err.error.detail, severity: 'error' });
          }
          return of(new LoadStatusNotificationsUpdateFailureAction(err));
        })
      );
    })
  );

  @Effect({ dispatch: false })
  $updateLoadStatusNotificationsMessage: Observable<Action> = this.actions$.pipe(
    ofType<LoadStatusNotificationsUpdateSuccessAction>(UserProfileActionTypes.LoadStatusNotificationsUpdate_Success),
    tap(profile => this.messageService.add({ id: 0, detail: 'Load status notifications updated', severity: 'success' }))
  );
  constructor(
    private actions$: Actions,
    private userProfileService: UserProfileService,
    private messageService: MessageService,
    private router: Router,
    private sharedStore$: Store<SharedState>,
    private userStore$: Store<UserState>
  ) {}
}
