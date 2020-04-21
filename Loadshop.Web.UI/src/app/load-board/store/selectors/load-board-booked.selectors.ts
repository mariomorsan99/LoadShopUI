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
  getBookedTotalRecords,
  getBookedQueryHelper,
} from '../reducers/load-board-booked.reducer';

const getLoadBoardBookedState = createSelector(getLoadBoardFeatureState, (state: LoadBoardState) => state.booked);
// TODO: Switch this to (x) => selectors.selectAll(x)
const getLoadBoardBookedEntities = createSelector(getLoadBoardBookedState, selectors.selectEntities);

export const getLoadBoardBookedLoads = createSelector(getLoadBoardBookedEntities, x => {
  // TODO: Remove this and replace with getLoadBoardBookedEntities and remove distinctUntilChanged where this is listened to
  return Object.keys(x).map(y => x[y]);
});
export const getLoadBoardBookedLoading = createSelector(getLoadBoardBookedState, getLoading);
export const getLoadBoardBookedLoaded = createSelector(getLoadBoardBookedState, getLoaded);
export const getLoadBoardBookedSavingLoadId = createSelector(getLoadBoardBookedState, getSavingLoadId);
export const getLoadBoardBookedErrorLoadId = createSelector(getLoadBoardBookedState, getErrorLoadId);
export const getLoadBoardBookedPhoneError = createSelector(getLoadBoardBookedState, getPhoneError);
export const getLoadBoardBookedTruckError = createSelector(getLoadBoardBookedState, getTruckError);
export const getLoadBoardBookedTotalRecords = createSelector(getLoadBoardBookedState, getBookedTotalRecords);
export const getLoadBoardBookedQueryHelper = createSelector(getLoadBoardBookedState, getBookedQueryHelper);
