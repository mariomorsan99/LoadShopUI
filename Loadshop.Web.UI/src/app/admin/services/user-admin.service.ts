import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CarrierScac,
  Customer,
  IdentityUserData,
  ISecurityAccessRoleData,
  ServiceResponse,
  UserAdminData,
  Carrier
} from '../../shared/models';
import { mapResponse } from '../../shared/operators/map-response';

@Injectable()
export class UserAdminService {
  constructor(private http: HttpClient) {}

  getAllMyAuthorizedShippers(): Observable<Customer[]> {
    return this.http.get<ServiceResponse<Customer[]>>(`${environment.apiUrl}/api/UserAdmin/GetAllMyAuthorizedShippers`).pipe(mapResponse());
  }

  getAllMyAuthroizedCarrierScacs(): Observable<CarrierScac[]> {
    return this.http
      .get<ServiceResponse<CarrierScac[]>>(`${environment.apiUrl}/api/UserAdmin/GetAllMyAuthorizedCarrierScacs`)
      .pipe(mapResponse());
  }

  getAllMyAuthorizedSecurityRoles(): Observable<ISecurityAccessRoleData[]> {
    return this.http
      .get<ServiceResponse<ISecurityAccessRoleData[]>>(`${environment.apiUrl}/api/UserAdmin/GetAllMyAuthorizedSecurityRoles`)
      .pipe(mapResponse());
  }

  getUsers(query: string): Observable<UserAdminData[]> {
    return this.http.get<ServiceResponse<UserAdminData[]>>(`${environment.apiUrl}/api/useradmin/?query=${query}`).pipe(mapResponse());
  }

  getAdminUsers(): Observable<UserAdminData[]> {
    return this.http.get<ServiceResponse<UserAdminData[]>>(`${environment.apiUrl}/api/useradmin/GetAllAdminUsers`).pipe(mapResponse());
  }

  getUser(userId: string): Observable<UserAdminData> {
    return this.http.get<ServiceResponse<UserAdminData>>(`${environment.apiUrl}/api/useradmin/${userId}`).pipe(mapResponse());
  }

  getIdentityUser(userName: string): Observable<IdentityUserData> {
    return this.http
      .get<ServiceResponse<IdentityUserData>>(`${environment.apiUrl}/api/useradmin/GetIdentityUserForCreate/${userName}`)
      .pipe(mapResponse());
  }

  updateUser(user: UserAdminData): Observable<UserAdminData> {
    return this.http.put<ServiceResponse<UserAdminData>>(`${environment.apiUrl}/api/useradmin/${user.userId}`, user).pipe(mapResponse());
  }

  createUser(user: UserAdminData): Observable<UserAdminData> {
    return this.http.post<ServiceResponse<UserAdminData>>(`${environment.apiUrl}/api/useradmin/`, user).pipe(mapResponse());
  }

  getAllMyAuthorizedCarriers(): Observable<Carrier[]> {
    return this.http
      .get<ServiceResponse<Carrier[]>>(`${environment.apiUrl}/api/UserAdmin/GetAllMyAuthorizedCarriers`)
      .pipe(mapResponse());
  }

  getCarrierUsers(carrierId: string): Observable<UserAdminData[]> {
    return this.http
      .get<ServiceResponse<UserAdminData[]>>(`${environment.apiUrl}/api/UserAdmin/GetCarrierUsers/${carrierId}`)
      .pipe(mapResponse());
  }
}
