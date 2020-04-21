import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { mapResponse } from '../../shared/operators/map-response';
import { T2GLoadStatus, ServiceResponse, LoadStatusTransaction, LoadStatusStopData } from 'src/app/shared/models';
import { tap } from 'rxjs/operators';
import { LoadStatusInTransitData } from 'src/app/shared/models';

@Injectable()
export class LoadStatusService {
  constructor(private http: HttpClient) {}

  getLatestTopsToGoLoadStatus(referenceLoadId: string): Observable<T2GLoadStatus> {
    return this.http.get<ServiceResponse<T2GLoadStatus>>(`${environment.tops2GoApiUrl}/api/Loads/LastLoadStatus/${referenceLoadId}`)
      .pipe(mapResponse(), this.mapT2GStatus());
  }

  getAllTopsToGoLoadStatuses(referenceLoadId: string): Observable<T2GLoadStatus[]> {
    return this.http.get<ServiceResponse<T2GLoadStatus[]>>(`${environment.tops2GoApiUrl}/api/Loads/AllLoadStatuses/${referenceLoadId}`)
      .pipe(mapResponse(), this.mapT2GStatuses());
  }

  getLatestLoadshopLoadStatus(loadId: string): Observable<LoadStatusTransaction> {
    return this.http.get<ServiceResponse<LoadStatusTransaction>>(`${environment.apiUrl}/api/LoadStatus/latest?loadId=${loadId}`)
      .pipe(mapResponse(), this.mapLoadshopStatus());
  }

  saveInTransit(status: LoadStatusInTransitData) {
    return this.http.post<ServiceResponse<LoadStatusTransaction>>(`${environment.apiUrl}/api/LoadStatus/inTransit`, status)
      .pipe(mapResponse(), this.mapLoadshopStatus());
  }

  saveStopData(status: LoadStatusStopData) {
    return this.http.post<ServiceResponse<LoadStatusTransaction>>(`${environment.apiUrl}/api/LoadStatus/stopStatuses`, status)
      .pipe(mapResponse(), this.mapLoadshopStatus());
  }

  mapT2GStatuses(): (source: Observable<T2GLoadStatus[]>) => Observable<T2GLoadStatus[]> {
    return function (source: Observable<T2GLoadStatus[]>) {
      return source.pipe(tap((statuses: T2GLoadStatus[]) => {
        if (statuses) {
          statuses.forEach(status => {
            status.lastChgDtTm = status.lastChgDtTm ? new Date(status.lastChgDtTm) : null;
            status.statusTime = new Date(status.dateUTCMilliseconds);
          });
        }
      }));
    };
  }

  mapT2GStatus(): (source: Observable<T2GLoadStatus>) => Observable<T2GLoadStatus> {
    return function (source: Observable<T2GLoadStatus>) {
      return source.pipe(tap((status: T2GLoadStatus) => {
        if (status) {
          status.lastChgDtTm = status.lastChgDtTm ? new Date(status.lastChgDtTm) : null;
          status.statusTime = new Date(status.dateUTCMilliseconds);
        }
      }));
    };
  }

  mapLoadshopStatus(): (source: Observable<LoadStatusTransaction>) => Observable<LoadStatusTransaction> {
    return function (source: Observable<LoadStatusTransaction>) {
      return source.pipe(tap((status: LoadStatusTransaction) => {
        if (status) {
          status.messageTime = status.messageTime ? new Date(status.messageTime) : null;
          status.transactionDtTm = status.transactionDtTm ? new Date(status.transactionDtTm) : null;
        }
      }));
    };
  }
}
