import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { MessageService } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { LoadDocumentMetadata, LoadDocumentUpload } from '../../models';
import { LoadDocumentService } from '../../services';
import {
  LoadBoardLoadDetailLoadAction,
  LoadDocumentActionTypes,
  LoadDocumentAddDocumentAction,
  LoadDocumentAddDocumentSuccessAction,
  LoadDocumentDeleteDocumentAction,
  LoadDocumentDeleteDocumentFailureAction,
  LoadDocumentDeleteDocumentSuccessAction,
  LoadDocumentDownloadDocumentAction,
  LoadDocumentDownloadDocumentFailureAction,
  LoadDocumentDownloadDocumentSuccessAction,
  LoadDocumentLoadTypesAction,
  LoadDocumentLoadTypesFailureAction,
  LoadDocumentLoadTypesSuccessAction,
} from '../actions';

@Injectable()
export class LoadDocumentEffects {
  @Effect()
  $loadTypes: Observable<Action> = this.actions$.pipe(
    ofType<LoadDocumentLoadTypesAction>(LoadDocumentActionTypes.GetTypes),
    switchMap(() => {
      return this.loadDocumentService.getDocumentTypes().pipe(
        map(data => new LoadDocumentLoadTypesSuccessAction(data)),
        catchError(err => of(new LoadDocumentLoadTypesFailureAction(err)))
      );
    })
  );

  @Effect()
  $uploadDocument: Observable<Action> = this.actions$.pipe(
    ofType<LoadDocumentAddDocumentAction>(LoadDocumentActionTypes.AddDocument),
    mapToPayload<LoadDocumentUpload>(),
    switchMap(documentUpload => {
      return this.loadDocumentService.addDocument(documentUpload).pipe(
        switchMap(data => [
          new LoadDocumentAddDocumentSuccessAction(data),
          new LoadBoardLoadDetailLoadAction(documentUpload.loadId), // reload the detail for load
        ]),
        catchError(err => of(new LoadDocumentLoadTypesFailureAction(err)))
      );
    })
  );

  @Effect()
  $removeDocument: Observable<Action> = this.actions$.pipe(
    ofType<LoadDocumentDeleteDocumentAction>(LoadDocumentActionTypes.DeleteDocument),
    mapToPayload<LoadDocumentMetadata>(),
    switchMap(documentUpload => {
      return this.loadDocumentService.removeDocument(documentUpload).pipe(
        switchMap(data => [
          new LoadDocumentDeleteDocumentSuccessAction(documentUpload),
          new LoadBoardLoadDetailLoadAction(documentUpload.loadId), // reload the detail for load
        ]),
        catchError(err => of(new LoadDocumentDeleteDocumentFailureAction(err)))
      );
    })
  );

  @Effect()
  $downloadDocument: Observable<Action> = this.actions$.pipe(
    ofType<LoadDocumentDownloadDocumentAction>(LoadDocumentActionTypes.DownloadDocument),
    mapToPayload<LoadDocumentMetadata>(),
    switchMap(documentUpload => {
      return this.loadDocumentService.downloadDocument(documentUpload).pipe(
        map(data => new LoadDocumentDownloadDocumentSuccessAction(data)),
        catchError(err => of(new LoadDocumentDownloadDocumentFailureAction(err)))
      );
    })
  );

  @Effect({ dispatch: false })
  $documentUploaded: Observable<LoadDocumentMetadata> = this.actions$.pipe(
    ofType<LoadDocumentAddDocumentSuccessAction>(LoadDocumentActionTypes.AddDocument_Success),
    mapToPayload<LoadDocumentMetadata>(),
    tap(_ => this.messageService.add({ id: 0, detail: `Successfully uploaded ${_.fileName}`, severity: 'success' }))
  );

  @Effect({ dispatch: false })
  $documentRemoved: Observable<LoadDocumentMetadata> = this.actions$.pipe(
    ofType<LoadDocumentDeleteDocumentSuccessAction>(LoadDocumentActionTypes.DeleteDocument_Success),
    mapToPayload<LoadDocumentMetadata>(),
    tap(_ => this.messageService.add({ id: 0, detail: `Successfully removed document`, severity: 'success' }))
  );

  constructor(private actions$: Actions, private loadDocumentService: LoadDocumentService, private messageService: MessageService) {}
}
