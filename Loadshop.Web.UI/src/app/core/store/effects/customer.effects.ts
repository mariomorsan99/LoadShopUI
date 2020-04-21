import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, mapTo, switchMap } from 'rxjs/operators';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services/common.service';
import { CustomerActionTypes, CustomerLoadAction, CustomerLoadFailureAction, CustomerLoadSuccessAction } from '../actions';

@Injectable()
export class CustomerEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType(CustomerActionTypes.Load),
    mapToPayload<{ customerId: string }>(),
    switchMap((payload: { customerId: string }) => {
      return this.commonService.getCustomer(payload.customerId).pipe(
        map(data => new CustomerLoadSuccessAction(data)),
        catchError(err => of(new CustomerLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $test: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    mapTo(new CustomerLoadAction({ customerId: '70C0DDC2-35D8-4003-BEE8-85D6EA973781' }))
  );

  constructor(private commonService: CommonService, private actions$: Actions) {}
}
