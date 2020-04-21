import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { ShippingPostedActionTypes } from '../actions';
import { ShippingPostedActions } from '../actions/shipping-posted.actions';
import { PageableQueryHelper } from 'src/app/shared/utilities';
import { PageableResult, DefaultPageableResult } from 'src/app/shared/models';

export interface ShippingPostedState {
  loading: boolean;
  postedLoads: PageableResult<ShippingLoadView>;
  queryHelper: PageableQueryHelper;
}
const initialState: ShippingPostedState = {
  loading: false,
  postedLoads: DefaultPageableResult.Create<ShippingLoadView>(),
  queryHelper: PageableQueryHelper.default(),
};

export function ShippingPostedReducer(state: ShippingPostedState = initialState, action: ShippingPostedActions): ShippingPostedState {
  switch (action.type) {
    case ShippingPostedActionTypes.Load_Shipping_Posted: {
      return { ...state, loading: true, postedLoads: { ...state.postedLoads, data: [] } };
    }
    case ShippingPostedActionTypes.Load_Shipping_Posted_Success: {
      return { ...state, loading: false, postedLoads: action.payload };
    }
    case ShippingPostedActionTypes.Load_Shipping_Posted_Failure: {
      return { ...state, loading: false };
    }

    case ShippingPostedActionTypes.Update_Shipping_Posted_Query: {
      return { ...state, queryHelper: action.payload };
    }
    default:
      return state;
  }
}

export const getPostedLoading = (state: ShippingPostedState) => state.loading;
export const getPostedLoads = (state: ShippingPostedState) => {
  return state.postedLoads;
};
