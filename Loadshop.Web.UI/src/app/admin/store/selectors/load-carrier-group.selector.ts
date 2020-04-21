import { createSelector } from '@ngrx/store';
import { AdminState, getAdminFeatureState } from '../reducers';
import { LoadCarrierGroupState, selectors } from '../reducers/load-carrier-group.reducer';

const getLoadCarrierGroupState = createSelector(getAdminFeatureState, (state: AdminState) => state.loadCarrierGroups);

export const getLoadCarrierGroups = createSelector(getLoadCarrierGroupState, (state: LoadCarrierGroupState) => selectors.selectAll(state));
export const getLoadingLoadCarrierGroups = createSelector(
  getLoadCarrierGroupState,
  (state: LoadCarrierGroupState) => state.loadingCarrierGroups
);

export const getSelectedLoadCarrierGroup = createSelector(
  getLoadCarrierGroupState,
  (state: LoadCarrierGroupState) => state.selectedLoadCarrierGroup
);
export const getLoadingSelectedLoadCarrierGroup = createSelector(
  getLoadCarrierGroupState,
  (state: LoadCarrierGroupState) => state.loadingSelectedCarrierGroup
);

export const getSavingLoadCarrierGroup = createSelector(
  getLoadCarrierGroupState,
  (state: LoadCarrierGroupState) => state.savingCarrierGroup
);
export const getSaveLoadCarrierGroupSucceeded = createSelector(
  getLoadCarrierGroupState,
  (state: LoadCarrierGroupState) => state.saveCarrierGroupSucceeded
);

export const getSaveLoadCarrierGroupProblemDetails = createSelector(
  getLoadCarrierGroupState,
  (state: LoadCarrierGroupState) => state.saveCarrierGroupProblemDetails
);

export const getLoadCarrierGroupTypes = createSelector(
  getLoadCarrierGroupState,
  (state: LoadCarrierGroupState) => state.loadCarrierGroupTypes
);
export const getLoadCarrierGroupTypesLoading = createSelector(
  getLoadCarrierGroupState,
  (state: LoadCarrierGroupState) => state.loadCarrierGroupTypesLoading
);
