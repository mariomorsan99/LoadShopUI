import { LoadDocumentType } from '../../models';
import { LoadDocumentDownload } from '../../models/load-document-download';
import { LoadDocumentActions, LoadDocumentActionTypes } from '../actions/load-document.actions';

export interface LoadDocumentState {
  loaded: boolean;
  loading: boolean;
  types: LoadDocumentType[];
  documentDownload: LoadDocumentDownload;
}
const initialState: LoadDocumentState = {
  loaded: false,
  loading: false,
  types: [],
  documentDownload: null,
};

export function LoadDocumentReducer(state: LoadDocumentState = initialState, action: LoadDocumentActions): LoadDocumentState {
  switch (action.type) {
    case LoadDocumentActionTypes.GetTypes: {
      return { ...state, loading: true, loaded: false, types: [] };
    }
    case LoadDocumentActionTypes.GetTypes_Success: {
      return { ...state, loading: false, loaded: true, types: action.payload };
    }
    case LoadDocumentActionTypes.GetTypes_Failure: {
      return { ...state, loading: false, loaded: false, types: [] };
    }
    case LoadDocumentActionTypes.AddDocument:
    case LoadDocumentActionTypes.DeleteDocument: {
      return { ...state, loading: true, loaded: false };
    }
    case LoadDocumentActionTypes.AddDocument_Success:
    case LoadDocumentActionTypes.DeleteDocument_Success: {
      return { ...state, loading: false, loaded: true };
    }
    case LoadDocumentActionTypes.AddDocument_Failure:
    case LoadDocumentActionTypes.DeleteDocument_Failure: {
      return { ...state, loading: false, loaded: true };
    }
    case LoadDocumentActionTypes.DownloadDocument: {
      return { ...state, loading: true, documentDownload: null };
    }
    case LoadDocumentActionTypes.DownloadDocument_Success: {
      return { ...state, loading: false, documentDownload: action.payload };
    }
    case LoadDocumentActionTypes.DownloadDocument_Clear: {
      return { ...state, loading: false, documentDownload: null };
    }
    default:
      return state;
  }
}

export const getLoaded = (state: LoadDocumentState) => state.loaded;
export const getLoading = (state: LoadDocumentState) => state.loading;
export const getDocumentTypes = (state: LoadDocumentState) => state.types;
export const getDocumentDownload = (state: LoadDocumentState) => state.documentDownload;
