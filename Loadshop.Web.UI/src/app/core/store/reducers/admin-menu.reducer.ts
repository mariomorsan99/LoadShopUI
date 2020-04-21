import { MenuItem } from 'primeng/api';
import { VisibilityBadge } from 'src/app/shared/models';
import { AdminMenuActionTypes, AdminMenuActions } from '../actions';

export interface AdminMenuState {
    loading: boolean;
    loaded: boolean;
    entities: MenuItem[];
    visibilityBadge: VisibilityBadge;
}

const initialState: AdminMenuState = {
    loading: false,
    loaded: false,
    entities: [],
    visibilityBadge: null
};

export function adminMenuReducer(state: AdminMenuState = initialState, action: AdminMenuActions): AdminMenuState {
    switch (action.type) {
        case AdminMenuActionTypes.Load: {
            return Object.assign({}, state, {
                loading: true
            });
        }
        case AdminMenuActionTypes.Load_Success: {
            const data = action.payload;
            return Object.assign({}, state, {
                loading: false,
                loaded: true,
                entities: data
            });
        }
        case AdminMenuActionTypes.Load_Failure: {
            return Object.assign({}, state, {
                loading: false,
                loaded: false
            });
        }
        default:
            return state;
    }
}

export const getLoading = (state: AdminMenuState) => state.loading;
export const getLoaded = (state: AdminMenuState) => state.loaded;
export const getEntities = (state: AdminMenuState) => state.entities;
