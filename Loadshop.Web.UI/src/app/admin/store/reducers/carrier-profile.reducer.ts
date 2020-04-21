import { CarrierProfile, Carrier } from 'src/app/shared/models';
import { CarrierProfileActions, CarrierProfileActionTypes } from '../actions';

export interface CarrierProfileState {
    loadingCarrierProfile: boolean;
    updatingCarrierProfile: boolean;
    loadingAllCarriers: boolean;
    selectedCarrier: CarrierProfile;
    allCarriers: Carrier[];
}

const initialState: CarrierProfileState = {
    loadingCarrierProfile: false,
    updatingCarrierProfile: false,
    loadingAllCarriers: false,
    selectedCarrier: null,
    allCarriers: []
};

export function CarrierProfileReducer(state: CarrierProfileState = initialState, action: CarrierProfileActions): CarrierProfileState {
    switch (action.type) {
        case CarrierProfileActionTypes.Load: {
            return { ...state, loadingCarrierProfile: true, selectedCarrier: null };
        }

        case CarrierProfileActionTypes.Load_Success: {
            return { ...state, loadingCarrierProfile: false, selectedCarrier: action.payload };
        }

        case CarrierProfileActionTypes.Load_Failure: {
            return { ...state, loadingCarrierProfile: false, selectedCarrier: null };
        }

        case CarrierProfileActionTypes.Update: {
            return { ...state, updatingCarrierProfile: true };
        }

        case CarrierProfileActionTypes.Update_Success: {
            return { ...state, updatingCarrierProfile: false, selectedCarrier: action.payload };
        }

        case CarrierProfileActionTypes.Update_Failure: {
            return { ...state, updatingCarrierProfile: false };
        }
        case CarrierProfileActionTypes.Cancel_Update: {
            return { ...state, selectedCarrier: null };
        }
        case CarrierProfileActionTypes.Load_All: {
            return { ...state, loadingAllCarriers: true };
        }
        case CarrierProfileActionTypes.Load_All_Success: {
            return { ...state, allCarriers: action.payload, loadingAllCarriers: false };
        }
        case CarrierProfileActionTypes.Load_All_Failure: {
            return { ...state, allCarriers: [], loadingAllCarriers: false };
        }

        default: {
            return state;
        }
    }
}
