import { createSelector } from '@ngrx/store';
import { getSharedFeatureState, SharedState } from '../reducers';

const loadshopApplicationState = createSelector(getSharedFeatureState, (state: SharedState) => state.loadshopApplication);

export const loadshopApplicationReady = createSelector(loadshopApplicationState, state => state.ready);
