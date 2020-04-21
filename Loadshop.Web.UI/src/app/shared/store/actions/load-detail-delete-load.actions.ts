/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';
import { IShippingLoadDetail, RemoveLoadData } from '../../models';

export enum LoadDetailDeleteLoadActionTypes {
  DeleteLoad = '[LoadBoardLoadDetail] DELETE_LOAD',
  DeleteLoad_Success = '[LoadBoardLoadDetail] DELETE_LOAD_SUCCESS',
  DeleteLoad_Failure = '[LoadBoardLoadDetail] DELETE_LOAD_FAILURE',
}

export class LoadDetailDeleteLoadAction implements Action {
  readonly type = LoadDetailDeleteLoadActionTypes.DeleteLoad;

  constructor(public payload: RemoveLoadData) {}
}

export class LoadDetailDeleteLoadSuccessAction implements Action {
  readonly type = LoadDetailDeleteLoadActionTypes.DeleteLoad_Success;

  constructor(public payload: IShippingLoadDetail) {}
}

export class LoadDetailDeleteLoadFailureAction implements Action {
  readonly type = LoadDetailDeleteLoadActionTypes.DeleteLoad_Failure;

  constructor(public payload: Error) {}
}

export type LoadDetailDeleteLoadActions =
  | LoadDetailDeleteLoadAction
  | LoadDetailDeleteLoadSuccessAction
  | LoadDetailDeleteLoadFailureAction;
