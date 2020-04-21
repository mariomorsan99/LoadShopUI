/* eslint-disable @typescript-eslint/camelcase */
import { Action } from '@ngrx/store';
import { Load, RemoveCarrierData } from '../../models';

export enum LoadDetailCarrierRemovedActionTypes {
  CarrierRemoved = '[LoadBoardLoadDetail] CARRIER_REMOVED',
  CarrierRemoved_Success = '[LoadBoardLoadDetail] CARRIER_REMOVED_SUCCESS',
  CarrierRemoved_Failure = '[LoadBoardLoadDetail] CARRIER_REMOVED_FAILURE',
}

export class LoadDetailCarrierRemovedAction implements Action {
  readonly type = LoadDetailCarrierRemovedActionTypes.CarrierRemoved;

  constructor(public payload: RemoveCarrierData) {}
}

export class LoadDetailCarrierRemovedSuccessAction implements Action {
  readonly type = LoadDetailCarrierRemovedActionTypes.CarrierRemoved_Success;

  constructor(public payload: Load) {}
}

export class LoadDetailCarrierRemovedFailureAction implements Action {
  readonly type = LoadDetailCarrierRemovedActionTypes.CarrierRemoved_Failure;

  constructor(public payload: Error) {}
}

export type LoadDetailCarrierRemovedActions =
  | LoadDetailCarrierRemovedAction
  | LoadDetailCarrierRemovedSuccessAction
  | LoadDetailCarrierRemovedFailureAction;
