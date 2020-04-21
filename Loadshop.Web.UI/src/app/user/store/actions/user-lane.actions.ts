import { Action } from '@ngrx/store';
import { UserLane } from '../../../shared/models';

export enum UserLaneActionTypes {
  Load = '[CustomerLane] LOAD',
  Load_Success = '[CustomerLane] LOAD_SUCCESS',
  Load_Failure = '[CustomerLane] LOAD_FAILURE',
  Add = '[CustomerLane] ADD',
  Add_Success = '[CustomerLane] ADD_SUCCESS',
  Add_Failure = '[CustomerLane] ADD_FAILURE',
  Update = '[CustomerLane] UPDATE',
  Update_Success = '[CustomerLane] UPDATE_SUCCESS',
  Update_Failure = '[CustomerLane] UPDATE_FAILURE',
  Delete = '[CustomerLane] DELETE',
  Delete_Success = '[CustomerLane] DELETE_SUCCESS',
  Delete_Failure = '[CustomerLane] DELETE_FAILURE',
  Toggle_Display = '[CustomerLane] TOGGLE',
  ToggleAll_Display = '[CustomerLane] TOGGLEALL',
  Selected = '[CustomerLane] SELECTED_LANE',
}

export class UserLaneLoadAction implements Action {
  readonly type = UserLaneActionTypes.Load;

  constructor() {}
}

export class UserLaneLoadSuccessAction implements Action {
  readonly type = UserLaneActionTypes.Load_Success;

  constructor(public payload: UserLane[]) {}
}

export class UserLaneLoadFailureAction implements Action {
  readonly type = UserLaneActionTypes.Load_Failure;

  constructor(public payload: Error) {}
}

export class UserLaneAddAction implements Action {
  readonly type = UserLaneActionTypes.Add;

  constructor(public payload: UserLane) {}
}

export class UserLaneAddSuccessAction implements Action {
  readonly type = UserLaneActionTypes.Add_Success;

  constructor(public payload: UserLane) {}
}

export class UserLaneAddFailureAction implements Action {
  readonly type = UserLaneActionTypes.Add_Failure;

  constructor(public payload: Error) {}
}

export class UserLaneUpdateAction implements Action {
  readonly type = UserLaneActionTypes.Update;

  constructor(public payload: UserLane) {}
}

export class UserLaneUpdateSuccessAction implements Action {
  readonly type = UserLaneActionTypes.Update_Success;

  constructor(public payload: UserLane) {}
}

export class UserLaneUpdateFailureAction implements Action {
  readonly type = UserLaneActionTypes.Update_Failure;

  constructor(public payload: Error) {}
}

export class UserLaneDeleteAction implements Action {
  readonly type = UserLaneActionTypes.Delete;

  constructor(public payload: UserLane) {}
}

export class UserLaneDeleteSuccessAction implements Action {
  readonly type = UserLaneActionTypes.Delete_Success;

  constructor(public payload: UserLane) {}
}

export class UserLaneDeleteFailureAction implements Action {
  readonly type = UserLaneActionTypes.Delete_Failure;

  constructor(public payload: Error) {}
}

export class UserLaneToggleDisplayAction implements Action {
  readonly type = UserLaneActionTypes.Toggle_Display;

  constructor(public payload: UserLane) {}
}

export class UserLaneToggleAllDisplayAction implements Action {
  readonly type = UserLaneActionTypes.ToggleAll_Display;

  constructor(public payload: UserLane[]) {}
}

export class UserLaneSelectedDetailAction implements Action {
  readonly type = UserLaneActionTypes.Selected;

  constructor(public payload: UserLane) {}
}

export type UserLaneActions =
  | UserLaneLoadAction
  | UserLaneLoadSuccessAction
  | UserLaneLoadFailureAction
  | UserLaneAddAction
  | UserLaneAddSuccessAction
  | UserLaneAddFailureAction
  | UserLaneUpdateAction
  | UserLaneUpdateSuccessAction
  | UserLaneUpdateFailureAction
  | UserLaneDeleteAction
  | UserLaneDeleteSuccessAction
  | UserLaneDeleteFailureAction
  | UserLaneToggleDisplayAction
  | UserLaneToggleAllDisplayAction
  | UserLaneSelectedDetailAction;
