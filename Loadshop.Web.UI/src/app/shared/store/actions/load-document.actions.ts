/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';
import { LoadDocumentMetadata, LoadDocumentType, LoadDocumentUpload } from '../../models';
import { LoadDocumentDownload } from '../../models/load-document-download';

export enum LoadDocumentActionTypes {
  GetTypes = '[LoadDocument] GET_TYPES',
  GetTypes_Success = '[LoadDocument] GET_TYPES_SUCCESS',
  GetTypes_Failure = '[LoadDocument] GET_TYPES_FAILURE',
  AddDocument = '[LoadDocument] ADD_DOCUMENT',
  AddDocument_Success = '[LoadDocument] ADD_DOCUMENT_SUCCESS',
  AddDocument_Failure = '[LoadDocument] ADD_DOCUMENT_FAILURE',
  DeleteDocument = '[LoadDocument] DELETE_DOCUMENT',
  DeleteDocument_Success = '[LoadDocument] DELETE_DOCUMENT_SUCCESS',
  DeleteDocument_Failure = '[LoadDocument] DELETE_DOCUMENT_FAILURE',
  DownloadDocument = '[LoadDocument] DOWNLOAD_DOCUMENT',
  DownloadDocument_Success = '[LoadDocument] DOWNLOAD_DOCUMENT_SUCCESS',
  DownloadDocument_Failure = '[LoadDocument] DOWNLOAD_DOCUMENT_FAILURE',
  DownloadDocument_Clear = '[LoadDocument] DOWNLOAD_DOCUMENT_CLEAR',
}

export class LoadDocumentLoadTypesAction implements Action {
  readonly type = LoadDocumentActionTypes.GetTypes;
}

export class LoadDocumentLoadTypesSuccessAction implements Action {
  readonly type = LoadDocumentActionTypes.GetTypes_Success;

  constructor(public payload: LoadDocumentType[]) {}
}

export class LoadDocumentLoadTypesFailureAction implements Action {
  readonly type = LoadDocumentActionTypes.GetTypes_Failure;

  constructor(public payload: Error) {}
}

export class LoadDocumentAddDocumentAction implements Action {
  readonly type = LoadDocumentActionTypes.AddDocument;
  constructor(public payload: LoadDocumentUpload) {}
}

export class LoadDocumentAddDocumentSuccessAction implements Action {
  readonly type = LoadDocumentActionTypes.AddDocument_Success;
  constructor(public payload: LoadDocumentMetadata) {}
}

export class LoadDocumentAddDocumentFailureAction implements Action {
  readonly type = LoadDocumentActionTypes.AddDocument_Failure;

  constructor(public payload: Error) {}
}

export class LoadDocumentDeleteDocumentAction implements Action {
  readonly type = LoadDocumentActionTypes.DeleteDocument;
  constructor(public payload: LoadDocumentMetadata) {}
}

export class LoadDocumentDeleteDocumentSuccessAction implements Action {
  readonly type = LoadDocumentActionTypes.DeleteDocument_Success;
  constructor(public payload: LoadDocumentMetadata) {}
}

export class LoadDocumentDeleteDocumentFailureAction implements Action {
  readonly type = LoadDocumentActionTypes.DeleteDocument_Failure;

  constructor(public payload: Error) {}
}

export class LoadDocumentDownloadDocumentAction implements Action {
  readonly type = LoadDocumentActionTypes.DownloadDocument;
  constructor(public payload: LoadDocumentMetadata) {}
}

export class LoadDocumentDownloadDocumentSuccessAction implements Action {
  readonly type = LoadDocumentActionTypes.DownloadDocument_Success;
  constructor(public payload: LoadDocumentDownload) {}
}

export class LoadDocumentDownloadDocumentFailureAction implements Action {
  readonly type = LoadDocumentActionTypes.DownloadDocument_Failure;

  constructor(public payload: Error) {}
}

export class LoadDocumentDownloadDocumentClearAction implements Action {
  readonly type = LoadDocumentActionTypes.DownloadDocument_Clear;
}

export type LoadDocumentActions =
  | LoadDocumentLoadTypesAction
  | LoadDocumentLoadTypesSuccessAction
  | LoadDocumentLoadTypesFailureAction
  | LoadDocumentAddDocumentAction
  | LoadDocumentAddDocumentSuccessAction
  | LoadDocumentAddDocumentFailureAction
  | LoadDocumentDeleteDocumentAction
  | LoadDocumentDeleteDocumentSuccessAction
  | LoadDocumentDeleteDocumentFailureAction
  | LoadDocumentDownloadDocumentAction
  | LoadDocumentDownloadDocumentSuccessAction
  | LoadDocumentDownloadDocumentFailureAction
  | LoadDocumentDownloadDocumentClearAction;
