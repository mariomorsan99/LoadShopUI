import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services/common.service';
import {
  AppointmentSchedulerConfirmationTypeActionTypes,
  AppointmentSchedulerConfirmationTypeLoadAction,
  AppointmentSchedulerConfirmationTypeLoadFailureAction,
  AppointmentSchedulerConfirmationTypeLoadSuccessAction,
} from '../actions';

@Injectable()
export class AppointmentSchedulerConfirmationTypeEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType(AppointmentSchedulerConfirmationTypeActionTypes.Load),
    switchMap(() => {
      return this.commonService.getAppointmentSchedulerConfirmationTypes().pipe(
        map(data => new AppointmentSchedulerConfirmationTypeLoadSuccessAction(data)),
        catchError(err => of(new AppointmentSchedulerConfirmationTypeLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    map(() => {
      return new AppointmentSchedulerConfirmationTypeLoadAction();
    })
  );

  constructor(private commonService: CommonService, private actions$: Actions) {}
}
