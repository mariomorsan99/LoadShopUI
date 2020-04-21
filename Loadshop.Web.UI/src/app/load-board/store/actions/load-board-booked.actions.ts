import { Action } from '@ngrx/store';
import { LoadView, LoadClaim, PageableResult } from '../../../shared/models';
import { PageableQueryHelper } from 'src/app/shared/utilities';

export enum LoadBoardBookedActionTypes {
  Load = '[LoadBoardBooked] LOAD',
  Load_Success = '[LoadBoardBooked] LOAD_SUCCESS',
  Load_Failure = '[LoadBoardBooked] LOAD_FAILURE',
  Selected = '[LoadBoardBooked] SELECTED',
  Save_Load = '[LoadBoardBooked] SAVE_LOAD',
  Save_Load_Success = '[LoadBoardBooked] SAVE_LOAD_SUCCESS',
  Save_Load_Failure = '[LoadBoardBooked] SAVE_LOAD_FAILURE',
  Update_Query = '[LoadBoardBooked] UPDATE_QUERY',
}

export class LoadBoardBookedLoadAction implements Action {
  readonly type = LoadBoardBookedActionTypes.Load;
  constructor(public payload: { queryHelper: PageableQueryHelper }) {}
}

export class LoadBoardBookedLoadSuccessAction implements Action {
  readonly type = LoadBoardBookedActionTypes.Load_Success;

  constructor(public payload: PageableResult<LoadView>) {}
}

export class LoadBoardBookedLoadFailureAction implements Action {
  readonly type = LoadBoardBookedActionTypes.Load_Failure;

  constructor(public payload: Error) {}
}

export class LoadBoardBookedSaveLoadAction implements Action {
  readonly type = LoadBoardBookedActionTypes.Save_Load;

  constructor(public payload: LoadView) {}
}

export class LoadBoardBookedSaveLoadSuccessAction implements Action {
  readonly type = LoadBoardBookedActionTypes.Save_Load_Success;

  constructor(public payload: LoadClaim) {}
}

export class LoadBoardBookedSaveLoadFailureAction implements Action {
  readonly type = LoadBoardBookedActionTypes.Save_Load_Failure;

  constructor(public payload: Error) {}
}

export class LoadBoardBookedUpdateQueryAction implements Action {
  readonly type = LoadBoardBookedActionTypes.Update_Query;

  constructor(public payload: PageableQueryHelper) {}
}

export type LoadBoardBookedActions =
  | LoadBoardBookedLoadAction
  | LoadBoardBookedLoadSuccessAction
  | LoadBoardBookedLoadFailureAction
  | LoadBoardBookedSaveLoadAction
  | LoadBoardBookedSaveLoadSuccessAction
  | LoadBoardBookedSaveLoadFailureAction
  | LoadBoardBookedUpdateQueryAction;
