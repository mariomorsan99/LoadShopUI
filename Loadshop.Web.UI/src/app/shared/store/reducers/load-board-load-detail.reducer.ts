import { LoadBoardLoadDetailActionTypes, LoadBoardLoadDetailActions } from '../actions/';
import { LoadDetail } from '../../models';

export interface LoadBoardLoadDetailState {
    loaded: boolean;
    loading: boolean;
    selectedEntity: LoadDetail;
}
const initialState: LoadBoardLoadDetailState = {
    loaded: false,
    loading: false,
    selectedEntity: null
};

export function LoadBoardLoadDetailReducer(state: LoadBoardLoadDetailState = initialState, action: LoadBoardLoadDetailActions)
    : LoadBoardLoadDetailState {
    switch (action.type) {
        case LoadBoardLoadDetailActionTypes.Load_Success: {
            return Object.assign({}, state, {
                selectedEntity: Object.assign({}, action.payload, {
                    loadTransaction: Object.assign({}, action.payload.loadTransaction)
                })
            });
        }
        case LoadBoardLoadDetailActionTypes.Load: {
            return Object.assign({}, state, {
                selectedEntity: null
            });
        }
        default:
            return state;
    }
}

export const getLoaded = (state: LoadBoardLoadDetailState) => state.loaded;
export const getLoading = (state: LoadBoardLoadDetailState) => state.loading;
export const getSelectedEntity = (state: LoadBoardLoadDetailState) => state.selectedEntity;
