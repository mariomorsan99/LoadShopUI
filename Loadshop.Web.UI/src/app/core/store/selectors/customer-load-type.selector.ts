import { createSelector } from '@ngrx/store';

import { CoreState } from '../reducers';
import { getEntities, getLoading } from '../reducers/customer-load-type.reducer';

export const getCustomerLoadTypeState = (state: CoreState) => state.customerLoadTypes;

export const getCustomerLoadTypes = createSelector(getCustomerLoadTypeState, getEntities);
export const getLoadingCustomerLoadTypes = createSelector(getCustomerLoadTypeState, getLoading);
