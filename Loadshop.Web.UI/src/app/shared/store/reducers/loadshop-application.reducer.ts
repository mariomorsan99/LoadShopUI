import { LoadshopApplicationActions, LoadshopApplicationActionTypes } from '../actions';

export interface LoadshopApplicationState {
  ready: boolean;
}
const initialState: LoadshopApplicationState = {
  ready: false,
};

export function LoadshopApplicationReducer(
  state: LoadshopApplicationState = initialState,
  action: LoadshopApplicationActions
): LoadshopApplicationState {
  switch (action.type) {
    case LoadshopApplicationActionTypes.LoadshopStart: {
      return { ...state, ready: true };
    }
    default:
      return state;
  }
}
