import { createSelector } from '@ngrx/store';
import { getLoadBoardFeatureState, LoadBoardState } from '../reducers';
import { getCurrentSearch, getLoaded, getLoading, selectors } from '../reducers/load-board-dashboard.reducer';


const getLoadBoardDashboardState = createSelector(getLoadBoardFeatureState, (state: LoadBoardState) => state.dashboard);
const getLoadBoardDashboardEntities = createSelector(getLoadBoardDashboardState, selectors.selectEntities);

export const getLoadBoardDashboardLoads = createSelector(getLoadBoardDashboardEntities, (x) => {
    return Object.keys(x).map(y => x[y]);
});
export const getLoadBoardDashboardLoading = createSelector(getLoadBoardDashboardState, getLoading);
export const getLoadBoardDashboardLoaded = createSelector(getLoadBoardDashboardState, getLoaded);
export const getLoadBoardDashboardCurrentSearch = createSelector(getLoadBoardDashboardState, getCurrentSearch);
