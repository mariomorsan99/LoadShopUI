import { createSelector } from '@ngrx/store';
import { getUserFeatureState, UserState } from '../reducers';
import { selectors } from '../reducers/user-lane.reducer';

const getUserLaneState = createSelector(getUserFeatureState, (state: UserState) => state.userLanes);

const getUserLanes = createSelector(getUserLaneState, selectors.selectEntities);
// export const getCustomerLaneEntities = createSelector(getCustomerLaneState, selectors.selectAll);
// TODO: change back to (x) => selectors.selectAll(x)
export const getUserLaneEntities = createSelector(getUserLanes, x => {
  return Object.keys(x).map(y => x[y]);
});

export const getActiveUserLaneEntities = createSelector(getUserLaneEntities, lanes => {
  return lanes.filter(l => l.display);
});

export const getUserLaneSelectedLane = createSelector(getUserLaneState, x => {
  return x.selectedUserLane;
});

export const getUserLaneLoading = createSelector(getUserLaneState, x => {
  return x.loading;
});
