import { ActionReducerMap, createFeatureSelector } from '@ngrx/store';
import { LoadBoardLoadDetailReducer, LoadBoardLoadDetailState } from './load-board-load-detail.reducer';
import { LoadDocumentReducer, LoadDocumentState } from './load-document.reducer';
import { LoadshopApplicationReducer, LoadshopApplicationState } from './loadshop-application.reducer';
import { UserCommuncationDisplayState, UserCommunicationDisplayReducer } from './user-communication-display.reducer';
import { FeedbackState, feedbackReducer } from './feedback.reducer';

export interface SharedState {
  loadDetail: LoadBoardLoadDetailState;
  userCommunicationDisplay: UserCommuncationDisplayState;
  loadshopApplication: LoadshopApplicationState;
  loadDocumentTypes: LoadDocumentState;
  feedback: FeedbackState;
}

export const reducers: ActionReducerMap<SharedState> = {
  loadDetail: LoadBoardLoadDetailReducer,
  userCommunicationDisplay: UserCommunicationDisplayReducer,
  loadshopApplication: LoadshopApplicationReducer,
  loadDocumentTypes: LoadDocumentReducer,
  feedback: feedbackReducer,
};

export const getSharedFeatureState = createFeatureSelector<SharedState>('sharedstate');
