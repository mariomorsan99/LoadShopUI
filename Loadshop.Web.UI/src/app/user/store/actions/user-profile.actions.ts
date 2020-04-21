/* eslint-disable @typescript-eslint/camelcase */
import { HttpErrorResponse } from '@angular/common/http';
import { Action } from '@ngrx/store';
import { LoadStatusNotificationsData, User } from '../../../shared/models';

export enum UserProfileActionTypes {
  Load = '[CustomerProfile] LOAD',
  Load_Success = '[CustomerProfile] LOAD_SUCCESS',
  Load_Failure = '[CustomerProfile] LOAD_FAILURE',
  Update = '[CustomerProfile] UPDATE',
  Update_Success = '[CustomerProfile] UPDATE_SUCCESS',
  Update_Failure = '[CustomerProfile] UPDATE_FAILURE',
  LoadStatusNotifications_Load = '[CustomerProfile] LOAD_STATUS_NOTIFICATIONS_LOAD',
  LoadStatusNotifications_Success = '[CustomerProfile] LOAD_STATUS_NOTIFICATIONS_LOAD_SUCCESS',
  LoadStatusNotifications_Failure = '[CustomerProfile] LOAD_STATUS_NOTIFICATIONS_LOAD_FAILURE',
  LoadStatusNotificationsUpdate = '[CustomerProfile] LOAD_STATUS_NOTIFICATIONS_UPDATE',
  LoadStatusNotificationsUpdate_Success = '[CustomerProfile] LOAD_STATUS_NOTIFICATIONS_UPDATE_SUCCESS',
  LoadStatusNotificationsUpdate_Failure = '[CustomerProfile] LOAD_STATUS_NOTIFICATIONS_UPDATE_FAILURE',
}

export class UserProfileLoadAction implements Action {
  readonly type = UserProfileActionTypes.Load;
}

export class UserProfileLoadSuccessAction implements Action {
  readonly type = UserProfileActionTypes.Load_Success;

  constructor(public payload: User) {}
}

export class UserProfileLoadFailureAction implements Action {
  readonly type = UserProfileActionTypes.Load_Failure;

  constructor(public payload: Error) {}
}

export class UserProfileUpdateAction implements Action {
  readonly type = UserProfileActionTypes.Update;

  constructor(public payload: User) {}
}

export class UserProfileUpdateSuccessAction implements Action {
  readonly type = UserProfileActionTypes.Update_Success;

  constructor(public payload: User) {}
}

export class UserProfileUpdateFailureAction implements Action {
  readonly type = UserProfileActionTypes.Update_Failure;

  constructor(public payload: HttpErrorResponse) {}
}

export class LoadStatusNotificationsLoadAction implements Action {
  readonly type = UserProfileActionTypes.LoadStatusNotifications_Load;
}

export class LoadStatusNotificationsLoadSuccessAction implements Action {
  readonly type = UserProfileActionTypes.LoadStatusNotifications_Success;

  constructor(public payload: LoadStatusNotificationsData) {}
}

export class LoadStatusNotificationsLoadFailureAction implements Action {
  readonly type = UserProfileActionTypes.LoadStatusNotifications_Failure;

  constructor(public payload: HttpErrorResponse) {}
}

export class LoadStatusNotificationsUpdateAction implements Action {
  readonly type = UserProfileActionTypes.LoadStatusNotificationsUpdate;

  constructor(public payload: LoadStatusNotificationsData) {}
}

export class LoadStatusNotificationsUpdateSuccessAction implements Action {
  readonly type = UserProfileActionTypes.LoadStatusNotificationsUpdate_Success;

  constructor(public payload: LoadStatusNotificationsData) {}
}

export class LoadStatusNotificationsUpdateFailureAction implements Action {
  readonly type = UserProfileActionTypes.LoadStatusNotificationsUpdate_Failure;

  constructor(public payload: HttpErrorResponse) {}
}

export type UserProfileActions =
  | UserProfileLoadAction
  | UserProfileLoadSuccessAction
  | UserProfileLoadFailureAction
  | UserProfileUpdateAction
  | UserProfileUpdateSuccessAction
  | UserProfileUpdateFailureAction
  | LoadStatusNotificationsLoadAction
  | LoadStatusNotificationsLoadSuccessAction
  | LoadStatusNotificationsLoadFailureAction
  | LoadStatusNotificationsUpdateAction
  | LoadStatusNotificationsUpdateSuccessAction
  | LoadStatusNotificationsUpdateFailureAction;
