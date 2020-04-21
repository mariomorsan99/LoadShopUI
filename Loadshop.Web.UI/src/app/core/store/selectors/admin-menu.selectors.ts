import { createSelector } from '@ngrx/store';

import { CoreState } from '../reducers';
import { getLoading, getLoaded, getEntities } from '../reducers/admin-menu.reducer';

export const getAdminMenuState = (state: CoreState) => state.adminMenu;
export const getAdminMenuLoading = createSelector(getAdminMenuState, getLoading);
export const getAdminMenuLoaded = createSelector(getAdminMenuState, getLoaded);
export const getAdminMenuEntities = createSelector(getAdminMenuState, getEntities);
