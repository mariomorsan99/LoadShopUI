import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, mergeMap } from 'rxjs/operators';
import { CommonService } from '../../services/common.service';
import {
  SmartSpotPriceActionTypes,
  SmartSpotPriceLoadFailureAction,
  SmartSpotPriceLoadSuccessAction,
  SmartSpotPriceLoadAction,
  SmartSpotPriceLoadQuoteAction,
  SmartSpotPriceLoadQuoteSuccessAction,
  SmartSpotPriceLoadQuoteFailureAction,
  SmartSpotPriceHideQuickQuoteAction,
} from '../actions';
import { Router } from '@angular/router';

@Injectable()
export class SmartSpotPriceEffects {
  @Effect()
  $load: Observable<Action> = this.actions$.pipe(
    ofType(SmartSpotPriceActionTypes.Load),
    mergeMap((action: SmartSpotPriceLoadAction) => {
      return this.commonService.getSmartSpotPrice(action.payload).pipe(
        map(data => new SmartSpotPriceLoadSuccessAction(data)),
        catchError(err => of(new SmartSpotPriceLoadFailureAction(err)))
      );
    })
  );

  @Effect()
  $loadQuote: Observable<Action> = this.actions$.pipe(
    ofType(SmartSpotPriceActionTypes.LoadQuote),
    switchMap((action: SmartSpotPriceLoadQuoteAction) => {
      return this.commonService.getSmartSpotQuote(action.payload).pipe(
        map(data => new SmartSpotPriceLoadQuoteSuccessAction(data)),
        catchError(err => of(new SmartSpotPriceLoadQuoteFailureAction(err)))
      );
    })
  );

  @Effect()
  $createOrderFromQuote: Observable<Action> = this.actions$.pipe(
    ofType(SmartSpotPriceActionTypes.CreateOrderFromQuote),
    switchMap(() => {
      this.router.navigate(['/shipping/home/create']);
      return of(new SmartSpotPriceHideQuickQuoteAction());
    })
  );

  constructor(private commonService: CommonService, private actions$: Actions, private router: Router) {}
}
