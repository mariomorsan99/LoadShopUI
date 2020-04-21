import { createSelector } from '@ngrx/store';
import { AdminState, getAdminFeatureState } from '../reducers';
import { LoadCarrierGroupCarrierState, selectors } from '../reducers/load-carrier-group-carrier.reducer';

const getLoadCarrierGroupCarrierState = createSelector(getAdminFeatureState, (state: AdminState) => state.loadCarrierGroupCarriers);

export const getLoadCarrierGroupCarriers = createSelector(getLoadCarrierGroupCarrierState, (state: LoadCarrierGroupCarrierState) =>
  selectors.selectAll(state)
); // must only call selectAll with the state or it will detect argument changes and different object references each time
export const getLoadingLoadCarrierGroupCarriers = createSelector(
  getLoadCarrierGroupCarrierState,
  (state: LoadCarrierGroupCarrierState) => state.loading
);
export const getSavingLoadCarrierGroupCarriers = createSelector(
  getLoadCarrierGroupCarrierState,
  (state: LoadCarrierGroupCarrierState) => state.saving
);
