import { BrowserActions, BrowserActionTypes } from '../actions';

export interface BrowserState {
    isMobile: boolean;
}

const initialState: BrowserState = {
    isMobile: false
};

export function browserReducer(state: BrowserState = initialState, action: BrowserActions): BrowserState {
    switch (action.type) {
        case BrowserActionTypes.SetIsMobile: {
            return { ...state, isMobile: action.payload.isMobile };
        }
        default:
            return state;
    }
}

export const getIsMobile = (state: BrowserState) => state.isMobile;
