import { createSelector } from '@ngrx/store';
import { AdminState, getAdminFeatureState } from '../reducers';
import { CarrierProfileState } from '../reducers/carrier-profile.reducer';

const getCarrierProfileState = createSelector(getAdminFeatureState,
    (state: AdminState) => state.carrierProfile);

export const getCarrierProfile = createSelector(getCarrierProfileState, (state: CarrierProfileState) => state.selectedCarrier);

export const getCarrierProfileLoading = createSelector(getCarrierProfileState, (state: CarrierProfileState) => state.loadingCarrierProfile);
export const getCarrierProfileUpdating =
    createSelector(getCarrierProfileState, (state: CarrierProfileState) => state.updatingCarrierProfile);

export const getAllCarriers = createSelector(getCarrierProfileState, (state: CarrierProfileState) => state.allCarriers);
export const getAllCarriersLoading = createSelector(getCarrierProfileState, (state: CarrierProfileState) => state.loadingAllCarriers);
