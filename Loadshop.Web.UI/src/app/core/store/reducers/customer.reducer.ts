import { CustomerActions, CustomerActionTypes } from '../actions';
import { Customer } from '../../../shared/models';

export interface CustomerState {
    loading: boolean;
    entity: Customer;
}

const initialState: CustomerState = {
    loading: false,
    entity: null
};

export function customerReducer(state: CustomerState = initialState, action: CustomerActions): CustomerState {
    switch (action.type) {
        case CustomerActionTypes.Load: {
            return { ...state, entity: null, loading: true };
        }
        case CustomerActionTypes.Load_Success: {
            return { ...state, entity: action.payload, loading: false };
        }
        case CustomerActionTypes.Load_Failure: {
            return { ...state, entity: null, loading: false };
        }
        default:
            return state;
    }
}

export const getLoading = (state: CustomerState) => state.loading;
export const getEntity = (state: CustomerState) => state.entity;
