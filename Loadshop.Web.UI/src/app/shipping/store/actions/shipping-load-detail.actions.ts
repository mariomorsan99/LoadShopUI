/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';
import { PostLoadsClientResponse } from 'src/app/shared/models/post-loads-client-response';
import { IShippingLoadDetail } from '../../../shared/models';

export enum ShippingLoadDetailActionTypes {
  Load_All = '[ShippingLoadDetail] LOAD_ALL',
  Load_All_Success = '[ShippingLoadDetail] LOAD_ALL_SUCCESS',
  Load_All_Failure = '[ShippingLoadDetail] LOAD_ALL_FAILURE',
  Post_Loads = '[ShippingLoadDetail] POST_LOADS',
  Post_Loads_Success = '[ShippingLoadDetail] POST_LOADS_SUCCESS',
  Post_Loads_Failure = '[ShippingLoadDetail] POST_LOADS_FAILURE',
  Remove_Load = '[ShippingLoadDetail] REMOVE_LOAD',
  Remove_Load_Success = '[ShippingLoadDetail] REMOVE_LOAD_SUCCESS',
  Remove_Load_Failure = '[ShippingLoadDetail] REMOVE_LOAD_FAILURE',
  Delete_Load = '[ShippingLoadDetail] DELETE_LOAD',
  Delete_Load_Success = '[ShippingLoadDetail] DELETE_LOAD_SUCCESS',
  Delete_Load_Failure = '[ShippingLoadDetail] DELETE_LOAD_FAILURE',
  Discard_Changes = '[ShippingLoadDetail] DISCARD_CHANGES',
  Update_Load = '[ShippingLoadDetail] UPDATE LOAD'
}

export class ShippingLoadDetailLoadAllAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Load_All;
}

export class ShippingLoadDetailLoadAllSuccessAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Load_All_Success;

  constructor(public payload: IShippingLoadDetail[]) {}
}

export class ShippingLoadDetailLoadAllFailureAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Load_All_Failure;

  constructor(public payload: Error) {}
}

export class ShippingLoadDetailPostLoadsAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Post_Loads;

  constructor(public payload: IShippingLoadDetail[]) {}
}

export class ShippingLoadDetailPostLoadsSuccessAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Post_Loads_Success;

  constructor(public payload: PostLoadsClientResponse) {}
}

export class ShippingLoadDetailPostLoadsFailureAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Post_Loads_Failure;

  constructor(public payload: Error) {}
}

export class ShippingLoadDetailRemoveLoadAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Remove_Load;

  constructor(public payload: string) {}
}

export class ShippingLoadDetailRemoveLoadSuccessAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Remove_Load_Success;

  constructor(public payload: IShippingLoadDetail) {}
}

export class ShippingLoadDetailRemoveLoadFailureAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Remove_Load_Failure;

  constructor(public payload: Error) {}
}

export class ShippingLoadDetailDeleteLoadAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Delete_Load;

  constructor(public payload: string) {}
}

export class ShippingLoadDetailDeleteLoadSuccessAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Delete_Load_Success;

  constructor(public payload: IShippingLoadDetail) {}
}

export class ShippingLoadDetailDeleteLoadFailureAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Delete_Load_Failure;

  constructor(public payload: Error) {}
}

export class ShippingLoadDetailDiscardChanges implements Action {
  readonly type = ShippingLoadDetailActionTypes.Discard_Changes;

  constructor(public payload: IShippingLoadDetail) {}
}

export class ShippingLoadDetailUpdateLoadAction implements Action {
  readonly type = ShippingLoadDetailActionTypes.Update_Load;

  constructor(public payload: IShippingLoadDetail) {}
}

export type ShippingLoadDetailActions =
  | ShippingLoadDetailLoadAllAction
  | ShippingLoadDetailLoadAllSuccessAction
  | ShippingLoadDetailLoadAllFailureAction
  | ShippingLoadDetailPostLoadsAction
  | ShippingLoadDetailPostLoadsSuccessAction
  | ShippingLoadDetailPostLoadsFailureAction
  | ShippingLoadDetailRemoveLoadAction
  | ShippingLoadDetailRemoveLoadSuccessAction
  | ShippingLoadDetailRemoveLoadFailureAction
  | ShippingLoadDetailDeleteLoadAction
  | ShippingLoadDetailDeleteLoadSuccessAction
  | ShippingLoadDetailDeleteLoadFailureAction
  | ShippingLoadDetailDiscardChanges
  | ShippingLoadDetailUpdateLoadAction;
