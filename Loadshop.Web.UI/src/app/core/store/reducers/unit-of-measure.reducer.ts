import { UnitOfMeasureActions, UnitOfMeasureActionTypes } from '../actions';
import { UnitOfMeasure } from '../../../shared/models';

export interface UnitOfMeasureState {
    loading: boolean;
    entities: UnitOfMeasure[];
}

const initialState: UnitOfMeasureState = {
    loading: false,
    entities: []
};

export function unitOfMeasureReducer(state: UnitOfMeasureState = initialState, action: UnitOfMeasureActions): UnitOfMeasureState {
    switch (action.type) {
        case UnitOfMeasureActionTypes.Load: {
            return { ...state, loading: true };
        }
        case UnitOfMeasureActionTypes.Load_Success: {
            return { ...state, entities: action.payload, loading: false };
        }
        case UnitOfMeasureActionTypes.Load_Failure: {
            return { ...state, loading: false };
        }
        default:
            return state;
    }
}

export const getLoading = (state: UnitOfMeasureState) => state.loading;
export const getEntities = (state: UnitOfMeasureState) => state.entities;
