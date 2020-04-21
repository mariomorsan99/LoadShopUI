import { createSelector } from '@ngrx/store';
import { getShippingFeatureState, ShippingState } from '../reducers';
import { getBookedLoading, getBookedLoads } from '../reducers/shipping-booked.reducer';

const getBookedState = createSelector(getShippingFeatureState, (state: ShippingState) => state.shippingBooked);

export const getShippingBookedLoads = createSelector(getBookedState, getBookedLoads);
export const getShippingBookedLoading = createSelector(getBookedState, getBookedLoading);

export const getShippingBookedQueryHelper = createSelector(getBookedState, state => state.queryHelper);
