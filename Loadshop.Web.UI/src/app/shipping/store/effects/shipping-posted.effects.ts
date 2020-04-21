import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action, Store } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, withLatestFrom } from 'rxjs/operators';
import { ShippingService } from '../../services';
import {
  ShippingPostedActionTypes,
  ShippingPostedLoadAction,
  ShippingPostedLoadFailureAction,
  ShippingPostedLoadSuccessAction,
} from '../actions';
import { LoadDetailDeleteLoadAction, LoadDetailDeleteLoadActionTypes } from 'src/app/shared/store';
import { ShippingLoadSearchTypes } from 'src/app/shared/models/shipping-load-search-types';
import { PageableQueryHelper } from 'src/app/shared/utilities';
import { ShippingState } from '../reducers';
import { getShippingPostedQueryHelper } from '../selectors/shipping-posted.selectors';

@Injectable()
export class ShippingPostedEffects {
  @Effect()
  $loadPosted: Observable<Action> = this.actions$.pipe(
    ofType<ShippingPostedLoadAction>(ShippingPostedActionTypes.Load_Shipping_Posted),
    mapToPayload<{ searchType: string; queryHelper: PageableQueryHelper }>(),
    switchMap((payload: { searchType: string; queryHelper: PageableQueryHelper }) => {
      return this.shippingService.getPageableLoadsBySearchType(payload.searchType, payload.queryHelper).pipe(
        map(data => new ShippingPostedLoadSuccessAction(data)),
        catchError(err => of(new ShippingPostedLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadDeleted: Observable<Action> = this.actions$.pipe(
    ofType<LoadDetailDeleteLoadAction>(LoadDetailDeleteLoadActionTypes.DeleteLoad_Success),
    withLatestFrom(this.store$.pipe(map(getShippingPostedQueryHelper))),
    switchMap(([action, currentQuery]) => {
      return this.shippingService.getPageableLoadsBySearchType(ShippingLoadSearchTypes.Posted, currentQuery).pipe(
        map(data => new ShippingPostedLoadSuccessAction(data)),
        catchError(err => of(new ShippingPostedLoadFailureAction(err)))
      );
    })
  );

  constructor(private actions$: Actions, private shippingService: ShippingService, private store$: Store<ShippingState>) {}
}
