import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services';
import { StateActionTypes, StateLoadAction, StateLoadFailureAction, StateLoadSuccessAction } from '../actions';

@Injectable()
export class StateEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType(StateActionTypes.Load),
    switchMap(() => {
      return this.commonService.getStates().pipe(
        map(data => new StateLoadSuccessAction(data)),
        catchError(err => of(new StateLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    map(() => {
      return new StateLoadAction();
    })
  );

  constructor(private commonService: CommonService, private actions$: Actions) {}
}
