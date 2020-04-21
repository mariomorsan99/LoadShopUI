import { createSelector } from '@ngrx/store';
import { CoreState } from '../reducers';
import { getEntities, StateState } from '../reducers/state.reducer';

export const getStateState = (state: CoreState) => state.states;
export const getStates = createSelector(getStateState, getEntities);
export const getLoadingStates = createSelector(getStateState, (x: StateState) => x.loading);
