import { CarrierActions, CarrierActionTypes } from '../actions';
import { Carrier, CarrierCarrierScacGroup } from '../../../shared/models';

export interface CarrierState {
    loading: boolean;
    entities: Carrier[];
    allCarriers: CarrierCarrierScacGroup[];
    allCarriersLoading: boolean;
}

const initialState: CarrierState = {
    loading: false,
    entities: null,
    allCarriers: null,
    allCarriersLoading: false
};

export function carrierReducer(state: CarrierState = initialState, action: CarrierActions): CarrierState {
    switch (action.type) {
        case CarrierActionTypes.Load: {
            return { ...state, loading: true };
        }
        case CarrierActionTypes.Load_Success: {
            return { ...state, entities: action.payload, loading: false };
        }
        case CarrierActionTypes.Load_Failure: {
            return { ...state, loading: false };
        }
        case CarrierActionTypes.CarrierCarrierScacLoad: {
            return { ...state, allCarriersLoading: true };
        }
        case CarrierActionTypes.CarrierCarrierScacLoad_Success: {
            return { ...state, allCarriers: action.payload, allCarriersLoading: false };
        }
        case CarrierActionTypes.CarrierCarrierScacLoad_Failure: {
            return { ...state, allCarriersLoading: false };
        }
        default:
            return state;
    }
}

export const getLoading = (state: CarrierState) => state.loading;
export const getEntities = (state: CarrierState) => state.entities;

export const getAllCarriersLoading = (state: CarrierState) => state.allCarriersLoading;
export const getAllCarriers = (state: CarrierState) => state.allCarriers;
