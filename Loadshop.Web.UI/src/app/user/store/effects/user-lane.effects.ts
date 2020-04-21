import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action, select, Store } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/components/common/messageservice';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap, withLatestFrom } from 'rxjs/operators';
import { CoreState } from 'src/app/core/store';
import { UserLane } from '../../../shared/models';
import { loadshopApplicationReady, SharedState } from '../../../shared/store';
import { UserLanesService } from '../../services';
import {
  UserLaneActionTypes,
  UserLaneAddAction,
  UserLaneAddFailureAction,
  UserLaneAddSuccessAction,
  UserLaneDeleteAction,
  UserLaneDeleteFailureAction,
  UserLaneDeleteSuccessAction,
  UserLaneLoadAction,
  UserLaneLoadFailureAction,
  UserLaneLoadSuccessAction,
  UserLaneUpdateAction,
  UserLaneUpdateFailureAction,
  UserLaneUpdateSuccessAction,
  UserProfileActionTypes,
  UserProfileLoadSuccessAction,
} from '../actions';
import { getUserProfileModel } from '../selectors';

@Injectable()
export class UserLaneEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<UserLaneLoadAction>(UserLaneActionTypes.Load),
    withLatestFrom(this.store$.pipe(map(getUserProfileModel))),
    switchMap(([_, user]) => {
      return this.userLanesService.getCustomerSavedLanes(user).pipe(
        map(data => new UserLaneLoadSuccessAction(data)),
        catchError(err => of(new UserLaneLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $update: Observable<Action> = this.actions$.pipe(
    ofType<UserLaneUpdateAction>(UserLaneActionTypes.Update),
    mapToPayload<UserLane>(),
    switchMap((lane: UserLane) => {
      return this.userLanesService.updateLane(lane).pipe(
        map(data => new UserLaneUpdateSuccessAction(data)),
        catchError(err => of(new UserLaneUpdateFailureAction(err)))
      );
    })
  );

  @Effect()
  $add: Observable<Action> = this.actions$.pipe(
    ofType<UserLaneAddAction>(UserLaneActionTypes.Add),
    mapToPayload<UserLane>(),
    switchMap((lane: UserLane) => {
      return this.userLanesService.addLane(lane).pipe(
        map(data => new UserLaneAddSuccessAction(data)),
        catchError(err => of(new UserLaneAddFailureAction(err)))
      );
    })
  );

  @Effect()
  $delete: Observable<Action> = this.actions$.pipe(
    ofType<UserLaneDeleteAction>(UserLaneActionTypes.Delete),
    mapToPayload<UserLane>(),
    switchMap((lane: UserLane) => {
      return this.userLanesService.deleteLane(lane).pipe(
        map(data => new UserLaneDeleteSuccessAction(lane)),
        catchError(err => of(new UserLaneDeleteFailureAction(err)))
      );
    })
  );

  @Effect({ dispatch: false })
  $laneAdd: Observable<Action> = this.actions$.pipe(
    ofType<UserLaneAddSuccessAction>(UserLaneActionTypes.Add_Success),
    tap(userLane => this.messageService.add({ id: 0, detail: 'Favorite Added', severity: 'success' }))
  );

  @Effect({ dispatch: false })
  $laneUpdate: Observable<Action> = this.actions$.pipe(
    ofType<UserLaneUpdateSuccessAction>(UserLaneActionTypes.Update_Success),
    tap(userLane => this.messageService.add({ id: 0, detail: 'Favorite Updated', severity: 'success' }))
  );

  @Effect({ dispatch: false })
  $laneDelete: Observable<Action> = this.actions$.pipe(
    ofType<UserLaneDeleteSuccessAction>(UserLaneActionTypes.Delete_Success),
    tap(userLane => this.messageService.add({ id: 0, detail: 'Favorite Deleted', severity: 'success' }))
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType<UserProfileLoadSuccessAction>(UserProfileActionTypes.Load_Success),
    withLatestFrom(this.sharedStore$.pipe(select(loadshopApplicationReady))),
    switchMap(([user, appReady]) => {
      if (appReady) {
        return of(new UserLaneLoadAction());
      }
      return of();
    })
  );

  constructor(
    private actions$: Actions,
    private userLanesService: UserLanesService,
    private messageService: MessageService,
    private store$: Store<CoreState>,
    private sharedStore$: Store<SharedState>
  ) {}
}
