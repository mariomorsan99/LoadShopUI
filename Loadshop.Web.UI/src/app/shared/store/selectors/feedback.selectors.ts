import { createSelector } from '@ngrx/store';

import { getSharedFeatureState } from '../reducers';
import { SharedState } from '../reducers';

const getFeedbackState = createSelector(getSharedFeatureState, (state: SharedState) => state.feedback);

export const getFeedbackQuestion = createSelector(getFeedbackState, state => state.question);
export const getFeedbackResponse = createSelector(getFeedbackState, state => state.response);
export const getLoadingQuestion = createSelector(getFeedbackState, state => state.loadingQuestion);
export const getLoadingResponse = createSelector(getFeedbackState, state => state.loadingResponse);
export const getSavingResponse = createSelector(getFeedbackState, state => state.savingResponse);
