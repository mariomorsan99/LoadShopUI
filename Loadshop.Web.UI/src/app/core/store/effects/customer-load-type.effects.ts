import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { Effect, Actions, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { CommonService } from '../../services/common.service';
import { map, switchMap, catchError } from 'rxjs/operators';
import { CustomerLoadTypeActionTypes, CustomerLoadTypeLoadSuccessAction, CustomerLoadTypeLoadFailureAction } from '../actions';

@Injectable()
export class CustomerLoadTypeEffects {

     @Effect()
     $load: Observable<Action> = this.actions$.pipe(
         ofType(CustomerLoadTypeActionTypes.Load),
         switchMap(() => {
             return this.commonService.getCustomerLoadTypes().pipe(
                 map((data) => new CustomerLoadTypeLoadSuccessAction(data)),
                 catchError((err) => of(new CustomerLoadTypeLoadFailureAction(err)))
             );
         })
     );

    constructor(private commonService: CommonService, private actions$: Actions) { }
}
