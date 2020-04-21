import { ShippingLoadCarrierGroupData } from 'src/app/shared/models';
import {
  ShippingLoadCarrierGroupActions,
  ShippingLoadCarrierGroupActionTypes,
  ShippingLoadDetailActions,
  ShippingLoadDetailActionTypes,
} from '../actions';

export interface ShippingLoadCarrierGroupState {
  loadsLoading: { [s: string]: boolean };
  loadCarrierGroups: { [s: string]: ShippingLoadCarrierGroupData[] };
}

const initialState: ShippingLoadCarrierGroupState = {
  loadsLoading: {},
  loadCarrierGroups: {},
};

// tslint:disable-next-line:no-use-before-declare
const deleteLoadId = ({ [loadId]: _, ...state }, loadId: string) => state;
export function ShippingLoadCarrierGroupReducer(
  state: ShippingLoadCarrierGroupState = initialState,
  action: ShippingLoadCarrierGroupActions | ShippingLoadDetailActions
): ShippingLoadCarrierGroupState {
  switch (action.type) {
    case ShippingLoadCarrierGroupActionTypes.Load_Carrier_Groups_Load: {
      return {
        ...state,
        loadCarrierGroups: deleteLoadId(state.loadCarrierGroups, action.payload.loadId),
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: true } },
      };
    }
    case ShippingLoadCarrierGroupActionTypes.Load_Carrier_Groups_Load_Success: {
      return {
        ...state,
        loadCarrierGroups: { ...state.loadCarrierGroups, ...{ [action.payload.loadId]: action.payload.logs } },
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } },
      };
    }
    case ShippingLoadCarrierGroupActionTypes.Load_Carrier_Groups_Load_Failure: {
      return { ...state, loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } } };
    }
    case ShippingLoadDetailActionTypes.Delete_Load_Success: {
      return {
        ...state,
        loadCarrierGroups: deleteLoadId(state.loadCarrierGroups, action.payload.loadId),
        loadsLoading: { ...state.loadsLoading, ...{ [action.payload.loadId]: false } },
      };
    }
    default:
      return state;
  }
}

export const getLoadCarrierGroups = (state: ShippingLoadCarrierGroupState) => state.loadCarrierGroups;
export const getLoadsLoading = (state: ShippingLoadCarrierGroupState) => state.loadsLoading;
