import { EquipmentActions, EquipmentActionTypes } from '../actions';
import { Equipment } from '../../../shared/models';

export interface EquipmentState {
    loading: boolean;
    loaded: boolean;
    entities: Equipment[];
}

const initialState: EquipmentState = {
    loading: false,
    loaded: false,
    entities: []
};

export function equipmentReducer(state: EquipmentState = initialState, action: EquipmentActions): EquipmentState {
    switch (action.type) {
        case EquipmentActionTypes.Load: {
            return { ...state, loading: true, loaded: false };
        }
        case EquipmentActionTypes.Load_Success: {
            return { ...state, entities: action.payload, loading: false, loaded: true };
        }
        case EquipmentActionTypes.Load_Failure: {
            return { ...state, loading: false };
        }
        default:
            return state;
    }
}

export const getLoading = (state: EquipmentState) => state.loading;
export const getLoaded = (state: EquipmentState) => state.loaded;
export const getEntities = (state: EquipmentState) => state.entities;
