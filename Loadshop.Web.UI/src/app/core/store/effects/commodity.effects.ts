import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, mapTo, switchMap } from 'rxjs/operators';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services/common.service';
import { CommodityActionTypes, CommodityLoadAction, CommodityLoadFailureAction, CommodityLoadSuccessAction } from '../actions';

@Injectable()
export class CommodityEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType(CommodityActionTypes.Load),
    switchMap(() => {
      return this.commonService.getCommodities().pipe(
        map(data => new CommodityLoadSuccessAction(data)),
        catchError(err => of(new CommodityLoadFailureAction(err)))
      );
    })
  );

  constructor(private commonService: CommonService, private actions$: Actions) {}

  @Effect()
  $test: Observable<Action> = this.actions$.pipe(ofType(LoadshopApplicationActionTypes.LoadshopStart), mapTo(new CommodityLoadAction()));
}
