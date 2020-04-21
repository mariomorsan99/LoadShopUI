import { TransportationMode } from '../../../shared/models';
import { TransportationModeActions, TransportationModeActionTypes } from '../actions';

export interface TransportationModeState {
  loading: boolean;
  entities: TransportationMode[];
}

const initialState: TransportationModeState = {
  loading: false,
  entities: [],
};

export function transportationModeReducer(
  state: TransportationModeState = initialState,
  action: TransportationModeActions
): TransportationModeState {
  switch (action.type) {
    case TransportationModeActionTypes.Load: {
      return { ...state, loading: true };
    }
    case TransportationModeActionTypes.Load_Success: {
      return { ...state, entities: action.payload, loading: false };
    }
    case TransportationModeActionTypes.Load_Failure: {
      return { ...state, loading: false };
    }
    default:
      return state;
  }
}

export const getLoading = (state: TransportationModeState) => state.loading;
export const getEntities = (state: TransportationModeState) => state.entities;
