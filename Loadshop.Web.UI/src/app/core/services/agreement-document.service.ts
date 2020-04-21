import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ServiceResponse } from 'src/app/shared/models';
import { environment } from '../../../environments/environment';
import { mapResponse } from '../../shared/operators/map-response';

@Injectable()
export class AgreementDocumentService {
  constructor(private http: HttpClient) {}

  acceptAgreement(): Observable<Response> {
    return this.http.post<ServiceResponse<Response>>(`${environment.apiUrl}/api/agreements`, null).pipe(mapResponse());
  }
}
