import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { LoadAudit } from 'src/app/shared/models/load-audit';
import { environment } from '../../../environments/environment';
import {
  Load,
  LoadDetail,
  LoadView,
  SaveVisibilityDataResponse,
  SearchTypeData,
  ServiceResponse,
  VisibilityBadge,
  PageableResult,
} from '../../shared/models';
import { mapResponse } from '../../shared/operators/map-response';
import { PageableQueryHelper } from 'src/app/shared/utilities';

@Injectable()
export class LoadBoardService {
  constructor(private http: HttpClient) {}

  getDashboardLoadsByUser(): Observable<LoadView[]> {
    // Marketplace is not using server side paging
    return this.getLoadsBySearchTypeAndUser(SearchTypeData.UserLanes, null).pipe(map(x => x.data));
  }

  getLoadsBySearchTypeAndUser(searchType: SearchTypeData, queryHelper: PageableQueryHelper): Observable<PageableResult<LoadView>> {
    let query = '?';
    if (queryHelper) {
      query = queryHelper.generateQuery() + '&';
    }

    if (queryHelper && queryHelper.filter) {
      return this.http
        .post<ServiceResponse<PageableResult<LoadView>>>(
          environment.apiUrl + `/api/Loads${query}searchType=${searchType}`,
          queryHelper.filter
        )
        .pipe(
          mapResponse(),
          map(x => {
            this.populateDisplays(x.data);
            return x;
          })
        );
    }
    return this.http.get<ServiceResponse<PageableResult<LoadView>>>(environment.apiUrl + `/api/Loads${query}searchType=${searchType}`).pipe(
      mapResponse(),
      map(x => {
        this.populateDisplays(x.data);
        return x;
      })
    );
  }

  getBookedLoadsByUser(queryHelper: PageableQueryHelper): Observable<PageableResult<LoadView>> {
    return this.getLoadsBySearchTypeAndUser(SearchTypeData.Booked, queryHelper);
  }

  getDeliveredLoadsByUser(queryHelper: PageableQueryHelper): Observable<PageableResult<LoadView>> {
    return this.getLoadsBySearchTypeAndUser(SearchTypeData.Delivered, queryHelper);
  }

  saveVisibilityData(load: LoadView): Observable<SaveVisibilityDataResponse> {
    return this.http
      .put<ServiceResponse<SaveVisibilityDataResponse>>(environment.apiUrl + '/api/Loads/visibilitydata', load)
      .pipe(mapResponse());
  }

  getLoadById(id: string): Observable<LoadDetail> {
    return this.http.get<ServiceResponse<LoadDetail>>(environment.apiUrl + '/api/Loads/' + id).pipe(
      mapResponse(),
      map(x => this.updateDetailDisplays(x))
    );
  }

  auditLoad(loadAudit: LoadAudit): Observable<number> {
    return this.http
      .post<ServiceResponse<number>>(environment.apiUrl + '/api/Loads/' + loadAudit.loadId + '/audit/' + loadAudit.auditType.toString(), {})
      .pipe(mapResponse());
  }

  update(load: Load): Observable<Load> {
    return this.http.put<ServiceResponse<Load>>(environment.apiUrl + '/api/Loads/', load).pipe(mapResponse());
  }

  getNumLoadsRequiringVisibilityInfo(): Observable<VisibilityBadge> {
    return this.http
      .get<ServiceResponse<VisibilityBadge>>(environment.apiUrl + '/api/Loads/num-requiring-visibility-info')
      .pipe(mapResponse());
  }

  private populateDisplays(loads: LoadView[]) {
    return loads.map(load => {
      load.distanceFrom = this.calculateDistanceFrom(load.distanceFromOrig, load.distanceFromDest);
      load.originDisplay = `${load.originCity}, ${load.originState}`;
      load.destinationDisplay = `${load.destCity}, ${load.destState}`;
      load.totalRateDisplay = load.lineHaulRate + load.fuelRate;
      return load;
    });
  }

  private calculateDistanceFrom(dist1: number, dist2: number) {
    if (!dist1 && !dist2) {
      return null;
    }
    return (dist1 ? dist1 : 0) + (dist2 ? dist2 : 0);
  }

  private updateDetailDisplays(load: LoadDetail) {
    load.loadStops.sort((a, b) => {
      if (a.stopNbr < b.stopNbr) {
        return -1;
      }
      return 1;
    });
    return load;
  }
}
