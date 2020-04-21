import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ILoadAuditLogData } from 'src/app/shared/models/load-audit-log-data';
import { defaultOrderEntry, OrderEntryForm } from 'src/app/shared/models/order-entry-form';
import { PostLoadsClientResponse } from 'src/app/shared/models/post-loads-client-response';
import { IPostingLoad } from 'src/app/shared/models/posting-load';
import { ShippingLoadView } from 'src/app/shared/models/shipping-load-view';
import { environment } from '../../../environments/environment';
import {
  CustomerLocation,
  IShippingLoadDetail,
  Load,
  LoadCarrierScacData,
  RemoveCarrierData,
  RemoveLoadData,
  ServiceResponse,
  ShippingLoadCarrierGroupData,
  StopTypes,
  PageableResult,
  LoadCarrierScacRestrictionData,
} from '../../shared/models';
import { mapResponse } from '../../shared/operators/map-response';
import { PageableQueryHelper } from 'src/app/shared/utilities';

@Injectable()
export class ShippingService {
  constructor(private http: HttpClient) {}

  getLoadById(id: string): Observable<IShippingLoadDetail> {
    return this.http.get<ServiceResponse<IShippingLoadDetail>>(environment.apiUrl + '/api/ShippingLoad/' + id).pipe(mapResponse());
  }

  getPageableLoadsBySearchType(searchType: string, queryHelper: PageableQueryHelper = null): Observable<PageableResult<ShippingLoadView>> {
    let query = '';
    if (queryHelper) {
      query = queryHelper.generateQuery();
    }

    if (queryHelper && queryHelper.filter) {
      return this.http
        .post<ServiceResponse<PageableResult<ShippingLoadView>>>(
          environment.apiUrl + '/api/ShippingLoad/GetLoadsBySearchType/' + searchType + query,
          queryHelper.filter
        )
        .pipe(mapResponse());
    } else {
      return this.http
        .get<ServiceResponse<PageableResult<ShippingLoadView>>>(
          environment.apiUrl + '/api/ShippingLoad/GetLoadsBySearchType/' + searchType + query
        )
        .pipe(mapResponse());
    }
  }

  // To Do: Remove when all shipping grids moved to paging
  getLoadsBySearchType(searchType: string, queryHelper: PageableQueryHelper = null): Observable<ShippingLoadView[]> {
    return this.getPageableLoadsBySearchType(searchType, queryHelper).pipe(map(x => x.data));
  }

  getLoadCarrierGroups(loadId: string): Observable<ShippingLoadCarrierGroupData[]> {
    return this.http
      .get<ServiceResponse<ShippingLoadCarrierGroupData[]>>(environment.apiUrl + '/api/LoadCarrierGroup/Load/' + loadId)
      .pipe(mapResponse());
  }

  getLoadCarrierScacs(loadId: string): Observable<LoadCarrierScacData[]> {
    return this.http
      .get<ServiceResponse<LoadCarrierScacData[]>>(environment.apiUrl + '/api/ShippingLoad/CarrierScac/' + loadId)
      .pipe(mapResponse());
  }

  getLoadCarrierScacRestrictions(loadId: string): Observable<LoadCarrierScacRestrictionData[]> {
    return this.http
      .get<ServiceResponse<LoadCarrierScacRestrictionData[]>>(environment.apiUrl + '/api/ShippingLoad/CarrierScacRestriction/' + loadId)
      .pipe(mapResponse());
  }

  getLoadsForHomeTab(): Observable<IShippingLoadDetail[]> {
    return this.http
      .get<ServiceResponse<IShippingLoadDetail[]>>(environment.apiUrl + '/api/ShippingLoad/GetLoadsForHomeTab/')
      .pipe(mapResponse());
  }

  postLoads(loads: IPostingLoad[]): Observable<PostLoadsClientResponse> {
    return this.http
      .post<ServiceResponse<PostLoadsClientResponse>>(environment.apiUrl + '/api/ShippingLoad/PostLoads/', loads)
      .pipe(mapResponse());
  }

  removeLoad(loadId: string): Observable<IShippingLoadDetail> {
    return this.http
      .post<ServiceResponse<IShippingLoadDetail>>(environment.apiUrl + '/api/ShippingLoad/RemoveLoad/' + loadId, null)
      .pipe(mapResponse());
  }

  deleteLoad(loadId: string): Observable<IShippingLoadDetail> {
    return this.http
      .post<ServiceResponse<IShippingLoadDetail>>(environment.apiUrl + '/api/ShippingLoad/DeleteLoad/' + loadId, null)
      .pipe(mapResponse());
  }

  deleteDetailLoad(payload: RemoveLoadData): Observable<IShippingLoadDetail> {
    return this.http
      .post<ServiceResponse<IShippingLoadDetail>>(
        environment.apiUrl + '/api/ShippingLoad/DeleteDetailLoad/' + payload.load.loadId,
        payload.ratingQuestionAnswer
      )
      .pipe(mapResponse());
  }

  removeCarrier(payload: RemoveCarrierData): Observable<Load> {
    return this.http
      .post<ServiceResponse<Load>>(
        environment.apiUrl + '/api/ShippingLoad/RemoveCarrier/' + payload.load.loadId,
        payload.ratingQuestionAnswer
      )
      .pipe(mapResponse());
  }

  getLoadAuditLogs(loadId: string): Observable<ILoadAuditLogData[]> {
    return this.http
      .get<ServiceResponse<ILoadAuditLogData[]>>(environment.apiUrl + '/api/ShippingLoad/AuditLog/' + loadId)
      .pipe(mapResponse());
  }

  getManuallyCreatedLoadById(id: string): Observable<any> {
    return this.http
      .get<ServiceResponse<OrderEntryForm>>(environment.apiUrl + '/api/ShippingLoad/ManuallyCreated/' + id)
      .pipe(mapResponse(), map(this.convertToOrderEntryForm));
  }

  searchLocations(searchTerm: string): Observable<any> {
    return this.http
      .get<ServiceResponse<CustomerLocation>>(environment.apiUrl + '/api/ShippingLoad/locations/' + searchTerm)
      .pipe(mapResponse());
  }

  createLoad(load: any): Observable<any> {
    return this.http.post<ServiceResponse<any>>(environment.apiUrl + '/api/ShippingLoad/CreateLoad/', load).pipe(mapResponse());
  }

  updateLoad(load: any): Observable<any> {
    return this.http.post<ServiceResponse<any>>(environment.apiUrl + '/api/ShippingLoad/UpdateLoad/', load).pipe(mapResponse());
  }

  private convertToOrderEntryForm(load: OrderEntryForm): OrderEntryForm {
    const result = { ...defaultOrderEntry, ...load };
    result.loadStops = load.loadStops.map((stop, index) => {
      if (stop.earlyDate && stop.earlyDate.length) {
        stop.earlyDate = new Date(stop.earlyDate);
      }
      if (stop.lateDate && stop.lateDate.length) {
        stop.lateDate = new Date(stop.lateDate);
      }

      if (!stop.stopType) {
        stop.stopType = index === 0 ? StopTypes[StopTypes.Pickup] : StopTypes[StopTypes.Delivery];
      }

      return stop;
    });

    return result;
  }
}
