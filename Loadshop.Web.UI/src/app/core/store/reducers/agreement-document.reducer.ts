import { AgreementDocumentActions, AgreementDocumentActionTypes } from '../actions';

export interface AgreementDocumentState {
  loading: boolean;
  loaded: boolean;
}

const initialState: AgreementDocumentState = {
  loading: false,
  loaded: false,
};

export function agreementDocumentReducer(
  state: AgreementDocumentState = initialState,
  action: AgreementDocumentActions
): AgreementDocumentState {
  switch (action.type) {
    case AgreementDocumentActionTypes.AcceptAgreement_Failure: {
      return {
        ...state,
        loading: false,
        loaded: true,
      };
    }
    case AgreementDocumentActionTypes.AcceptAgreement: {
      return {
        ...state,
        loading: true,
        loaded: false,
      };
    }
    case AgreementDocumentActionTypes.AcceptAgreement_Success: {
      return {
        ...state,
        loading: false,
        loaded: true,
      };
    }
    default:
      return state;
  }
}

export const getLoading = (state: AgreementDocumentState) => state.loading;
export const getLoaded = (state: AgreementDocumentState) => state.loaded;
