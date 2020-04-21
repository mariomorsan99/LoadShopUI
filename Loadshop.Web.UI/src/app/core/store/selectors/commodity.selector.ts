import { createSelector } from '@ngrx/store';

import { CoreState } from '../reducers';
import { getEntities, getLoading } from '../reducers/commodity.reducer';

export const getCommodityState = (state: CoreState) => state.commodities;

export const getCommodities = createSelector(getCommodityState, getEntities);
export const getLoadingCommodities = createSelector(getCommodityState, getLoading);
