import { T2GLoadStatus, LoadStatusTransaction, ValidationProblemDetails } from 'src/app/shared/models';
import { LoadStatusActions, LoadStatusActionTypes } from '../actions';

export interface LoadStatusState {
    loadingCount: number;
    loadingAll: boolean;
    tops2GoStatus: T2GLoadStatus;
    allTops2GoStatuses: T2GLoadStatus[];
    loadshopStatus: LoadStatusTransaction;
    saving: boolean;
    problemDetails: ValidationProblemDetails;
}

const initialState: LoadStatusState = {
    loadingCount: 0,
    loadingAll: false,
    tops2GoStatus: null,
    allTops2GoStatuses: null,
    loadshopStatus: null,
    saving: false,
    problemDetails: null,
};

export function loadStatusReducer(state: LoadStatusState = initialState, action: LoadStatusActions): LoadStatusState {
    switch (action.type) {
        case LoadStatusActionTypes.Load: {
            return { ...state, tops2GoStatus: null, loadshopStatus: null, loadingCount: 2, problemDetails: null };
        }
        case LoadStatusActionTypes.Tops2GoStatusLoadSuccess: {
            return { ...state, tops2GoStatus: action.payload, loadingCount: state.loadingCount - 1 };
        }
        case LoadStatusActionTypes.Tops2GoStatusLoadFailure: {
            return { ...state, loadingCount: state.loadingCount - 1 };
        }
        case LoadStatusActionTypes.LoadshopStatusLoadSuccess: {
            return { ...state, loadshopStatus: action.payload, loadingCount: state.loadingCount - 1 };
        }
        case LoadStatusActionTypes.LoadshopStatusLoadFailure: {
            return { ...state, loadingCount: state.loadingCount - 1 };
        }
        case LoadStatusActionTypes.LoadshopInTransitStatusSave:
        case LoadStatusActionTypes.LoadshopStopStatusSave: {
            return { ...state, saving: true, problemDetails: null };
        }
        case LoadStatusActionTypes.LoadshopLoadStatusSaveSuccess: {
            return { ...state, saving: false, loadshopStatus: action.payload, problemDetails: null };
        }
        case LoadStatusActionTypes.LoadshopLoadStatusSaveFailure: {
            return { ...state, saving: false, problemDetails: action.payload.error };
        }
        case LoadStatusActionTypes.LoadAll: {
            return { ...state, allTops2GoStatuses: null, loadingAll: true };
        }
        case LoadStatusActionTypes.LoadAllSuccess: {
            return { ...state, allTops2GoStatuses: action.payload, loadingAll: false };
        }
        case LoadStatusActionTypes.LoadAllFailure: {
            return { ...state, loadingAll: false };
        }
        default:
            return state;
    }
}

export const getLoading = (state: LoadStatusState) => state.loadingCount > 0;
export const getSaving = (state: LoadStatusState) => state.saving;
export const getLoadingAll = (state: LoadStatusState) => state.loadingAll;
