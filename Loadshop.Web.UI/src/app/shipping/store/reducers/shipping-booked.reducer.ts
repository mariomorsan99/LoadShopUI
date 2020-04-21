import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { ShippingBookedActions, ShippingBookedActionTypes } from '../actions';
import { PageableQueryHelper } from 'src/app/shared/utilities';
import { PageableResult, DefaultPageableResult } from 'src/app/shared/models';

export interface ShippingBookedState {
  loading: boolean;
  bookedLoads: PageableResult<ShippingLoadView>;
  queryHelper: PageableQueryHelper;
}
const initialState: ShippingBookedState = {
  loading: false,
  bookedLoads: DefaultPageableResult.Create<ShippingLoadView>(),
  queryHelper: PageableQueryHelper.default(),
};

export function ShippingBookedReducer(state: ShippingBookedState = initialState, action: ShippingBookedActions): ShippingBookedState {
  switch (action.type) {
    case ShippingBookedActionTypes.Load_Shipping_Booked: {
      return { ...state, loading: true, bookedLoads: { ...state.bookedLoads, data: [] } };
    }
    case ShippingBookedActionTypes.Load_Shipping_Booked_Success: {
      return { ...state, loading: false, bookedLoads: action.payload };
    }
    case ShippingBookedActionTypes.Load_Shipping_Booked_Failure: {
      return { ...state, loading: false };
    }
    case ShippingBookedActionTypes.Update_Shipping_Booked_Query: {
      return { ...state, queryHelper: action.payload };
    }
    default:
      return state;
  }
}

export const getBookedLoading = (state: ShippingBookedState) => state.loading;
export const getBookedLoads = (state: ShippingBookedState) => {
  return state.bookedLoads;
};
