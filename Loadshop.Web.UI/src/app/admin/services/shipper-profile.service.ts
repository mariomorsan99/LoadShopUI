import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ServiceResponse,
  Customer,
  CustomerProfile
} from '../../shared/models';
import { environment } from '../../../environments/environment';
import { mapResponse } from '../../shared/operators/map-response';
import { map } from 'rxjs/operators';
import { LoadshopShipperMapping } from 'src/app/shared/models/loadshop-shipper-mapping';

@Injectable()
export class ShipperProfileService {
  constructor(private http: HttpClient) { }

  getAllShippers(): Observable<Customer[]> {
    return this.http.get<ServiceResponse<Customer[]>>(`${environment.apiUrl}/api/ShipperAdmin/GetAllShippers`).pipe(
      mapResponse()
    );
  }

  getShipper(customerId: string): Observable<CustomerProfile> {
    return this.http.get<ServiceResponse<CustomerProfile>>(`${environment.apiUrl}/api/ShipperAdmin/GetShipper?customerId=${customerId}`)
      .pipe(
        mapResponse(),
        map(convertExpirationDate)
      );
  }

  createShipper(customer: CustomerProfile): Observable<CustomerProfile> {
    return this.http.put<ServiceResponse<CustomerProfile>>(`${environment.apiUrl}/api/ShipperAdmin/`, customer)
      .pipe(
        mapResponse(),
        map(convertExpirationDate)
      );
  }

  updateShipper(customer: CustomerProfile): Observable<CustomerProfile> {
    return this.http.post<ServiceResponse<CustomerProfile>>(`${environment.apiUrl}/api/ShipperAdmin/`, customer)
      .pipe(
        mapResponse(),
        map(convertExpirationDate)
      );
  }

  getShipperMappings(ownerId: string): Observable<LoadshopShipperMapping[]> {
    return this.http.get<ServiceResponse<LoadshopShipperMapping[]>>(
      `${environment.apiUrl}/api/ShipperAdmin/GetShipperMappings?ownerId=${ownerId}`
      ).pipe(
        mapResponse(),
      );
  }

  getSourceSystemOwners(ownerId: string): Observable<Map<string, string[]>> {
    return this.http.get<ServiceResponse<Map<string, string[]>>>(
      `${environment.apiUrl}/api/ShipperAdmin/GetSourceSystemOwners?ownerId=${ownerId}`
      ).pipe(
        mapResponse(),
      );
  }

  updateShipperMapping(mapping: LoadshopShipperMapping): Observable<LoadshopShipperMapping> {
    return this.http.put<ServiceResponse<LoadshopShipperMapping>>(
      `${environment.apiUrl}/api/ShipperAdmin/ShipperMapping`, mapping
      ).pipe(
        mapResponse(),
      );
  }

  createShipperMapping(mapping: LoadshopShipperMapping): Observable<LoadshopShipperMapping> {
    return this.http.post<ServiceResponse<LoadshopShipperMapping>>(
      `${environment.apiUrl}/api/ShipperAdmin/ShipperMapping`, mapping
      ).pipe(
        mapResponse(),
      );
  }

  setupCustomerApi(customer: CustomerProfile): Observable<CustomerProfile> {
    return this.http.post<ServiceResponse<CustomerProfile>>(`${environment.apiUrl}/api/ShipperAdmin/SetupCustomerApi/`, customer)
      .pipe(
        mapResponse(),
        map(convertExpirationDate)
      );
  }
}

function convertExpirationDate(customer: CustomerProfile): CustomerProfile {
  if (customer) {
    if (customer.customerLoadTypeExpirationDate && customer.customerLoadTypeExpirationDate.length) {
      customer.customerLoadTypeExpirationDate = new Date(customer.customerLoadTypeExpirationDate);
    }
  }

  return customer;
}

