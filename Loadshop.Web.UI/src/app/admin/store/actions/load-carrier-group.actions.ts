/* eslint-disable @typescript-eslint/camelcase */
import { HttpErrorResponse } from '@angular/common/http';
import { Action } from '@ngrx/store';
import { LoadCarrierGroupCarrierData, LoadCarrierGroupDetailData, LoadCarrierGroupType } from '../../../shared/models';

export enum LoadCarrierGroupActionTypes {
  Load = '[LoadCarrierGroup] LOAD',
  Load_Success = '[LoadCarrierGroup] LOAD_SUCCESS',
  Load_Failure = '[LoadCarrierGroup] LOAD_FAILURE',
  Load_Group = '[LoadCarrierGroup] LOAD_GROUP',
  Load_Group_Success = '[LoadCarrierGroup] LOAD_GROUP_SUCCESS',
  Load_Group_Failure = '[LoadCarrierGroup] LOAD_GROUP_FAILURE',
  Add = '[LoadCarrierGroup] ADD',
  Add_Success = '[LoadCarrierGroup] ADD_SUCCESS',
  Add_Failure = '[LoadCarrierGroup] ADD_FAILURE',
  Update = '[LoadCarrierGroup] UPDATE',
  Update_Success = '[LoadCarrierGroup] UPDATE_SUCCESS',
  Update_Failure = '[LoadCarrierGroup] UPDATE_FAILURE',
  Delete = '[LoadCarrierGroup] DELETE',
  Delete_Success = '[LoadCarrierGroup] DELETE_SUCCESS',
  Delete_Failure = '[LoadCarrierGroup] DELETE_FAILURE',
  Clear_Save_Succeeded = '[LoadCarrierGroup] CLEAR_SAVE_SUCCEEDED',
  Copy_Carriers_Load = '[LoadCarrierGroup] COPY_CARRIERS_LOAD',
  Copy_Carriers_Load_Success = '[LoadCarrierGroup] COPY_CARRIERS_LOAD_SUCCESS',
  Copy_Carriers_Load_Fail = '[LoadCarrierGroup] COPY_CARRIERS_LOAD_FAILURE',
  Load_Carrier_Group_Types = '[LoadCarrierGroup] LOAD_CARRIER_GROUP_TYPES',
  Load_Carrier_Group_Types_Success = '[LoadCarrierGroup] LOAD_CARRIER_GROUP_TYPES_SUCCESS',
  Load_Carrier_Group_Types_Failure = '[LoadCarrierGroup] LOAD_CARRIER_GROUP_TYPES_FAILURE',
}

export class LoadCarrierGroupLoadAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Load;
  constructor(public payload: { customerId: string }) {}
}

export class LoadCarrierGroupLoadSuccessAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Load_Success;

  constructor(public payload: LoadCarrierGroupDetailData[]) {}
}

export class LoadCarrierGroupLoadFailureAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Load_Failure;

  constructor(public payload: Error) {}
}

export class LoadCarrierGroupLoadGroupAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Load_Group;
  constructor(public payload: { loadCarrierGroupId: number }) {}
}

export class LoadCarrierGroupLoadGroupSuccessAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Load_Group_Success;

  constructor(public payload: LoadCarrierGroupDetailData) {}
}

export class LoadCarrierGroupLoadGroupFailureAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Load_Group_Failure;

  constructor(public payload: Error) {}
}

export class LoadCarrierGroupAddAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Add;

  constructor(public payload: LoadCarrierGroupDetailData) {}
}

export class LoadCarrierGroupAddSuccessAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Add_Success;

  constructor(public payload: LoadCarrierGroupDetailData) {}
}

export class LoadCarrierGroupAddFailureAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Add_Failure;

  constructor(public payload: HttpErrorResponse) {}
}

export class LoadCarrierGroupUpdateAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Update;

  constructor(public payload: LoadCarrierGroupDetailData) {}
}

export class LoadCarrierGroupUpdateSuccessAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Update_Success;

  constructor(public payload: LoadCarrierGroupDetailData) {}
}

export class LoadCarrierGroupUpdateFailureAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Update_Failure;

  constructor(public payload: HttpErrorResponse) {}
}

export class LoadCarrierGroupDeleteAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Delete;

  constructor(public payload: LoadCarrierGroupDetailData) {}
}

export class LoadCarrierGroupDeleteSuccessAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Delete_Success;

  constructor(public payload: LoadCarrierGroupDetailData) {}
}

export class LoadCarrierGroupDeleteFailureAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Delete_Failure;

  constructor(public payload: Error) {}
}

export class LoadCarrierGroupClearSaveSucceededAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Clear_Save_Succeeded;

}

export class LoadCarrierGroupCopyCarriersAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Copy_Carriers_Load;
  constructor(public payload: { loadCarrierGroupId: number }) {}
}
export class LoadCarrierGroupCopyCarriersSuccessAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Copy_Carriers_Load_Success;

  constructor(public payload: LoadCarrierGroupCarrierData[]) {}
}

export class LoadCarrierGroupCopyCarriersFailureAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Copy_Carriers_Load_Fail;

  constructor(public payload: Error) {}
}

export class LoadCarrierGroupLoadCarrierGroupTypesAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Load_Carrier_Group_Types;

}

export class LoadCarrierGroupLoadCarrierGroupTypesSuccessAction implements Action {
  readonly type = LoadCarrierGroupActionTypes.Load_Carrier_Group_Types_Success;

  constructor(public payload: LoadCarrierGroupType[]) {}
}

export class LoadCarrierGroupLoadCarrierGroupTypesFailure implements Action {
  readonly type = LoadCarrierGroupActionTypes.Load_Carrier_Group_Types_Failure;

  constructor(public payload: Error) {}
}

export type LoadCarrierGroupActions =
  | LoadCarrierGroupLoadAction
  | LoadCarrierGroupLoadSuccessAction
  | LoadCarrierGroupLoadFailureAction
  | LoadCarrierGroupLoadGroupAction
  | LoadCarrierGroupLoadGroupSuccessAction
  | LoadCarrierGroupLoadGroupFailureAction
  | LoadCarrierGroupAddAction
  | LoadCarrierGroupAddSuccessAction
  | LoadCarrierGroupAddFailureAction
  | LoadCarrierGroupUpdateAction
  | LoadCarrierGroupUpdateSuccessAction
  | LoadCarrierGroupUpdateFailureAction
  | LoadCarrierGroupDeleteAction
  | LoadCarrierGroupDeleteSuccessAction
  | LoadCarrierGroupDeleteFailureAction
  | LoadCarrierGroupClearSaveSucceededAction
  | LoadCarrierGroupCopyCarriersAction
  | LoadCarrierGroupCopyCarriersSuccessAction
  | LoadCarrierGroupCopyCarriersFailureAction
  | LoadCarrierGroupLoadCarrierGroupTypesAction
  | LoadCarrierGroupLoadCarrierGroupTypesSuccessAction
  | LoadCarrierGroupLoadCarrierGroupTypesFailure;
