import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { LoadDocumentMetadata, LoadDocumentType, LoadDocumentUpload, ServiceResponse } from '../models';
import { LoadDocumentDownload } from '../models/load-document-download';
import { mapResponse } from '../operators/map-response';

@Injectable()
export class LoadDocumentService {
  constructor(private http: HttpClient) {}

  getDocumentTypes(): Observable<LoadDocumentType[]> {
    return this.http.get<ServiceResponse<LoadDocumentType[]>>(`${environment.apiUrl}/api/loaddocuments/types`).pipe(mapResponse());
  }

  addDocument(document: LoadDocumentUpload): Observable<LoadDocumentMetadata> {
    const postFormData: FormData = new FormData();

    postFormData.append('attachment', document.file, document.file.name);

    // clear out object before JSON string
    document.file = null;
    postFormData.append('loaddocumentform', JSON.stringify(document));
    return this.http
      .post<ServiceResponse<LoadDocumentMetadata>>(`${environment.apiUrl}/api/loaddocuments`, postFormData)
      .pipe(mapResponse());
  }

  removeDocument(document: LoadDocumentMetadata): Observable<Response> {
    return this.http
      .delete<ServiceResponse<Response>>(`${environment.apiUrl}/api/loaddocuments/${document.loadDocumentId}`)
      .pipe(mapResponse());
  }

  downloadDocument(document: LoadDocumentMetadata): Observable<LoadDocumentDownload> {
    return this.http
      .get(`${environment.apiUrl}/api/loaddocuments/${document.loadDocumentId}/download`, {
        responseType: 'blob',
      })
      .pipe(
        map(blob => {
          return {
            metadata: document,
            file: blob,
          };
        })
      );
  }
}
