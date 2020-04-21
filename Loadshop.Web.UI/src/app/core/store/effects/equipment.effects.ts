import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services/common.service';
import { EquipmentActionTypes, EquipmentLoadAction, EquipmentLoadFailureAction, EquipmentLoadSuccessAction } from '../actions';

@Injectable()
export class EquipmentEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType(EquipmentActionTypes.Load),
    switchMap(() => {
      return this.commonService.getEquipment().pipe(
        map(data => new EquipmentLoadSuccessAction(data)),
        catchError(err => of(new EquipmentLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    map(() => {
      return new EquipmentLoadAction();
    })
  );

  constructor(private commonService: CommonService, private actions$: Actions) {}
}
