import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { ShippingDeliveredActions, ShippingDeliveredActionTypes } from '../actions';
import { PageableQueryHelper } from 'src/app/shared/utilities';
import { PageableResult, DefaultPageableResult } from 'src/app/shared/models';

export interface ShippingDeliveredState {
  loading: boolean;
  deliveredLoads: PageableResult<ShippingLoadView>;
  queryHelper: PageableQueryHelper;
}
const initialState: ShippingDeliveredState = {
  loading: false,
  deliveredLoads: DefaultPageableResult.Create<ShippingLoadView>(),
  queryHelper: PageableQueryHelper.default(),
};

export function ShippingDeliveredReducer(
  state: ShippingDeliveredState = initialState,
  action: ShippingDeliveredActions
): ShippingDeliveredState {
  switch (action.type) {
    case ShippingDeliveredActionTypes.Load_Shipping_Delivered: {
      return { ...state, loading: true, deliveredLoads: { ...state.deliveredLoads, data: [] } };
    }
    case ShippingDeliveredActionTypes.Load_Shipping_Delivered_Success: {
      return { ...state, loading: false, deliveredLoads: action.payload };
    }
    case ShippingDeliveredActionTypes.Load_Shipping_Delivered_Failure: {
      return { ...state, loading: false };
    }
    case ShippingDeliveredActionTypes.Load_Shipping_Query_Update: {
      return { ...state, queryHelper: action.payload };
    }
    default:
      return state;
  }
}

export const getDeliveredLoading = (state: ShippingDeliveredState) => state.loading;
export const getDeliveredLoads = (state: ShippingDeliveredState) => state.deliveredLoads;
