import { Action } from '@ngrx/store';
import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { PageableQueryHelper } from 'src/app/shared/utilities';
import { PageableResult } from 'src/app/shared/models';

export enum ShippingPostedActionTypes {
  Load_Shipping_Posted = '[Posted] LOAD',
  Load_Shipping_Posted_Success = '[Posted] LOAD_SUCCESS',
  Load_Shipping_Posted_Failure = '[Posted] LOAD_FAILURE',
  Update_Shipping_Posted_Query = '[Posted] UPDATE_SHIPPING_POSTED_QUERY',
}

export class ShippingPostedLoadAction implements Action {
  readonly type = ShippingPostedActionTypes.Load_Shipping_Posted;

  constructor(public payload: { searchType: string; queryHelper: PageableQueryHelper }) {}
}

export class ShippingPostedLoadSuccessAction implements Action {
  readonly type = ShippingPostedActionTypes.Load_Shipping_Posted_Success;

  constructor(public payload: PageableResult<ShippingLoadView>) {}
}

export class ShippingPostedLoadFailureAction implements Action {
  readonly type = ShippingPostedActionTypes.Load_Shipping_Posted_Failure;

  constructor(public payload: Error) {}
}

export class ShippingPostedUpdateQueryAction implements Action {
  readonly type = ShippingPostedActionTypes.Update_Shipping_Posted_Query;

  constructor(public payload: PageableQueryHelper) {}
}

export type ShippingPostedActions =
  | ShippingPostedLoadAction
  | ShippingPostedLoadSuccessAction
  | ShippingPostedLoadFailureAction
  | ShippingPostedUpdateQueryAction;
