import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { defaultSearch, LoadView, Search } from '../../../shared/models';
import { LoadBoardDashboardActions, LoadBoardDashboardActionTypes } from '../actions';

export interface LoadBoardDashboardState extends EntityState<LoadView> {
    loaded: boolean;
    loading: boolean;
    currentSearch: Search;
}

export const adapter: EntityAdapter<LoadView> = createEntityAdapter<LoadView>({
    selectId: (x) => x.loadId
});

const initialState: LoadBoardDashboardState = adapter.getInitialState({
    loaded: false,
    loading: false,
    currentSearch: { ...defaultSearch }
});

export function loadboardDashboardReducer(state: LoadBoardDashboardState = initialState,
    action: LoadBoardDashboardActions): LoadBoardDashboardState {

    switch (action.type) {
        case LoadBoardDashboardActionTypes.Load: {
            return Object.assign({}, state, { loading: true });
        }
        case LoadBoardDashboardActionTypes.Load_Success: {
            return adapter.addAll(action.payload, { ...state, loading: false });
        }
        case LoadBoardDashboardActionTypes.Search_Add:
        case LoadBoardDashboardActionTypes.Search_Clear: {
            return { ...state, currentSearch: action.payload };
        }
        default:
            return state;
    }
}

export const getLoaded = (state: LoadBoardDashboardState) => state.loaded;
export const getLoading = (state: LoadBoardDashboardState) => state.loading;
export const getCurrentSearch = (state: LoadBoardDashboardState) => state.currentSearch;
export const selectors = adapter.getSelectors();
