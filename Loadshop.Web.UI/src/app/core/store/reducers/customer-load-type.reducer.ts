import { CustomerLoadTypeActions, CustomerLoadTypeActionTypes } from '../actions';
import { CustomerLoadType } from '../../../shared/models';

export interface CustomerLoadTypeState {
    loading: boolean;
    entities: CustomerLoadType[];
}

const initialState: CustomerLoadTypeState = {
    loading: false,
    entities: null
};

export function customerLoadTypeReducer(state: CustomerLoadTypeState = initialState,
  action: CustomerLoadTypeActions): CustomerLoadTypeState {
    switch (action.type) {
        case CustomerLoadTypeActionTypes.Load: {
            return { ...state, loading: true };
        }
        case CustomerLoadTypeActionTypes.Load_Success: {
            return { ...state, entities: action.payload, loading: false };
        }
        case CustomerLoadTypeActionTypes.Load_Failure: {
            return { ...state, loading: false };
        }
        default:
            return state;
    }
}

export const getLoading = (state: CustomerLoadTypeState) => state.loading;
export const getEntities = (state: CustomerLoadTypeState) => state.entities;
