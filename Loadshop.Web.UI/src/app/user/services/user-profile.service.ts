/* eslint-disable @typescript-eslint/camelcase */
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SelectItemGroup } from 'primeng/api';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { GuidEmpty } from '../../core/utilities/constants';
import {
  AllMessageTypes,
  LoadStatusNotificationsData,
  ServiceResponse,
  User,
  UserFocusEntity,
  UserFocusEntityResult,
} from '../../shared/models';
import { mapResponse } from '../../shared/operators/map-response';
import { groupBy } from '../../shared/utilities';

@Injectable()
export class UserProfileService {
  constructor(private http: HttpClient) {}

  getCustomerProfile(): Observable<User> {
    return this.http
      .get<ServiceResponse<User>>(environment.apiUrl + '/api/UserProfiles/')
      .pipe(mapResponse(), map(this.populateSelectList));
  }

  getUserLoadStatusNotifications(): Observable<LoadStatusNotificationsData> {
    return this.http
      .get<ServiceResponse<LoadStatusNotificationsData>>(environment.apiUrl + '/api/UserProfiles/loadstatusnotifications')
      .pipe(mapResponse());
  }

  getAllMyAuthorizedEntities(): Observable<UserFocusEntityResult> {
    return this.http.get<ServiceResponse<UserFocusEntity[]>>(environment.apiUrl + '/api/UserProfiles/GetAllMyAuthorizedEntities').pipe(
      mapResponse(),
      map(entities => this.buildFocusEntitiesResult(entities))
    );
  }

  updateFocusEntity(focusEntity: UserFocusEntity): Observable<User> {
    return this.http.put<ServiceResponse<User>>(environment.apiUrl + '/api/UserProfiles/PutFocusEntity', focusEntity).pipe(mapResponse());
  }

  update(user: User): Observable<User> {
    return this.http.put<ServiceResponse<User>>(environment.apiUrl + '/api/UserProfiles/', user).pipe(mapResponse());
  }

  updateLoadStatusNotifications(payload: LoadStatusNotificationsData): Observable<LoadStatusNotificationsData> {
    return this.http
      .put<ServiceResponse<LoadStatusNotificationsData>>(environment.apiUrl + '/api/UserProfiles/loadstatusnotifications', payload)
      .pipe(mapResponse());
  }
  validateUserNotifications(user: User): boolean {
    // check if the user has a phone and email, if not send them to the profile page
    const emailNotifications = user.userNotifications.filter(x => x.messageTypeId === AllMessageTypes.Email);

    if (emailNotifications.every(x => x.userNotificationId === GuidEmpty)) {
      return false;
    }
    // check if the user has a phone and email, if not send them to the profile page
    const phoneNotifications = user.userNotifications.filter(
      x => x.messageTypeId === AllMessageTypes.Phone || x.messageTypeId === AllMessageTypes.CellPhone
    );

    if (phoneNotifications.every(x => x.userNotificationId === GuidEmpty && (!x.notificationValue || x.notificationValue.trim() === ''))) {
      return false;
    }
    return true;
  }

  populateSelectList(user: User) {
    if (user && user.availableCarrierScacs) {
      user.availableCarrierScacsSelectItems = user.availableCarrierScacs.map(selectItem => {
        return { label: selectItem, value: selectItem };
      });
    }

    return user;
  }

  buildFocusEntitiesResult(focusEntities: UserFocusEntity[]) {
    const focusEntitySelectItemGroup = new Array<SelectItemGroup>();
    if (focusEntities.length > 0) {
      const groupedFocusEntities = groupBy(userFocusEntity => userFocusEntity.group, focusEntities);

      groupedFocusEntities.forEach(element => {
        focusEntitySelectItemGroup.push({
          label: (element.key || '').toString(),
          items: element.items.map(userFocusEntity => {
            return { label: userFocusEntity.name, value: userFocusEntity };
          }),
        });
      });
    }

    return { focusEntites: focusEntities, groupedFocusEntities: focusEntitySelectItemGroup };
  }
}
