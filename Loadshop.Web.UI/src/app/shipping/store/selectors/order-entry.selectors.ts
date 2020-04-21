import { createSelector } from '@ngrx/store';
import { OrderEntryState, getLoading, getProblemDetails, getForm, getSavedId } from '../reducers/order-entry.reducer';
import { ShippingState, getShippingFeatureState } from '../reducers';

const getOrderEntryState = createSelector(getShippingFeatureState, (state: ShippingState) => state.orderEntry);

export const getOrderEntryLoadingDetails = createSelector(getOrderEntryState, (state: OrderEntryState) => getLoading(state));
export const getOrderEntryProblemDetails = createSelector(getOrderEntryState, (state: OrderEntryState) => getProblemDetails(state));
export const getOrderEntryForm = createSelector(getOrderEntryState, (state:  OrderEntryState) => getForm(state));
export const getOrderEntrySavedId = createSelector(getOrderEntryState, (state: OrderEntryState) => getSavedId(state));
