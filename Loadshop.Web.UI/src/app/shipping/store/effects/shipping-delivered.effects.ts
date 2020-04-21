import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { ShippingLoadSearchTypes } from 'src/app/shared/models/shipping-load-search-types';
import { ShippingService } from '../../services';
import {
  ShippingDeliveredActionTypes,
  ShippingDeliveredLoadAction,
  ShippingDeliveredLoadFailureAction,
  ShippingDeliveredLoadSuccessAction,
} from '../actions';
import { mapToPayload } from '@tms-ng/shared';
import { PageableQueryHelper } from 'src/app/shared/utilities';

@Injectable()
export class ShippingDeliveredEffects {
  @Effect()
  $loadDelivered: Observable<Action> = this.actions$.pipe(
    ofType<ShippingDeliveredLoadAction>(ShippingDeliveredActionTypes.Load_Shipping_Delivered),
    mapToPayload<{ searchType: string; queryHelper: PageableQueryHelper }>(),
    switchMap(payload => {
      return this.shippingService.getPageableLoadsBySearchType(ShippingLoadSearchTypes.Delivered, payload.queryHelper).pipe(
        map(data => new ShippingDeliveredLoadSuccessAction(data)),
        catchError(err => of(new ShippingDeliveredLoadFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private shippingService: ShippingService) {}
}
