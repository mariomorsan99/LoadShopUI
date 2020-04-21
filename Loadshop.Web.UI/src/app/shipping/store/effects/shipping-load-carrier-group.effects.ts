import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { ShippingService } from '../../services';
import {
  ShippingLoadCarrierGroupActionTypes,
  ShippingLoadCarrierGroupsLoadAction,
  ShippingLoadCarrierGroupsLoadFailureAction,
  ShippingLoadCarrierGroupsLoadSuccessAction,
} from '../actions';

@Injectable()
export class ShippingLoadCarrierGroupEffects {
  @Effect()
  $loadGroups: Observable<Action> = this.actions$.pipe(
    ofType<ShippingLoadCarrierGroupsLoadAction>(ShippingLoadCarrierGroupActionTypes.Load_Carrier_Groups_Load),
    mapToPayload<{ loadId: string }>(),
    // use mergeMap to allow the logs for multiple loads to be loaded at once
    mergeMap((payload: { loadId: string }) => {
      return this.shippingService.getLoadCarrierGroups(payload.loadId).pipe(
        map(data => new ShippingLoadCarrierGroupsLoadSuccessAction({ loadId: payload.loadId, logs: data })),
        catchError(err => of(new ShippingLoadCarrierGroupsLoadFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private shippingService: ShippingService) {}
}
