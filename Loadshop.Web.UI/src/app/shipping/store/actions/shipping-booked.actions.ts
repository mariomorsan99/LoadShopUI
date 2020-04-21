import { Action } from '@ngrx/store';
import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { PageableQueryHelper } from 'src/app/shared/utilities';
import { PageableResult } from 'src/app/shared/models';

export enum ShippingBookedActionTypes {
  Load_Shipping_Booked = '[Shipping Booked] LOAD',
  Load_Shipping_Booked_Success = '[Shipping Booked] LOAD_SUCCESS',
  Load_Shipping_Booked_Failure = '[Shipping Booked] LOAD_FAILURE',
  Update_Shipping_Booked_Query = '[Shipping Booked] UPDATE_QUERY',
}

export class ShippingBookedLoadAction implements Action {
  readonly type = ShippingBookedActionTypes.Load_Shipping_Booked;

  constructor(public payload: { searchType: string; queryHelper: PageableQueryHelper }) {}
}

export class ShippingBookedLoadSuccessAction implements Action {
  readonly type = ShippingBookedActionTypes.Load_Shipping_Booked_Success;

  constructor(public payload: PageableResult<ShippingLoadView>) {}
}

export class ShippingBookedLoadFailureAction implements Action {
  readonly type = ShippingBookedActionTypes.Load_Shipping_Booked_Failure;

  constructor(public payload: Error) {}
}

export class ShippingBookedUpdateQueryAction implements Action {
  readonly type = ShippingBookedActionTypes.Update_Shipping_Booked_Query;

  constructor(public payload: PageableQueryHelper) {}
}

export type ShippingBookedActions =
  | ShippingBookedLoadAction
  | ShippingBookedLoadSuccessAction
  | ShippingBookedLoadFailureAction
  | ShippingBookedUpdateQueryAction;
