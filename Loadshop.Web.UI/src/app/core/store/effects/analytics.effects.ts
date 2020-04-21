/* eslint-disable @typescript-eslint/camelcase */
import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { RouterNavigationAction, ROUTER_NAVIGATION } from '@ngrx/router-store';
import { Action } from '@ngrx/store';
import { AuthLoginSuccessAction } from '@tms-ng/core';
import { defer, Observable, of, timer } from 'rxjs';
import { delayWhen, map, retryWhen, tap } from 'rxjs/operators';
import {
  LoadBoardLoadBookActionTypes,
  LoadBoardLoadBookSuccessAction,
  LoadBoardLoadDetailActionTypes,
  LoadBoardLoadDetailLoadSuccessAction,
  LoadshopApplicationActionTypes,
} from '../../../shared/store';
import { AnalyticsService } from '../../services/google-analytics.service';

declare let window;

@Injectable()
export class AnalyticsEffects {
  @Effect({ dispatch: false })
  $login: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    tap((action: AuthLoginSuccessAction) => {
      // this.analyticsService.login(action.payload);
      this.analyticsService.addEvent('login', {
        method: 'IdentityServer',
      });
    })
  );

  @Effect({ dispatch: false })
  $route: Observable<Action> = this.actions$.pipe(
    ofType(ROUTER_NAVIGATION),
    tap((action: RouterNavigationAction) => {
      const route = action.payload;
      this.analyticsService.addEvent('pageview', {
        event_label: route.routerState.url,
        event_category: 'routing',
      });
    })
  );

  @Effect({ dispatch: false })
  $detail: Observable<Action> = this.actions$.pipe(
    ofType(LoadBoardLoadDetailActionTypes.Load_Success),
    tap((action: LoadBoardLoadDetailLoadSuccessAction) => {
      const load = action.payload;
      this.analyticsService.addEvent('detail', {
        event_label: load.referenceLoadDisplay,
        event_category: 'detail',
      });
    })
  );

  @Effect({ dispatch: false })
  $booked: Observable<Action> = this.actions$.pipe(
    ofType<LoadBoardLoadBookSuccessAction>(LoadBoardLoadBookActionTypes.Book_Success),
    tap((action: LoadBoardLoadBookSuccessAction) => {
      const load = action.payload;
      this.analyticsService.addEvent('booked', {
        event_label: load.referenceLoadDisplay,
        event_category: 'detail',
      });
    })
  );

  @Effect()
  $startup: Observable<Action> = defer(() => of(null)).pipe(
    map(() => {
      if (window.gtag) {
        this.analyticsService.initialize();
      }
      throw new Error('google.analytics not loaded yet');
    }),
    retryWhen(errors => errors.pipe(delayWhen(() => timer(10))))
  );

  constructor(private analyticsService: AnalyticsService, private actions$: Actions) {}
}
