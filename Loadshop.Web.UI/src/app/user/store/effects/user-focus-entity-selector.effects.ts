import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import { NavigationGoAction } from '@tms-ng/core';
import { mapToPayload } from '@tms-ng/shared';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { UserFocusEntity } from '../../../shared/models';
import { LoadshopApplicationActionTypes } from '../../../shared/store';
import { UserProfileService } from '../../services';
import {
  AllMyAuthorizedEntitiesLoadAction,
  AllMyAuthorizedEntitiesLoadFail,
  AllMyAuthorizedEntitiesLoadSuccess,
  UpdateFocusEntityAction,
  UpdateFocusEntityFailAction,
  UpdateFocusEntitySuccessAction,
  UserFocusEntitySelectorTypes,
  UserProfileUpdateSuccessAction,
} from '../actions';

@Injectable()
export class UserFocusEntitySelectorEffects {
  @Effect()
  $loadAllMyCarrierScacs: Observable<Action> = this.actions$.pipe(
    ofType<AllMyAuthorizedEntitiesLoadAction>(UserFocusEntitySelectorTypes.LoadMyAuthorizedEntities),
    mapToPayload<string>(),
    switchMap(() => {
      return this.userProfileService.getAllMyAuthorizedEntities().pipe(
        map(data => new AllMyAuthorizedEntitiesLoadSuccess(data)),
        catchError(err => of(new AllMyAuthorizedEntitiesLoadFail(err)))
      );
    })
  );

  @Effect()
  $update: Observable<Action> = this.actions$.pipe(
    ofType<UpdateFocusEntityAction>(UserFocusEntitySelectorTypes.UpdateFocusEntity),
    mapToPayload<UserFocusEntity>(),
    switchMap((focusEntity: UserFocusEntity) => this.userProfileService.updateFocusEntity(focusEntity)),
    switchMap(user => [new UpdateFocusEntitySuccessAction(user), new UserProfileUpdateSuccessAction(user)]),
    catchError(err => of(new UpdateFocusEntityFailAction(err)))
  );

  @Effect()
  $startup: Observable<Action> = this.actions$.pipe(
    ofType(LoadshopApplicationActionTypes.LoadshopStart),
    switchMap(() => {
      return this.userProfileService.getAllMyAuthorizedEntities().pipe(
        map(data => new AllMyAuthorizedEntitiesLoadSuccess(data)),
        catchError(err => {
          if (!(err instanceof HttpErrorResponse)) {
            return of(new NavigationGoAction({ path: ['invalid'] }));
          }
        })
      );
    })
  );

  constructor(private actions$: Actions, private userProfileService: UserProfileService) {}
}
