import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { SecurityAppActionType } from 'src/app/shared/models/security-app-action-type';
import { environment } from '../../../environments/environment';
import { AllMessageTypes, ServiceResponse, UserLane, UserModel } from '../../shared/models';
import { mapResponse } from '../../shared/operators/map-response';

@Injectable()
export class UserLanesService {
  constructor(private http: HttpClient) {}
  getCustomerSavedLanes(user: UserModel): Observable<UserLane[]> {
    if (user.hasSecurityAction(SecurityAppActionType.ViewFavorites)) {
      return this.http.get<ServiceResponse<UserLane[]>>(environment.apiUrl + '/api/UserLanes/').pipe(
        mapResponse(),
        map(x => x.map(this.mapLane))
      );
    }

    return of([]);
  }

  updateLane(lane: UserLane): Observable<UserLane> {
    const update = this.createUserLane(lane);
    return this.http.put<ServiceResponse<UserLane>>(environment.apiUrl + '/api/UserLanes/', update).pipe(mapResponse(), map(this.mapLane));
  }

  addLane(lane: UserLane): Observable<UserLane> {
    const update = this.createUserLane(lane);
    return this.http.post<ServiceResponse<UserLane>>(environment.apiUrl + '/api/UserLanes/', update).pipe(mapResponse(), map(this.mapLane));
  }

  deleteLane(lane: UserLane): Observable<UserLane> {
    return this.http.delete<ServiceResponse<UserLane>>(`${environment.apiUrl}/api/UserLanes?id=${lane.userLaneId}`).pipe(mapResponse());
  }

  createUserLane(lane: UserLane): any {
    const update = Object.assign(lane, {
      userLaneMessageTypes: lane.userLaneMessageTypes.map(x => {
        x.selected = lane.laneNotifications[x.messageTypeId]; // object properties are lowercase
        return x;
      }),
    });
    delete update.laneNotifications;
    return update;
  }

  mapLane(lane: UserLane): UserLane {
    return Object.assign(lane, {
      display: true,
      equipmentCount: lane.equipmentIds.length,
      laneNotifications: {
        Email: lane.userLaneMessageTypes.find(z => z.messageTypeId === AllMessageTypes.Email).selected,
        Cell_Phone: lane.userLaneMessageTypes.find(z => z.messageTypeId === AllMessageTypes.CellPhone).selected,
      },
    });
  }
}
