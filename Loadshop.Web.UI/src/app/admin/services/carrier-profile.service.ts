import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
    ServiceResponse,
    CarrierProfile,
    CarrierScac,
    Carrier
} from '../../shared/models';
import { environment } from '../../../environments/environment';
import { mapResponse } from '../../shared/operators/map-response';
import { map } from 'rxjs/operators';

@Injectable()
export class CarrierProfileService {
    constructor(private http: HttpClient) { }

    getCarrier(carrierId: string): Observable<CarrierProfile> {
        return this.http.get<ServiceResponse<CarrierProfile>>(`${environment.apiUrl}/api/CarrierAdmin/${carrierId}`)
            .pipe(
                mapResponse(),
                map(carrier => {
                    convertDate(carrier.scacs);
                    return carrier;
                })
            );
    }

    updateCarrier(carrier: CarrierProfile): Observable<CarrierProfile> {
        return this.http.put<ServiceResponse<CarrierProfile>>(`${environment.apiUrl}/api/CarrierAdmin/`, carrier)
            .pipe(
                mapResponse(),
                map(c => {
                    convertDate(c.scacs);
                    return c;
                })
            );
    }

    getAllCarriers(): Observable<Carrier[]> {
        return this.http.get<ServiceResponse<Carrier[]>>(`${environment.apiUrl}/api/CarrierAdmin`).pipe(mapResponse());
    }
}

function convertDate(carrierScacs: CarrierScac[]) {
    carrierScacs.forEach(scac => {
        if (scac) {
            if (scac.expirationDate && scac.expirationDate.length) {
                scac.expirationDate = new Date(scac.expirationDate);
            }
        }

        if (scac) {
            if (scac.effectiveDate && scac.effectiveDate.length) {
                scac.effectiveDate = new Date(scac.effectiveDate);
            }
        }
    });
}

