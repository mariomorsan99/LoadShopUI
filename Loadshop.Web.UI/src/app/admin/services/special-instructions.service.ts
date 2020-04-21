import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceResponse } from '../../shared/models';
import { SpecialInstructionData } from '../../shared/models/special-instruction-data';
import { mapResponse } from '../../shared/operators/map-response';

@Injectable()
export class SpecialInstructionsService {
  constructor(private http: HttpClient) {}

  getAll(customerId: string): Observable<SpecialInstructionData[]> {
    return this.http
      .get<ServiceResponse<SpecialInstructionData[]>>(`${environment.apiUrl}/api/SpecialInstructions?customerId=${customerId}`)
      .pipe(mapResponse());
  }

  get(specialInstructionId: number): Observable<SpecialInstructionData> {
    return this.http
      .get<ServiceResponse<SpecialInstructionData>>(`${environment.apiUrl}/api/SpecialInstructions/${specialInstructionId}`)
      .pipe(mapResponse());
  }

  update(instruction: SpecialInstructionData): Observable<SpecialInstructionData> {
    return this.http
      .put<ServiceResponse<SpecialInstructionData>>(
        `${environment.apiUrl}/api/SpecialInstructions/${instruction.specialInstructionId}`,
        instruction
      )
      .pipe(mapResponse());
  }

  add(instruction: SpecialInstructionData): Observable<SpecialInstructionData> {
    return this.http
      .post<ServiceResponse<SpecialInstructionData>>(`${environment.apiUrl}/api/SpecialInstructions/`, instruction)
      .pipe(mapResponse());
  }

  delete(instruction: SpecialInstructionData): Observable<object> {
    return this.http
      .delete<ServiceResponse<object>>(`${environment.apiUrl}/api/SpecialInstructions/${instruction.specialInstructionId}`)
      .pipe(mapResponse());
  }
}
