import { LoadCarrierScacData } from 'src/app/shared/models';
import {
  ShippingLoadCarrierScacActions,
  ShippingLoadCarrierScacActionTypes,
  ShippingLoadDetailActions,
  ShippingLoadDetailActionTypes,
} from '../actions';

export interface ShippingLoadCarrierScacState {
  loadsLoading: { [s: string]: boolean };
  loadCarrierScacs: { [s: string]: LoadCarrierScacData[] };
}

const initialState: ShippingLoadCarrierScacState = {
  loadsLoading: {},
  loadCarrierScacs: {},
};

// tslint:disable-next-line:no-use-before-declare
const deleteLoadId = ({ [loadId]: _, ...state }, loadId: string) => state;
export function ShippingLoadCarrierScacReducer(
  state: ShippingLoadCarrierScacState = initialState,
  action: ShippingLoadCarrierScacActions | ShippingLoadDetailActions
): ShippingLoadCarrierScacState {
  switch (action.type) {
    case ShippingLoadCarrierScacActionTypes.Load_Carrier_Scacs_Load: {
      return {
        ...state,
        loadCarrierScacs: deleteLoadId(state.loadCarrierScacs, action.payload.loadId),
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: true } },
      };
    }
    case ShippingLoadCarrierScacActionTypes.Load_Carrier_Scacs_Load_Success: {
      return {
        ...state,
        loadCarrierScacs: { ...state.loadCarrierScacs, ...{ [action.payload.loadId]: action.payload.scacs } },
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } },
      };
    }
    case ShippingLoadCarrierScacActionTypes.Load_Carrier_Scacs_Load_Failure: {
      return { ...state, loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } } };
    }
    case ShippingLoadDetailActionTypes.Delete_Load_Success: {
      return {
        ...state,
        loadCarrierScacs: deleteLoadId(state.loadCarrierScacs, action.payload.loadId),
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } },
      };
    }
    default:
      return state;
  }
}

export const getLoadCarrierScacs = (state: ShippingLoadCarrierScacState) => state.loadCarrierScacs;
export const getLoadsLoading = (state: ShippingLoadCarrierScacState) => state.loadsLoading;
