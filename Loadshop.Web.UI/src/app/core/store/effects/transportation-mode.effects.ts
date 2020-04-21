import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services/common.service';
import {
  TransportationModeActionTypes,
  TransportationModeLoadAction,
  TransportationModeLoadFailureAction,
  TransportationModeLoadSuccessAction,
} from '../actions';

@Injectable()
export class TransportationModeEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType(TransportationModeActionTypes.Load),
    switchMap(() => {
      return this.commonService.getTransportationModes().pipe(
        map(data => new TransportationModeLoadSuccessAction(data)),
        catchError(err => of(new TransportationModeLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    map(() => {
      return new TransportationModeLoadAction();
    })
  );

  constructor(private commonService: CommonService, private actions$: Actions) {}
}
