import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { Effect, Actions, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { CommonService } from '../../services/common.service';
import { map, switchMap, catchError } from 'rxjs/operators';
import {
    CarrierActionTypes,
    CarrierLoadSuccessAction,
    CarrierLoadFailureAction,
    CarrierCarrierScacLoadSuccessAction,
    CarrierCarrierScacLoadFailureAction,
} from '../actions';

@Injectable()
export class CarrierEffects {
    @Effect()
    $load: Observable<Action> = this.actions$.pipe(
        ofType(CarrierActionTypes.Load),
        switchMap(() => {
            return this.commonService.getCarriers().pipe(
                map(data => new CarrierLoadSuccessAction(data)),
                catchError(err => of(new CarrierLoadFailureAction(err))),
            );
        }),
    );

    @Effect()
    $loadCarrierCarrierScacs: Observable<Action> = this.actions$.pipe(
        ofType(CarrierActionTypes.CarrierCarrierScacLoad),
        switchMap(() => {
            return this.commonService.getAllCarrierCarrierScacs().pipe(
                map(data => new CarrierCarrierScacLoadSuccessAction(data)),
                catchError(err =>
                    of(new CarrierCarrierScacLoadFailureAction(err)),
                ),
            );
        }),
    );

    constructor(
        private commonService: CommonService,
        private actions$: Actions,
    ) {}
}
