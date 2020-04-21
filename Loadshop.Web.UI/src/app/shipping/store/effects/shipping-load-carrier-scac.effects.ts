import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { ShippingService } from '../../services';
import {
  ShippingLoadCarrierScacActionTypes,
  ShippingLoadCarrierScacsLoadAction,
  ShippingLoadCarrierScacsLoadFailureAction,
  ShippingLoadCarrierScacsLoadSuccessAction,
} from '../actions';

@Injectable()
export class ShippingLoadCarrierScacEffects {
  @Effect()
  $loadScacs: Observable<Action> = this.actions$.pipe(
    ofType<ShippingLoadCarrierScacsLoadAction>(ShippingLoadCarrierScacActionTypes.Load_Carrier_Scacs_Load),
    mapToPayload<{ loadId: string }>(),
    // use mergeMap to allow the logs for multiple loads to be loaded at once
    mergeMap((payload: { loadId: string }) => {
      return this.shippingService.getLoadCarrierScacs(payload.loadId).pipe(
        map(data => new ShippingLoadCarrierScacsLoadSuccessAction({ loadId: payload.loadId, scacs: data })),
        catchError(err => of(new ShippingLoadCarrierScacsLoadFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private shippingService: ShippingService) {}
}
