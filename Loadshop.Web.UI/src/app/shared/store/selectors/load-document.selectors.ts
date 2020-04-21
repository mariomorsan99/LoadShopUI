import { createSelector } from '@ngrx/store';
import { getSharedFeatureState, SharedState } from '../reducers';
import { getDocumentTypes, getLoaded, getLoading, getDocumentDownload } from '../reducers/load-document.reducer';

const getLoadDocumentState = createSelector(getSharedFeatureState, (state: SharedState) => state.loadDocumentTypes);

export const getLoadDocumentLoading = createSelector(getLoadDocumentState, getLoading);
export const getLoadDocumentLoaded = createSelector(getLoadDocumentState, getLoaded);
export const getLoadDocumentTypes = createSelector(getLoadDocumentState, getDocumentTypes);
export const getLoadDocumentDownload = createSelector(getLoadDocumentState, getDocumentDownload);
