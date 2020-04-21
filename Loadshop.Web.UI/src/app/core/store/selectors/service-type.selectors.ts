import { createSelector } from '@ngrx/store';
import { CoreState } from '../reducers';
import { getEntities, getLoading } from '../reducers/service-type.reducer';

export const getServiceTypeState = (state: CoreState) => state.serviceType;
export const getServiceTypes = createSelector(getServiceTypeState, getEntities);
export const getLoadingServiceTypes = createSelector(getServiceTypeState, getLoading);
