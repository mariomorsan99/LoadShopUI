import { ServiceTypeActions, ServiceTypeActionTypes } from '../actions';
import { ServiceType } from '../../../shared/models';

export interface ServiceTypeState {
    loading: boolean;
    entities: ServiceType[];
}

const initialState: ServiceTypeState = {
    loading: false,
    entities: []
};

export function serviceTypeReducer(state: ServiceTypeState = initialState, action: ServiceTypeActions): ServiceTypeState {
    switch (action.type) {
        case ServiceTypeActionTypes.Load: {
            return { ...state, loading: true };
        }
        case ServiceTypeActionTypes.Load_Success: {
            return { ...state, entities: action.payload, loading: false };
        }
        case ServiceTypeActionTypes.Load_Failure: {
            return { ...state, loading: false };
        }
        default:
            return state;
    }
}

export const getLoading = (state: ServiceTypeState) => state.loading;
export const getEntities = (state: ServiceTypeState) => state.entities;
