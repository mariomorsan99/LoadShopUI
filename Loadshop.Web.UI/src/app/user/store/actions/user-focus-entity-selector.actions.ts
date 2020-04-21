/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';
import { User, UserFocusEntity, UserFocusEntityResult } from 'src/app/shared/models';

export enum UserFocusEntitySelectorTypes {
  LoadMyAuthorizedEntities = '[LoadMyAuthorizedEntities] LOAD',
  LoadMyAuthorizedEntities_Success = '[LoadMyAuthorizedEntities] LOAD_SUCCESS',
  LoadMyAuthorizedEntities_Fail = '[LoadMyAuthorizedEntities] LOAD_FAILURE',
  UpdateFocusEntity = '[UpdateFocusEntity] UPDATE',
  UpdateFocusEntity_Success = '[LoadMyAuthorizedEntities] UPDATE_SUCCESS',
  UpdateFocusEntity_Fail = '[LoadMyAuthorizedEntities] UPDATE_FAILURE',
}

export class AllMyAuthorizedEntitiesLoadAction implements Action {
  readonly type = UserFocusEntitySelectorTypes.LoadMyAuthorizedEntities;
}

export class AllMyAuthorizedEntitiesLoadSuccess implements Action {
  readonly type = UserFocusEntitySelectorTypes.LoadMyAuthorizedEntities_Success;

  constructor(public payload: UserFocusEntityResult) {}
}

export class AllMyAuthorizedEntitiesLoadFail implements Action {
  readonly type = UserFocusEntitySelectorTypes.LoadMyAuthorizedEntities_Fail;

  constructor(public payload: Error) {}
}

export class UpdateFocusEntityAction implements Action {
  readonly type = UserFocusEntitySelectorTypes.UpdateFocusEntity;

  constructor(public payload: UserFocusEntity) {}
}

export class UpdateFocusEntitySuccessAction implements Action {
  readonly type = UserFocusEntitySelectorTypes.UpdateFocusEntity_Success;

  constructor(public payload: User) {}
}

export class UpdateFocusEntityFailAction implements Action {
  readonly type = UserFocusEntitySelectorTypes.UpdateFocusEntity_Fail;

  constructor(public payload: Error) {}
}

export type UserFocusEntitySelectorActions =
  | AllMyAuthorizedEntitiesLoadAction
  | AllMyAuthorizedEntitiesLoadSuccess
  | AllMyAuthorizedEntitiesLoadFail
  | UpdateFocusEntityAction
  | UpdateFocusEntitySuccessAction
  | UpdateFocusEntityFailAction;
