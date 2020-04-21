import { createSelector } from '@ngrx/store';

import { CoreState } from '../reducers';
import { getEntity, getLoading } from '../reducers/customer.reducer';

export const getCustomerState = (state: CoreState) => state.customer;

export const getCustomer = createSelector(getCustomerState, getEntity);
export const getLoadingCustomer = createSelector(getCustomerState, getLoading);
