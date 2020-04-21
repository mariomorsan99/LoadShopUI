import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action, Store } from '@ngrx/store';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, withLatestFrom } from 'rxjs/operators';
import { getUserProfileModel, UserProfileActionTypes } from 'src/app/user/store';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { CommonService } from '../../services';
import { AdminMenuActionTypes, AdminMenuLoadAction, AdminMenuLoadFailureAction, AdminMenuLoadSuccessAction } from '../actions';
import { CoreState } from '../reducers';

@Injectable()
export class AdminMenuEffects {
  @Effect()
  $login: Observable<Action> = this.actions$.pipe(
    ofType(AdminMenuActionTypes.Load, UserProfileActionTypes.Load_Success),
    withLatestFrom(this.store$.pipe(map(getUserProfileModel))),
    switchMap(([_, user]) => {
      return this.menuService.getAdminMenuItems(user).pipe(
        map(data => {
          return new AdminMenuLoadSuccessAction(data);
        }),
        catchError(err => {
          return of(new AdminMenuLoadFailureAction(err));
        })
      );
    })
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    switchMap(() => {
      return of(new AdminMenuLoadAction());
    })
  );

  constructor(private menuService: CommonService, private actions$: Actions, private store$: Store<CoreState>) {}
}
