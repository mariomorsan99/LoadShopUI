import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { UserCommunication } from '../../models';
import { UserCommunicationDisplayService } from '../../services';
import {
  LoadshopApplicationActionTypes,
  UserCommunicationDisplayAcknowledgeAction,
  UserCommunicationDisplayAcknowledgeFailureAction,
  UserCommunicationDisplayAcknowledgeSuccessAction,
  UserCommunicationDisplayActionTypes,
  UserCommunicationDisplayLoadAction,
  UserCommunicationDisplayLoadFailureAction,
  UserCommunicationDisplayLoadSuccessAction,
} from '../actions';

@Injectable()
export class UserCommunicationDisplayEffects {
  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    switchMap(() => {
      return of(new UserCommunicationDisplayLoadAction());
    })
  );

  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<UserCommunicationDisplayLoadAction>(UserCommunicationDisplayActionTypes.Load),
    switchMap(() => {
      return this.userCommunicationDisplayService.getUserCommunicationsForDisplay().pipe(
        map(data => new UserCommunicationDisplayLoadSuccessAction(data)),
        catchError(err => of(new UserCommunicationDisplayLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $acknowledge: Observable<Action> = this.actions$.pipe(
    ofType<UserCommunicationDisplayAcknowledgeAction>(UserCommunicationDisplayActionTypes.Acknowledge),
    mapToPayload<UserCommunication>(),
    switchMap(userCommunication => {
      return this.userCommunicationDisplayService.acknowledgeUserCommunication(userCommunication.userCommunicationId).pipe(
        map(data => new UserCommunicationDisplayAcknowledgeSuccessAction(data)),
        catchError(err => of(new UserCommunicationDisplayAcknowledgeFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private userCommunicationDisplayService: UserCommunicationDisplayService) {}
}
