import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { UserCommunicationService } from '../../services';
import {
  UserCommunicationLoadAllAction,
  UserCommunicationActionTypes,
  UserCommunicationLoadAllSuccessAction,
  UserCommunicationLoadAllFailureAction,
  UserCommunicationUpdateSuccessAction,
  UserCommunicationUpdateAction,
  UserCommunicationUpdateFailureAction,
  UserCommunicationCreateAction,
  UserCommunicationCreateSuccessAction,
  UserCommunicationCreateFailureAction,
  UserCommunicationLoadAction,
  UserCommunicationLoadSuccessAction,
  UserCommunicationLoadFailureAction,
} from '../actions';
import { UserCommunicationDetail } from 'src/app/shared/models';
import { NavigationGoAction } from '@tms-ng/core';

@Injectable()
export class UserCommunicationEffects {
  @Effect()
  $loadUserCommunications: Observable<Action> = this.actions$.pipe(
    ofType<UserCommunicationLoadAllAction>(UserCommunicationActionTypes.Load_All),
    switchMap(() => {
      return this.userCommunicationService.getUserCommunications().pipe(
        map(data => new UserCommunicationLoadAllSuccessAction(data)),
        catchError(err => of(new UserCommunicationLoadAllFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadUserCommunication: Observable<Action> = this.actions$.pipe(
    ofType<UserCommunicationLoadAction>(UserCommunicationActionTypes.Load),
    mapToPayload<{ userCommunicationId: string }>(),
    switchMap((payload: { userCommunicationId: string }) => {
      return this.userCommunicationService.getUserCommunication(payload.userCommunicationId).pipe(
        map(data => new UserCommunicationLoadSuccessAction(data)),
        catchError(err => of(new UserCommunicationLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $updateUserCommunication: Observable<Action> = this.actions$.pipe(
    ofType<UserCommunicationUpdateAction>(UserCommunicationActionTypes.Update),
    mapToPayload<UserCommunicationDetail>(),
    switchMap((payload: UserCommunicationDetail) => {
      return this.userCommunicationService.updateUserCommunication(payload).pipe(
        map(data => new UserCommunicationUpdateSuccessAction(data)),
        catchError(err => of(new UserCommunicationUpdateFailureAction(err)))
      );
    }),
    tap((action: UserCommunicationUpdateSuccessAction) =>
      this.messageService.add({ id: 0, detail: `User Communication "${action.payload.title}" Updated`, severity: 'success' })
    ),
    switchMap(result => {
      return [new NavigationGoAction({ path: [`/maint/user-communications/`] }), result];
    })
  );

  @Effect()
  $createUserCommunication: Observable<Action> = this.actions$.pipe(
    ofType<UserCommunicationCreateAction>(UserCommunicationActionTypes.Create),
    mapToPayload<UserCommunicationDetail>(),
    switchMap((payload: UserCommunicationDetail) => {
      return this.userCommunicationService.createUserCommunication(payload).pipe(
        map(data => new UserCommunicationCreateSuccessAction(data)),
        catchError(err => of(new UserCommunicationCreateFailureAction(err)))
      );
    }),
    tap((action: UserCommunicationCreateSuccessAction) =>
      this.messageService.add({ id: 0, detail: `User Communication "${action.payload.title}" Created`, severity: 'success' })
    ),
    switchMap(result => {
      return [new NavigationGoAction({ path: [`/maint/user-communications/`] }), result];
    })
  );

  constructor(
    private actions$: Actions,
    private userCommunicationService: UserCommunicationService,
    private messageService: MessageService
  ) {}
}
