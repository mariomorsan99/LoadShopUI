import { createSelector } from '@ngrx/store';
import { getUserFeatureState, UserState } from '../reducers';
import { getAuthorizedFocusEntities, getLoaded, getLoading, getSaving } from '../reducers/user-focus-entity-selector.reducer';

const getUserFocusEntitySelectorState = createSelector(getUserFeatureState, (state: UserState) => state.userFocusEntitySelector);

export const getAuthorizedFocusEntitiesSelector = createSelector(getUserFocusEntitySelectorState, getAuthorizedFocusEntities);
export const getAuthorizedFocusEntitiesLoadingSelector = createSelector(getUserFocusEntitySelectorState, getLoading);
export const getAuthorizedFocusEntitiesLoadedSelector = createSelector(getUserFocusEntitySelectorState, getLoaded);
export const getSavingSelector = createSelector(getUserFocusEntitySelectorState, getSaving);
