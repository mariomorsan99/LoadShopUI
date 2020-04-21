import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { ServiceResponse, State } from '../../shared/models';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { map } from 'rxjs/operators';

@Injectable({providedIn: 'root'})
export class StateService {
  constructor(private http: HttpClient) { }

  getStates(): Observable<State[]> {
    return this.http.get<ServiceResponse<State[]>>(environment.apiUrl + '/api/States/').pipe(
      map(x => x.data)
    );
  }
}

