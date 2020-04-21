import { Action } from '@ngrx/store';
import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { PageableQueryHelper } from 'src/app/shared/utilities/';
import { PageableResult } from 'src/app/shared/models';

export enum ShippingDeliveredActionTypes {
  Load_Shipping_Delivered = '[Shipping Delivered] LOAD',
  Load_Shipping_Delivered_Success = '[Shipping Delivered] LOAD_SUCCESS',
  Load_Shipping_Delivered_Failure = '[Shipping Delivered] LOAD_FAILURE',
  Load_Shipping_Query_Update = '[Shipping Delivered] QUERY_UPDATE',
}

export class ShippingDeliveredLoadAction implements Action {
  readonly type = ShippingDeliveredActionTypes.Load_Shipping_Delivered;

  constructor(public payload: { searchType: string; queryHelper: PageableQueryHelper }) {}
}

export class ShippingDeliveredLoadSuccessAction implements Action {
  readonly type = ShippingDeliveredActionTypes.Load_Shipping_Delivered_Success;

  constructor(public payload: PageableResult<ShippingLoadView>) {}
}

export class ShippingDeliveredLoadFailureAction implements Action {
  readonly type = ShippingDeliveredActionTypes.Load_Shipping_Delivered_Failure;

  constructor(public payload: Error) {}
}

export class ShippingDeliveredQueryUpdateAction implements Action {
  readonly type = ShippingDeliveredActionTypes.Load_Shipping_Query_Update;

  constructor(public payload: PageableQueryHelper) {}
}

export type ShippingDeliveredActions =
  | ShippingDeliveredLoadAction
  | ShippingDeliveredLoadSuccessAction
  | ShippingDeliveredLoadFailureAction
  | ShippingDeliveredQueryUpdateAction;
