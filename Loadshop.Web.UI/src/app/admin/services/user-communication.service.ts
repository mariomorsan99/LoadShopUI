import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceResponse, UserCommunication, UserCommunicationDetail } from '../../shared/models';
import { mapResponse } from '../../shared/operators/map-response';
import { map } from 'rxjs/operators';

@Injectable()
export class UserCommunicationService {
    constructor(private http: HttpClient) { }

    getUserCommunications(): Observable<UserCommunication[]> {
        return this.http.get<ServiceResponse<UserCommunication[]>>(`${environment.apiUrl}/api/UserCommunication/`)
            .pipe(mapResponse(), map(items => items.map(item => this.setDates(item))));
    }

    getUserCommunication(userCommunicationId: string): Observable<UserCommunicationDetail> {
        return this.http.get<ServiceResponse<UserCommunicationDetail>>(`${environment.apiUrl}/api/UserCommunication/${userCommunicationId}`)
            .pipe(mapResponse(), this.mapDates());
    }

    updateUserCommunication(userCommunication: UserCommunicationDetail): Observable<UserCommunicationDetail> {
        return this.http.put<ServiceResponse<UserCommunicationDetail>>(`${environment.apiUrl}/api/UserCommunication/`, userCommunication)
            .pipe(mapResponse(), this.mapDates());
    }

    createUserCommunication(userCommunication: UserCommunicationDetail): Observable<UserCommunicationDetail> {
        return this.http.post<ServiceResponse<UserCommunicationDetail>>(`${environment.apiUrl}/api/UserCommunication/`, userCommunication)
            .pipe(mapResponse(), this.mapDates());
    }

    private mapDates<T extends UserCommunication>() {

        return (userCommunication$: Observable<T>) => userCommunication$.pipe(map(userCommunication => {
            return this.setDates<T>(userCommunication);
        }));

    }

    private setDates<T extends UserCommunication>(userCommunication: T) {
        userCommunication.effectiveDate = userCommunication.effectiveDate ? new Date(userCommunication.effectiveDate) : null;
        userCommunication.expirationDate = userCommunication.expirationDate ? new Date(userCommunication.expirationDate) : null;
        return userCommunication;
    }
}
