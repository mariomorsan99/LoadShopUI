import { createSelector } from '@ngrx/store';

import { CoreState } from '../reducers';
import { getLoading, getLoaded, getEntities, getVisibilityBadge } from '../reducers/menu.reducer';

export const getMenuState = (state: CoreState) => state.menu;
export const getMenuLoading = createSelector(getMenuState, getLoading);
export const getMenuLoaded = createSelector(getMenuState, getLoaded);
export const getMenuEntities = createSelector(getMenuState, getEntities);
export const getMenuVisibilityBadge = createSelector(getMenuState, getVisibilityBadge);
