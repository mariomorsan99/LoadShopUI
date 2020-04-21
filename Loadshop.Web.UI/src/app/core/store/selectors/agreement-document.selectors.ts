import { createSelector } from '@ngrx/store';
import { CoreState } from '../reducers';
import { getLoaded, getLoading } from '../reducers/agreement-document.reducer';

export const getAgreementDocumentState = (state: CoreState) => state.agreementDocumentState;
export const getAgreementDocumentLoading = createSelector(getAgreementDocumentState, getLoading);
export const getAgreementDocumentLoaded = createSelector(getAgreementDocumentState, getLoaded);
