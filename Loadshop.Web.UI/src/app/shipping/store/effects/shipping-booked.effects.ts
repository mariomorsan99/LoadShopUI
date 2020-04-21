import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action, Store } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, withLatestFrom } from 'rxjs/operators';
import { ShippingLoadSearchTypes } from 'src/app/shared/models/shipping-load-search-types';
import { ShippingService } from '../../services';
import {
  ShippingBookedActionTypes,
  ShippingBookedLoadAction,
  ShippingBookedLoadFailureAction,
  ShippingBookedLoadSuccessAction,
} from '../actions';
import {
  LoadDetailCarrierRemovedActionTypes,
  LoadDetailCarrierRemovedSuccessAction,
  LoadDetailDeleteLoadAction,
  LoadDetailDeleteLoadActionTypes,
} from 'src/app/shared/store';
import { PageableQueryHelper } from 'src/app/shared/utilities';
import { mapToPayload } from '@tms-ng/shared';
import { ShippingState } from '../reducers';
import { getShippingBookedQueryHelper } from '../selectors';

@Injectable()
export class ShippingBookedEffects {
  @Effect()
  $loadBooked: Observable<Action> = this.actions$.pipe(
    ofType<ShippingBookedLoadAction>(ShippingBookedActionTypes.Load_Shipping_Booked),
    mapToPayload<{ searchType: string; queryHelper: PageableQueryHelper }>(),
    switchMap(payload => {
      return this.shippingService.getPageableLoadsBySearchType(ShippingLoadSearchTypes.Booked, payload.queryHelper).pipe(
        map(data => new ShippingBookedLoadSuccessAction(data)),
        catchError(err => of(new ShippingBookedLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $carrierRemoved: Observable<Action> = this.actions$.pipe(
    ofType<LoadDetailCarrierRemovedSuccessAction>(LoadDetailCarrierRemovedActionTypes.CarrierRemoved_Success),
    withLatestFrom(this.store$.pipe(map(getShippingBookedQueryHelper))),
    switchMap(([action, currentQuery]) => {
      return this.shippingService.getPageableLoadsBySearchType(ShippingLoadSearchTypes.Booked, currentQuery).pipe(
        map(data => new ShippingBookedLoadSuccessAction(data)),
        catchError(err => of(new ShippingBookedLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadDeleted: Observable<Action> = this.actions$.pipe(
    ofType<LoadDetailDeleteLoadAction>(LoadDetailDeleteLoadActionTypes.DeleteLoad_Success),
    withLatestFrom(this.store$.pipe(map(getShippingBookedQueryHelper))),
    switchMap(([action, currentQuery]) => {
      return this.shippingService.getPageableLoadsBySearchType(ShippingLoadSearchTypes.Booked, currentQuery).pipe(
        map(data => new ShippingBookedLoadSuccessAction(data)),
        catchError(err => of(new ShippingBookedLoadFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private shippingService: ShippingService, private store$: Store<ShippingState>) {}
}
