import { createSelector } from '@ngrx/store';

import { getSharedFeatureState } from '../reducers';
import { SharedState } from '../reducers';

const getUserCommunicationDisplayState = createSelector(getSharedFeatureState, (state: SharedState) => state.userCommunicationDisplay);

export const getUserCommunicationsLoading = createSelector(getUserCommunicationDisplayState, state => state.userCommunicationsLoading);
export const getUserCommunications = createSelector(getUserCommunicationDisplayState, state => state.userCommunications);

export const getUserCommunicatonAcknowledgementPosting = createSelector(
  getUserCommunicationDisplayState,
  state => state.acknowledgePosting
);
