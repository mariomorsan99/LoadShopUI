import { LoadCarrierScacRestrictionData } from 'src/app/shared/models';
import {
  ShippingLoadCarrierScacRestrictionActions,
  ShippingLoadCarrierScacRestrictionActionTypes,
  ShippingLoadDetailActions,
  ShippingLoadDetailActionTypes,
} from '../actions';

export interface ShippingLoadCarrierScacRestrictionState {
  loadsLoading: { [s: string]: boolean };
  loadCarrierScacRestrictions: { [s: string]: LoadCarrierScacRestrictionData[] };
}

const initialState: ShippingLoadCarrierScacRestrictionState = {
  loadsLoading: {},
  loadCarrierScacRestrictions: {},
};

// tslint:disable-next-line:no-use-before-declare
const deleteLoadId = ({ [loadId]: _, ...state }, loadId: string) => state;
export function ShippingLoadCarrierScacRestrictionReducer(
  state: ShippingLoadCarrierScacRestrictionState = initialState,
  action: ShippingLoadCarrierScacRestrictionActions | ShippingLoadDetailActions
): ShippingLoadCarrierScacRestrictionState {
  switch (action.type) {
    case ShippingLoadCarrierScacRestrictionActionTypes.Load_Carrier_Scac_Restrictions_Load: {
      return {
        ...state,
        loadCarrierScacRestrictions: deleteLoadId(state.loadCarrierScacRestrictions, action.payload.loadId),
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: true } },
      };
    }
    case ShippingLoadCarrierScacRestrictionActionTypes.Load_Carrier_Scac_Restrictions_Load_Success: {
      return {
        ...state,
        loadCarrierScacRestrictions: { ...state.loadCarrierScacRestrictions, ...{ [action.payload.loadId]: action.payload.scacs } },
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } },
      };
    }
    case ShippingLoadCarrierScacRestrictionActionTypes.Load_Carrier_Scac_Restrictions_Load_Failure: {
      return { ...state, loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } } };
    }
    case ShippingLoadDetailActionTypes.Delete_Load_Success: {
      return {
        ...state,
        loadCarrierScacRestrictions: deleteLoadId(state.loadCarrierScacRestrictions, action.payload.loadId),
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } },
      };
    }
    default:
      return state;
  }
}

export const getLoadCarrierScacRestrictions = (state: ShippingLoadCarrierScacRestrictionState) => state.loadCarrierScacRestrictions;
export const getLoadsLoading = (state: ShippingLoadCarrierScacRestrictionState) => state.loadsLoading;
