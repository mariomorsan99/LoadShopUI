import { createSelector } from '@ngrx/store';
import { getShippingFeatureState, ShippingState } from '../reducers';
import { getDeliveredLoading, getDeliveredLoads } from '../reducers/shipping-delivered.reducer';

const getDeliveredState = createSelector(getShippingFeatureState, (state: ShippingState) => state.shippingDelivered);

export const getShippingDeliveredLoads = createSelector(getDeliveredState, getDeliveredLoads);
export const getShippingDeliveredLoading = createSelector(getDeliveredState, getDeliveredLoading);

export const getShippingDeliveredQueryHelper = createSelector(getDeliveredState, state => state.queryHelper);
