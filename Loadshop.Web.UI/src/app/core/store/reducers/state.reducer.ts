import { StateActions, StateActionTypes } from '../actions';
import { State } from '../../../shared/models';

export interface StateState {
    loading: boolean;
    loaded: boolean;
    entities: State[];
}

const initialState: StateState = {
    loading: false,
    loaded: false,
    entities: []
};

export function stateReducer(state: StateState = initialState, action: StateActions): StateState {
    switch (action.type) {
        case StateActionTypes.Load: {
            return { ...state, loading: true, loaded: false };
        }
        case StateActionTypes.Load_Success: {
            return { ...state, entities: action.payload, loading: false, loaded: true };
        }
        case StateActionTypes.Load_Failure: {
            return { ...state, loading: false };
        }
        default:
            return state;
    }
}

export const getLoading = (state: StateState) => state.loading;
export const getLoaded = (state: StateState) => state.loaded;
export const getEntities = (state: StateState) => state.entities;
