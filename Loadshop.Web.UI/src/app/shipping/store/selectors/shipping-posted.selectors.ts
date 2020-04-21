import { createSelector } from '@ngrx/store';
import { getShippingFeatureState, ShippingState } from '../reducers';
import { getPostedLoading, getPostedLoads } from '../reducers/shipping-posted.reducer';

const getPostedState = createSelector(getShippingFeatureState, (state: ShippingState) => state.shippingPosted);

export const getShippingPostedLoads = createSelector(getPostedState, getPostedLoads);
export const getShippingPostedLoading = createSelector(getPostedState, getPostedLoading);

export const getShippingPostedQueryHelper = createSelector(getPostedState, state => state.queryHelper);
