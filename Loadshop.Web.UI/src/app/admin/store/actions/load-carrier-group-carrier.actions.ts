import { Action } from '@ngrx/store';
import { LoadCarrierGroupCarrierData } from '../../../shared/models';

export enum LoadCarrierGroupCarrierActionTypes {
    Load = '[LoadCarrierGroupCarrier] LOAD',
    Load_Success = '[LoadCarrierGroupCarrier] LOAD_SUCCESS',
    Load_Failure = '[LoadCarrierGroupCarrier] LOAD_FAILURE',
    Add = '[LoadCarrierGroupCarrier] ADD',
    Add_Success = '[LoadCarrierGroupCarrier] ADD_SUCCESS',
    Add_Failure = '[LoadCarrierGroupCarrier] ADD_FAILURE',
    Delete = '[LoadCarrierGroupCarrier] DELETE',
    Delete_Success = '[LoadCarrierGroupCarrier] DELETE_SUCCESS',
    Delete_Failure = '[LoadCarrierGroupCarrier] DELETE_FAILURE',
    Delete_All = '[LoadCarrierGroupCarrier] DELETE_ALL',
    Delete_All_Success = '[LoadCarrierGroupCarrier] DELETE_SUCCESS_ALL',
    Delete_All_Failure = '[LoadCarrierGroupCarrier] DELETE_FAILURE_ALL',
}

export class LoadCarrierGroupCarrierLoadAction implements Action {
    readonly type = LoadCarrierGroupCarrierActionTypes.Load;
    constructor(public payload: { loadCarrierGroupId: number }) { }
}

export class LoadCarrierGroupCarrierLoadSuccessAction implements Action {
    readonly type = LoadCarrierGroupCarrierActionTypes.Load_Success;

    constructor(public payload: LoadCarrierGroupCarrierData[]) { }
}

export class LoadCarrierGroupCarrierLoadFailureAction implements Action {
    readonly type = LoadCarrierGroupCarrierActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export class LoadCarrierGroupCarrierDeleteAllAction implements Action {
    readonly type = LoadCarrierGroupCarrierActionTypes.Delete_All;

    constructor(public payload: { loadCarrierGroupId: number }) { }
}

export class LoadCarrierGroupCarrierDeleteAllSuccessAction implements Action {
    readonly type = LoadCarrierGroupCarrierActionTypes.Delete_All_Success;

    constructor(public payload: { loadCarrierGroupId: number }) { }
}

export class LoadCarrierGroupCarrierDeleteAllFailureAction implements Action {
    readonly type = LoadCarrierGroupCarrierActionTypes.Delete_All_Failure;

    constructor(public payload: Error) { }
}

export type LoadCarrierGroupCarrierActions =
    LoadCarrierGroupCarrierLoadAction |
    LoadCarrierGroupCarrierLoadSuccessAction |
    LoadCarrierGroupCarrierLoadFailureAction |
    LoadCarrierGroupCarrierDeleteAllAction |
    LoadCarrierGroupCarrierDeleteAllSuccessAction |
    LoadCarrierGroupCarrierDeleteAllFailureAction
    ;
