import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services/common.service';
import {
  UnitOfMeasureActionTypes,
  UnitOfMeasureLoadAction,
  UnitOfMeasureLoadFailureAction,
  UnitOfMeasureLoadSuccessAction,
} from '../actions';

@Injectable()
export class UnitOfMeasureEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType(UnitOfMeasureActionTypes.Load),
    switchMap(() => {
      return this.commonService.getUnitsOfMeasure().pipe(
        map(data => new UnitOfMeasureLoadSuccessAction(data)),
        catchError(err => of(new UnitOfMeasureLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    map(() => {
      return new UnitOfMeasureLoadAction();
    })
  );

  constructor(private commonService: CommonService, private actions$: Actions) {}
}
