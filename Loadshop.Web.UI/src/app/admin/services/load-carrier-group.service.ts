import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoadCarrierGroupCarrierData, LoadCarrierGroupDetailData, LoadCarrierGroupType, ServiceResponse } from '../../shared/models';
import { mapResponse } from '../../shared/operators/map-response';

@Injectable()
export class LoadCarrierGroupService {
  constructor(private http: HttpClient) {}

  getGroups(customerId: string): Observable<LoadCarrierGroupDetailData[]> {
    return this.http
      .get<ServiceResponse<LoadCarrierGroupDetailData[]>>(`${environment.apiUrl}/api/LoadCarrierGroup?customerId=${customerId}`)
      .pipe(mapResponse());
  }

  getGroup(loadCarrierGroupId: number): Observable<LoadCarrierGroupDetailData> {
    return this.http
      .get<ServiceResponse<LoadCarrierGroupDetailData>>(`${environment.apiUrl}/api/LoadCarrierGroup/${loadCarrierGroupId}`)
      .pipe(mapResponse());
  }

  updateGroup(group: LoadCarrierGroupDetailData): Observable<LoadCarrierGroupDetailData> {
    const update = this.createGroup(group);
    return this.http
      .put<ServiceResponse<LoadCarrierGroupDetailData>>(`${environment.apiUrl}/api/LoadCarrierGroup/`, update)
      .pipe(mapResponse());
  }

  addGroup(group: LoadCarrierGroupDetailData): Observable<LoadCarrierGroupDetailData> {
    const update = this.createGroup(group);
    return this.http
      .post<ServiceResponse<LoadCarrierGroupDetailData>>(`${environment.apiUrl}/api/LoadCarrierGroup/`, update)
      .pipe(mapResponse());
  }

  deleteGroup(group: LoadCarrierGroupDetailData): Observable<object> {
    return this.http
      .delete<ServiceResponse<object>>(`${environment.apiUrl}/api/LoadCarrierGroup/${group.loadCarrierGroupId}`)
      .pipe(mapResponse());
  }

  getCarriers(loadCarrierGroupId: number): Observable<LoadCarrierGroupCarrierData[]> {
    return this.http
      .get<ServiceResponse<LoadCarrierGroupCarrierData[]>>(`${environment.apiUrl}/api/LoadCarrierGroup/${loadCarrierGroupId}/Carrier`)
      .pipe(mapResponse());
  }

  addCarriers(carriers: LoadCarrierGroupCarrierData[]): Observable<LoadCarrierGroupCarrierData[]> {
    return this.http
      .post<ServiceResponse<LoadCarrierGroupCarrierData[]>>(`${environment.apiUrl}/api/LoadCarrierGroup/Carrier`, carriers)
      .pipe(mapResponse());
  }

  deleteCarrier(carrier: LoadCarrierGroupCarrierData): Observable<object> {
    return this.http
      .delete<ServiceResponse<object>>(`${environment.apiUrl}/api/LoadCarrierGroup/Carrier/${carrier.loadCarrierGroupCarrierId}`)
      .pipe(mapResponse());
  }

  deleteAllCarriers(loadCarrierGroupId: number): Observable<object> {
    return this.http
      .delete<ServiceResponse<object>>(`${environment.apiUrl}/api/LoadCarrierGroup/${loadCarrierGroupId}/Carrier/`)
      .pipe(mapResponse());
  }

  getLoadCarrierGroupTypes(): Observable<LoadCarrierGroupType[]> {
    return this.http
      .get<ServiceResponse<LoadCarrierGroupType[]>>(`${environment.apiUrl}/api/LoadCarrierGroup/LoadCarrierGroupTypes`)
      .pipe(mapResponse());
  }

  private createGroup(group: LoadCarrierGroupDetailData): LoadCarrierGroupDetailData {
    const update = Object.assign({}, group);
    // TODO: delete update.customer;
    // delete update.equipment;
    return update;
  }
}
