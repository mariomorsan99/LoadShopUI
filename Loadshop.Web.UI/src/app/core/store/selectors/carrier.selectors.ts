import { createSelector } from '@ngrx/store';

import { CoreState } from '../reducers';
import { getEntities, getLoading, getAllCarriers, getAllCarriersLoading } from '../reducers/carrier.reducer';

export const getCarrierState = (state: CoreState) => state.carriers;

export const getCarriers = createSelector(getCarrierState, getEntities);
export const getLoadingCarriers = createSelector(getCarrierState, getLoading);

export const getAllCarrierGroups = createSelector(getCarrierState, getAllCarriers);
export const getLoadingAllCarrierGroups = createSelector(getCarrierState, getAllCarriersLoading);
