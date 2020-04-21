import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { LoadCarrierGroupService } from '../../services';
import {
  LoadCarrierGroupCarrierActionTypes,
  LoadCarrierGroupCarrierDeleteAllAction,
  LoadCarrierGroupCarrierDeleteAllFailureAction,
  LoadCarrierGroupCarrierDeleteAllSuccessAction,
  LoadCarrierGroupCarrierLoadAction,
  LoadCarrierGroupCarrierLoadFailureAction,
  LoadCarrierGroupCarrierLoadSuccessAction,
} from '../actions';

@Injectable()
export class LoadCarrierGroupCarrierEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupCarrierLoadAction>(LoadCarrierGroupCarrierActionTypes.Load),
    mapToPayload<{ loadCarrierGroupId: number }>(),
    switchMap((payload: { loadCarrierGroupId: number }) => {
      return this.loadCarrierGroupService.getCarriers(payload.loadCarrierGroupId).pipe(
        map(data => new LoadCarrierGroupCarrierLoadSuccessAction(data)),
        catchError(err => of(new LoadCarrierGroupCarrierLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $deleteAll: Observable<Action> = this.actions$.pipe(
    ofType<LoadCarrierGroupCarrierDeleteAllAction>(LoadCarrierGroupCarrierActionTypes.Delete_All),
    mapToPayload<{ loadCarrierGroupId: number }>(),
    switchMap((payload: { loadCarrierGroupId: number }) => {
      return this.loadCarrierGroupService.deleteAllCarriers(payload.loadCarrierGroupId).pipe(
        map(data => new LoadCarrierGroupCarrierDeleteAllSuccessAction(payload)),
        catchError(err => of(new LoadCarrierGroupCarrierDeleteAllFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private loadCarrierGroupService: LoadCarrierGroupService) {}
}
