import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action, Store } from '@ngrx/store';
import { AuthLoginActionTypes, NavigationGoAction } from '@tms-ng/core';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of, timer } from 'rxjs';
import { catchError, filter, map, mapTo, mergeMap, switchMap, takeUntil, withLatestFrom } from 'rxjs/operators';
import { LoadBoardService } from 'src/app/load-board/services';
import { getUserProfileModel, UserFocusEntitySelectorTypes, UserProfileActionTypes } from 'src/app/user/store';
import { User } from '../../../shared/models';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services';
import {
  MenuActionTypes,
  MenuLoadAction,
  MenuLoadFailureAction,
  MenuLoadSuccessAction,
  MenuUpdateFailureAction,
  MenuUpdateSuccessAction,
  MenuVisibilityBadgeLoadAction,
  MenuVisibilityBadgeLoadFailureAction,
  MenuVisibilityBadgeLoadSuccessAction,
} from '../actions';
import { CoreState } from '../reducers';

@Injectable()
export class MenuEffects {
  visibilityBadgePollingIntervalInSeconds: number = 2 * 60; // 2 mins

  @Effect()
  $login: Observable<Action> = this.actions$.pipe(
    ofType(MenuActionTypes.Load, UserProfileActionTypes.Load_Success),
    withLatestFrom(this.store$.pipe(map(getUserProfileModel))),
    switchMap(([_, user]) => {
      return this.menuService.getMenuItems(user).pipe(
        map(data => {
          return new MenuLoadSuccessAction(data);
        }),
        catchError(err => {
          return of(new MenuLoadFailureAction(err));
        })
      );
    })
  );

  @Effect()
  $profileUpdate: Observable<Action> = this.actions$.pipe(
    ofType(UserFocusEntitySelectorTypes.UpdateFocusEntity),
    switchMap(() => [new NavigationGoAction({ path: ['/change-entity'] }), new MenuUpdateSuccessAction([])])
  );

  @Effect()
  $profileUpdateSuccess: Observable<Action> = this.actions$.pipe(
    ofType(UserProfileActionTypes.Update_Success),
    withLatestFrom(this.store$.pipe(map(getUserProfileModel))),
    switchMap(([_, user]) => {
      return this.menuService.getMenuItems(user).pipe(
        mergeMap(data => [new MenuUpdateSuccessAction(data), new NavigationGoAction({ path: ['/'] })]),
        catchError(err => {
          return of(new MenuUpdateFailureAction(err));
        })
      );
    })
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    switchMap(() => {
      return of(new MenuLoadAction());
    })
  );

  /**
   * Load the Visibility Badge data from the API after the user profile is loaded
   */
  // TODO: We can probably eliminate this and replace with a start/cancel polling call when the app components tab menu
  // is inited/destroyed and once more structure is around the different pages that may need to be moved to whatever container
  // ends up holding the p-tabmenu
  @Effect()
  $polling: Observable<Action> = this.actions$.pipe(
    ofType(UserProfileActionTypes.Load_Success),
    mapToPayload<User>(),
    filter(user => user && user.isCarrier && user.carrierVisibilityTypes && user.carrierVisibilityTypes.length > 0),
    switchMap(() =>
      timer(0, this.visibilityBadgePollingIntervalInSeconds * 1000).pipe(
        takeUntil(this.actions$.pipe(ofType(AuthLoginActionTypes.Logout))),
        mapTo(new MenuVisibilityBadgeLoadAction())
      )
    )
  );

  @Effect()
  $loadVisibilityBadge: Observable<Action> = this.actions$.pipe(
    ofType<MenuVisibilityBadgeLoadAction>(MenuActionTypes.Visibility_Badge_Load),
    withLatestFrom(this.store$.pipe(map(getUserProfileModel))),
    map(([, user]) => user),
    filter(user => user && user.isCarrier && user.carrierVisibilityTypes && user.carrierVisibilityTypes.length > 0),
    switchMap(() => {
      return this.getVisibilityBadge();
    })
  );

  getVisibilityBadge() {
    return this.loadService.getNumLoadsRequiringVisibilityInfo().pipe(
      map(data => new MenuVisibilityBadgeLoadSuccessAction(data)),
      catchError(err => of(new MenuVisibilityBadgeLoadFailureAction(err)))
    );
  }

  constructor(
    private menuService: CommonService,
    private actions$: Actions,
    private loadService: LoadBoardService,
    private store$: Store<CoreState>
  ) {}
}
