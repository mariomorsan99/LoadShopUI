import { createSelector } from '@ngrx/store';
import { AdminState, getAdminFeatureState } from '../reducers';
import { selectors, ShipperProfileState } from '../reducers/shipper-profile.reducer';

const getShipperProfileState = createSelector(getAdminFeatureState, (state: AdminState) => state.shipperProfile);

export const getShippers = createSelector(getShipperProfileState, (state: ShipperProfileState) => selectors.selectAll(state));
export const getLoadingShippers = createSelector(getShipperProfileState, (state: ShipperProfileState) => state.loadingShippers);

export const getSelectedShipper = createSelector(getShipperProfileState, (state: ShipperProfileState) => state.selectedShipper);
export const getLoadingSelectedShipper = createSelector(getShipperProfileState, (state: ShipperProfileState) => state.loadingShipper);

export const getSavingCustomer = createSelector(getShipperProfileState, (state: ShipperProfileState) => state.savingCustomer);
export const getSaveCustomerSucceeded = createSelector(getShipperProfileState, (state: ShipperProfileState) => state.saveCustomerSucceeded);

export const getShipperMappings = createSelector(getShipperProfileState, (state: ShipperProfileState) => state.shipperMappings);
export const loadingShipperMappings = createSelector(getShipperProfileState, (state: ShipperProfileState) => state.loadingShipperMappings);
export const savingShipperMapping = createSelector(getShipperProfileState, (state: ShipperProfileState) => state.savingMapping);

export const getSourceSystemOwners = createSelector(getShipperProfileState, (state: ShipperProfileState) => state.sourceSystemOwners);
export const loadingSourceSystemOwners =
    createSelector(getShipperProfileState, (state: ShipperProfileState) => state.loadingSourceSystemOwners);
