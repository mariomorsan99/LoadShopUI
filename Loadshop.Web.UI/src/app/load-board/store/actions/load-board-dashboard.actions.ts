import { Action } from '@ngrx/store';
import { defaultSearch, LoadView, Search } from '../../../shared/models';

export enum LoadBoardDashboardActionTypes {
  Load = '[LoadBoardDashboard] LOAD',
  Start_Polling = '[LoadBoardDashboard] START_POLLING',
  Cancel_Polling = '[LoadBoardDashboard] END_POLLING',
  Load_Success = '[LoadBoardDashboard] LOAD_SUCCESS',
  Load_Failure = '[LoadBoardDashboard] LOAD_FAILURE',
  Selected = '[LoadBoardDashboard] SELECTED',
  Search_Add = '[LoadBoardDashboard] SEARCH_ADD',
  Search_Clear = '[LoadBoardDashboard] SEARCH_CLEAR',
}

export class LoadBoardDashboardLoadAction implements Action {
  readonly type = LoadBoardDashboardActionTypes.Load;
  constructor() {}
}

export class LoadBoardDashboardStartLoadPollingAction implements Action {
  readonly type = LoadBoardDashboardActionTypes.Start_Polling;
  constructor() {}
}

export class LoadBoardDashboardCancelLoadPollingAction implements Action {
  readonly type = LoadBoardDashboardActionTypes.Cancel_Polling;
  constructor() {}
}

export class LoadBoardDashboardLoadSuccessAction implements Action {
  readonly type = LoadBoardDashboardActionTypes.Load_Success;

  constructor(public payload: LoadView[]) {}
}

export class LoadBoardDashboardLoadFailureAction implements Action {
  readonly type = LoadBoardDashboardActionTypes.Load_Failure;

  constructor(public payload: Error) {}
}

export class LoadBoardDashboardSearchAddAction implements Action {
  readonly type = LoadBoardDashboardActionTypes.Search_Add;
  constructor(public payload: Search) {}
}
export class LoadBoardDashboardSearchClearAction implements Action {
  readonly type = LoadBoardDashboardActionTypes.Search_Clear;
  constructor(public payload: Search = { ...defaultSearch }) {}
}

export type LoadBoardDashboardActions =
  | LoadBoardDashboardLoadAction
  | LoadBoardDashboardStartLoadPollingAction
  | LoadBoardDashboardCancelLoadPollingAction
  | LoadBoardDashboardLoadSuccessAction
  | LoadBoardDashboardLoadFailureAction
  | LoadBoardDashboardSearchAddAction
  | LoadBoardDashboardSearchClearAction;
