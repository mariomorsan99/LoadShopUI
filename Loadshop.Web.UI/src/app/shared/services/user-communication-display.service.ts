import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { mapResponse } from '../../shared/operators/map-response';
import { map } from 'rxjs/operators';
import { UserCommunication, ServiceResponse } from '../models';

@Injectable()
export class UserCommunicationDisplayService {
    constructor(private http: HttpClient) { }

    getUserCommunicationsForDisplay(): Observable<UserCommunication[]> {
        return this.http.get<ServiceResponse<UserCommunication[]>>(
            `${environment.apiUrl}/api/UserCommunication/GetUserCommunicationsForDisplay`)
            .pipe(mapResponse(), map(items => items.map(item => this.setDates(item))));
    }

    acknowledgeUserCommunication(userCommunicationId: string): Observable<UserCommunication[]> {
        return this.http.post<ServiceResponse<UserCommunication[]>>(
            `${environment.apiUrl}/api/UserCommunication/Acknowledge`, { userCommunicationId })
            .pipe(mapResponse());
    }

    private setDates<T extends UserCommunication>(userCommunication: T) {
        userCommunication.effectiveDate = userCommunication.effectiveDate ? new Date(userCommunication.effectiveDate) : null;
        userCommunication.expirationDate = userCommunication.expirationDate ? new Date(userCommunication.expirationDate) : null;
        return userCommunication;
    }
}
