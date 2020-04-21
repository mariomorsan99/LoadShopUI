import { Action } from '@ngrx/store';
import { CarrierProfile, Carrier } from '../../../shared/models';

export enum CarrierProfileActionTypes {
    Load = '[CarrierProfile] LOAD',
    Load_Success = '[CarrierProfile] LOAD_SUCCESS',
    Load_Failure = '[CarrierProfile] LOAD_FAILURE',
    Load_All = '[CarrierProfile] LOAD_ALL',
    Load_All_Success = '[CarrierProfile] LOAD_ALL_SUCCESS',
    Load_All_Failure = '[CarrierProfile] LOAD_ALL_FAILURE',
    // Load_New = '[CarrierProfile] LOAD_NEW',
    // Add = '[CarrierProfile] ADD',
    // Add_Success = '[CarrierProfile] ADD_SUCCESS',
    // Add_Failure = '[CarrierProfile] ADD_FAILURE',
    Update = '[CarrierProfile] UPDATE',
    Update_Success = '[CarrierProfile] UPDATE_SUCCESS',
    Update_Failure = '[CarrierProfile] UPDATE_FAILURE',
    Cancel_Update = '[CarrierProfile] CANCEL_UPDATE',
    /*
    Delete = '[CarrierProfile] DELETE',
    Delete_Success = '[CarrierProfile] DELETE_SUCCESS',
    Delete_Failure = '[CarrierProfile] DELETE_FAILURE',
    */
    Clear_Save_Succeeded = '[CarrierProfile] CLEAR_SAVE_SUCCEEDED',
}

export class CarrierProfileLoadAction implements Action {
    readonly type = CarrierProfileActionTypes.Load;
    constructor(public payload: { carrierId: string }) { }
}

export class CarrierProfileLoadSuccessAction implements Action {
    readonly type = CarrierProfileActionTypes.Load_Success;

    constructor(public payload: CarrierProfile) { }
}

export class CarrierProfileLoadFailureAction implements Action {
    readonly type = CarrierProfileActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export class CarrierProfileLoadAllAction implements Action {
    readonly type = CarrierProfileActionTypes.Load_All;
    constructor() { }
}

export class CarrierProfileLoadAllSuccessAction implements Action {
    readonly type = CarrierProfileActionTypes.Load_All_Success;

    constructor(public payload: Carrier[]) { }
}

export class CarrierProfileLoadAllFailureAction implements Action {
    readonly type = CarrierProfileActionTypes.Load_All_Failure;

    constructor(public payload: Error) { }
}

export class CarrierProfileUpdateAction implements Action {
    readonly type = CarrierProfileActionTypes.Update;
    constructor(public payload: CarrierProfile) { }
}

export class CarrierProfileUpdateSuccessAction implements Action {
    readonly type = CarrierProfileActionTypes.Update_Success;

    constructor(public payload: CarrierProfile) { }
}

export class CarrierProfileUpdateFailureAction implements Action {
    readonly type = CarrierProfileActionTypes.Update_Failure;

    constructor(public payload: Error) { }
}

export class CarrierProfileClearSuccessAction implements Action {
    readonly type = CarrierProfileActionTypes.Clear_Save_Succeeded;
    constructor(public payload: CarrierProfile) { }
}

export class CarrierProfileCancelUpdateAction implements Action {
    readonly type = CarrierProfileActionTypes.Cancel_Update;
}

export type CarrierProfileActions =
    CarrierProfileLoadAction |
    CarrierProfileLoadSuccessAction |
    CarrierProfileLoadFailureAction |
    CarrierProfileUpdateAction |
    CarrierProfileUpdateSuccessAction |
    CarrierProfileUpdateFailureAction |
    CarrierProfileClearSuccessAction |
    CarrierProfileCancelUpdateAction |
    CarrierProfileLoadAllAction |
    CarrierProfileLoadAllSuccessAction |
    CarrierProfileLoadAllFailureAction;
