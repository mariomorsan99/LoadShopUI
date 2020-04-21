import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { ShippingService } from '../../services';
import {
  ShippingLoadCarrierScacRestrictionActionTypes,
  ShippingLoadCarrierScacRestrictionsLoadAction,
  ShippingLoadCarrierScacRestrictionsLoadFailureAction,
  ShippingLoadCarrierScacRestrictionsLoadSuccessAction,
} from '../actions';

@Injectable()
export class ShippingLoadCarrierScacRestrictionEffects {
  @Effect()
  $loadScacs: Observable<Action> = this.actions$.pipe(
    ofType<ShippingLoadCarrierScacRestrictionsLoadAction>(
      ShippingLoadCarrierScacRestrictionActionTypes.Load_Carrier_Scac_Restrictions_Load
    ),
    mapToPayload<{ loadId: string }>(),
    // use mergeMap to allow the logs for multiple loads to be loaded at once
    mergeMap((payload: { loadId: string }) => {
      return this.shippingService.getLoadCarrierScacRestrictions(payload.loadId).pipe(
        map(data => new ShippingLoadCarrierScacRestrictionsLoadSuccessAction({ loadId: payload.loadId, scacs: data })),
        catchError(err => of(new ShippingLoadCarrierScacRestrictionsLoadFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private shippingService: ShippingService) {}
}
