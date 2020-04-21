import { Action } from '@ngrx/store';
import { LoadView, LoadClaim, PageableResult } from '../../../shared/models';
import { PageableQueryHelper } from 'src/app/shared/utilities';

export enum LoadBoardDeliveredActionTypes {
  Load = '[LoadBoardDelivered] LOAD',
  Load_Success = '[LoadBoardDelivered] LOAD_SUCCESS',
  Load_Failure = '[LoadBoardDelivered] LOAD_FAILURE',
  Selected = '[LoadBoardDelivered] SELECTED',
  Save_Load = '[LoadBoardDelivered] SAVE_LOAD',
  Save_Load_Success = '[LoadBoardDelivered] SAVE_LOAD_SUCCESS',
  Save_Load_Failure = '[LoadBoardDelivered] SAVE_LOAD_FAILURE',
  Update_Query = '[LoadBoardDelivered] QUERY_UDPATE',
}

export class LoadBoardDeliveredLoadAction implements Action {
  readonly type = LoadBoardDeliveredActionTypes.Load;
  constructor(public payload: { queryHelper: PageableQueryHelper }) {}
}

export class LoadBoardDeliveredLoadSuccessAction implements Action {
  readonly type = LoadBoardDeliveredActionTypes.Load_Success;

  constructor(public payload: PageableResult<LoadView>) {}
}

export class LoadBoardDeliveredLoadFailureAction implements Action {
  readonly type = LoadBoardDeliveredActionTypes.Load_Failure;

  constructor(public payload: Error) {}
}

export class LoadBoardDeliveredSaveLoadAction implements Action {
  readonly type = LoadBoardDeliveredActionTypes.Save_Load;

  constructor(public payload: LoadView) {}
}

export class LoadBoardDeliveredSaveLoadSuccessAction implements Action {
  readonly type = LoadBoardDeliveredActionTypes.Save_Load_Success;

  constructor(public payload: LoadClaim) {}
}

export class LoadBoardDeliveredSaveLoadFailureAction implements Action {
  readonly type = LoadBoardDeliveredActionTypes.Save_Load_Failure;

  constructor(public payload: Error) {}
}

export class LoadBoardDeliveredUpdateQueryAction implements Action {
  readonly type = LoadBoardDeliveredActionTypes.Update_Query;

  constructor(public payload: PageableQueryHelper) {}
}

export type LoadBoardDeliveredActions =
  | LoadBoardDeliveredLoadAction
  | LoadBoardDeliveredLoadSuccessAction
  | LoadBoardDeliveredLoadFailureAction
  | LoadBoardDeliveredSaveLoadAction
  | LoadBoardDeliveredSaveLoadSuccessAction
  | LoadBoardDeliveredSaveLoadFailureAction
  | LoadBoardDeliveredUpdateQueryAction;
