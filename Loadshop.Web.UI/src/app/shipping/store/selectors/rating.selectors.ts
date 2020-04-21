import { createSelector } from '@ngrx/store';
import { getShippingFeatureState, ShippingState } from '../reducers';
import { getLoaded, getLoading, RatingState } from '../reducers/rating.reducer';

const getRatingState = createSelector(getShippingFeatureState, (state: ShippingState) => state.rating);

export const getRatingQuestions = createSelector(getRatingState, (state: RatingState) => state.questions);
export const getRatingQuestionsLoaded = createSelector(getRatingState, (state: RatingState) => getLoaded(state));
export const getRatingQuestionsLoading = createSelector(getRatingState, (state: RatingState) => getLoading(state));
