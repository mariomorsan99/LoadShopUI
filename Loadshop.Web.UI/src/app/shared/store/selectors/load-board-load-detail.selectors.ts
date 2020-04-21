import { createSelector } from '@ngrx/store';

import { getSharedFeatureState } from '../reducers';
import { getLoading, getLoaded, getSelectedEntity } from '../reducers/load-board-load-detail.reducer';
import { SharedState } from '../reducers';

const getLoadBoardLoadDetailState = createSelector(getSharedFeatureState, (state: SharedState) => state.loadDetail);

export const getLoadBoardLoadDetailLoading = createSelector(getLoadBoardLoadDetailState, getLoading);
export const getLoadBoardLoadDetailLoaded = createSelector(getLoadBoardLoadDetailState, getLoaded);
export const getLoadBoardSelectedLoad = createSelector(getLoadBoardLoadDetailState, getSelectedEntity);
