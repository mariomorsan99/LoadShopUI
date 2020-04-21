import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RatingQuestion, ServiceResponse } from '../../shared/models';
import { mapResponse } from '../../shared/operators/map-response';

@Injectable()
export class RatingService {
  constructor(private http: HttpClient) {}

  getRatingQuestions(): Observable<RatingQuestion[]> {
    return this.http.get<ServiceResponse<RatingQuestion[]>>(environment.apiUrl + '/api/ratings/questions').pipe(mapResponse());
  }
}
