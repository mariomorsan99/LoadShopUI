import { CommodityActions, CommodityActionTypes } from '../actions';
import { Commodity } from '../../../shared/models';

export interface CommodityState {
    loading: boolean;
    entities: Commodity[];
}

const initialState: CommodityState = {
    loading: false,
    entities: null
};

export function commodityReducer(state: CommodityState = initialState, action: CommodityActions): CommodityState {
    switch (action.type) {
        case CommodityActionTypes.Load: {
            return { ...state, loading: true };
        }
        case CommodityActionTypes.Load_Success: {
            return { ...state, entities: action.payload, loading: false };
        }
        case CommodityActionTypes.Load_Failure: {
            return { ...state, loading: false };
        }
        default:
            return state;
    }
}

export const getLoading = (state: CommodityState) => state.loading;
export const getEntities = (state: CommodityState) => state.entities;
