import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services/common.service';
import { ServiceTypeActionTypes, ServiceTypeLoadAction, ServiceTypeLoadFailureAction, ServiceTypeLoadSuccessAction } from '../actions';

@Injectable()
export class ServiceTypeEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType(ServiceTypeActionTypes.Load),
    switchMap(() => {
      return this.commonService.getServiceTypes().pipe(
        map(data => new ServiceTypeLoadSuccessAction(data)),
        catchError(err => of(new ServiceTypeLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    map(() => {
      return new ServiceTypeLoadAction();
    })
  );

  constructor(private commonService: CommonService, private actions$: Actions) {}
}
