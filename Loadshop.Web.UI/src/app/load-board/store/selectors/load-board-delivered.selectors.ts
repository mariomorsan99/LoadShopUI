import { createSelector } from '@ngrx/store';
import { getLoadBoardFeatureState, LoadBoardState } from '../reducers';
import {
  getErrorLoadId,
  getLoaded,
  getLoading,
  getPhoneError,
  getSavingLoadId,
  getTruckError,
  selectors,
  getDeliveredTotalRecords,
  getDeliveredQueryHelper,
} from '../reducers/load-board-delivered.reducer';

const getLoadBoardDeliveredState = createSelector(getLoadBoardFeatureState, (state: LoadBoardState) => state.delivered);
// TODO: Switch this to (x) => selectors.selectAll(x)
const getLoadBoardDeliveredEntities = createSelector(getLoadBoardDeliveredState, selectors.selectEntities);

export const getLoadBoardDeliveredLoads = createSelector(getLoadBoardDeliveredEntities, x => {
  // TODO: Remove this and replace with getLoadBoardDeliveredEntities and remove distinctUntilChanged where this is listened to
  return Object.keys(x).map(y => x[y]);
});
export const getLoadBoardDeliveredLoading = createSelector(getLoadBoardDeliveredState, getLoading);
export const getLoadBoardDeliveredLoaded = createSelector(getLoadBoardDeliveredState, getLoaded);
export const getLoadBoardDeliveredSavingLoadId = createSelector(getLoadBoardDeliveredState, getSavingLoadId);
export const getLoadBoardDeliveredErrorLoadId = createSelector(getLoadBoardDeliveredState, getErrorLoadId);
export const getLoadBoardDeliveredPhoneError = createSelector(getLoadBoardDeliveredState, getPhoneError);
export const getLoadBoardDeliveredTruckError = createSelector(getLoadBoardDeliveredState, getTruckError);
export const getLoadBoardDeliveredTotalRecords = createSelector(getLoadBoardDeliveredState, getDeliveredTotalRecords);
export const getLoadBoardDeliveredQueryHelper = createSelector(getLoadBoardDeliveredState, getDeliveredQueryHelper);
