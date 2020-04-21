/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';
import { MenuItem } from 'primeng/api';
import { VisibilityBadge } from '../../../shared/models';

export enum MenuActionTypes {
  Load = '[Menu] LOAD',
  Load_Success = '[Menu] LOAD_SUCCESS',
  Load_Failure = '[Menu] LOAD_FAILURE',
  Update = '[Menu] UPDATE',
  Update_Success = '[Menu] UPDATE_SUCCESS',
  Update_Failure = '[Menu] UPDATE_FAILURE',
  Visibility_Badge_Load = '[Menu] VISIBILITY_BADGE_LOAD',
  Visibility_Badge_Load_Success = '[Menu] VISIBILITY_BADGE_LOAD_SUCCESS',
  Visibility_Badge_Load_Failure = '[Menu] VISIBILITY_BADGE_LOAD_FAILURE',
}

export class MenuLoadAction implements Action {
  readonly type = MenuActionTypes.Load;
}

export class MenuLoadSuccessAction implements Action {
  readonly type = MenuActionTypes.Load_Success;

  constructor(public payload: MenuItem[]) {}
}

export class MenuLoadFailureAction implements Action {
  readonly type = MenuActionTypes.Load_Failure;

  constructor(public payload: Error) {}
}

export class MenuUpdateAction implements Action {
  readonly type = MenuActionTypes.Update;
}

export class MenuUpdateSuccessAction implements Action {
  readonly type = MenuActionTypes.Update_Success;

  constructor(public payload: MenuItem[]) {}
}

export class MenuUpdateFailureAction implements Action {
  readonly type = MenuActionTypes.Update_Failure;

  constructor(public payload: Error) {}
}

export class MenuVisibilityBadgeLoadAction implements Action {
  readonly type = MenuActionTypes.Visibility_Badge_Load;
}

export class MenuVisibilityBadgeLoadSuccessAction implements Action {
  readonly type = MenuActionTypes.Visibility_Badge_Load_Success;

  constructor(public payload: VisibilityBadge) {}
}

export class MenuVisibilityBadgeLoadFailureAction implements Action {
  readonly type = MenuActionTypes.Visibility_Badge_Load_Failure;

  constructor(public payload: Error) {}
}

export type MenuActions =
  | MenuLoadAction
  | MenuLoadSuccessAction
  | MenuLoadFailureAction
  | MenuUpdateAction
  | MenuUpdateSuccessAction
  | MenuUpdateFailureAction
  | MenuVisibilityBadgeLoadAction
  | MenuVisibilityBadgeLoadSuccessAction
  | MenuVisibilityBadgeLoadFailureAction;
