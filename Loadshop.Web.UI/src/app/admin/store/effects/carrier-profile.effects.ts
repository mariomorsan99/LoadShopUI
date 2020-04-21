import { Injectable } from '@angular/core';
import { Effect, ofType, Actions } from '@ngrx/effects';
import { mapToPayload } from '@tms-ng/shared';
import { switchMap, map, catchError, tap } from 'rxjs/operators';
import { of, Observable } from 'rxjs';
import { Action } from '@ngrx/store';
import {
  CarrierProfileLoadAction,
  CarrierProfileActionTypes,
  CarrierProfileLoadSuccessAction,
  CarrierProfileLoadFailureAction,
  CarrierProfileUpdateAction,
  CarrierProfileUpdateSuccessAction,
  CarrierProfileUpdateFailureAction,
  CarrierProfileLoadAllFailureAction,
  CarrierProfileLoadAllSuccessAction,
} from '../actions';
import { CarrierProfileService } from '../../services';
import { MessageService } from 'primeng/api';
import { CarrierProfile } from 'src/app/shared/models';
import { TitleCasePipe } from '@angular/common';

@Injectable()
export class CarrierProfileEffects {
  @Effect()
  $loadCarrierProfile: Observable<Action> = this.actions$.pipe(
    ofType<CarrierProfileLoadAction>(CarrierProfileActionTypes.Load),
    mapToPayload<{ carrierId: string }>(),
    switchMap((payload: { carrierId: string }) => {
      return this.carrierProfileService.getCarrier(payload.carrierId).pipe(
        map(data => new CarrierProfileLoadSuccessAction(data)),
        catchError(err => of(new CarrierProfileLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $updateCarrierProfile: Observable<Action> = this.actions$.pipe(
    ofType<CarrierProfileUpdateAction>(CarrierProfileActionTypes.Update),
    mapToPayload<CarrierProfile>(),
    switchMap((payload: CarrierProfile) => {
      return this.carrierProfileService.updateCarrier(payload).pipe(
        map(data => new CarrierProfileUpdateSuccessAction(data)),
        catchError(err => of(new CarrierProfileUpdateFailureAction(err)))
      );
    }),
    tap((action: CarrierProfileUpdateSuccessAction) =>
      this.messageService.add({ id: 0, detail: `${this.titleCasePipe.transform(action.payload.carrierName)} Updated`, severity: 'success' })
    )
  );

  @Effect()
  $loadAllCarriers: Observable<Action> = this.actions$.pipe(
    ofType<CarrierProfileLoadAllFailureAction>(CarrierProfileActionTypes.Load_All),
    switchMap(() => {
      return this.carrierProfileService.getAllCarriers().pipe(
        map(data => new CarrierProfileLoadAllSuccessAction(data)),
        catchError(err => of(new CarrierProfileLoadAllFailureAction(err)))
      );
    })
  );

  constructor(
    private actions$: Actions,
    private carrierProfileService: CarrierProfileService,
    private messageService: MessageService,
    private titleCasePipe: TitleCasePipe
  ) {}
}
