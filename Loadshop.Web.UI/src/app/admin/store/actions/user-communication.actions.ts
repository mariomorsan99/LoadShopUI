import { Action } from '@ngrx/store';
import { UserCommunication, UserCommunicationDetail } from '../../../shared/models';

export enum UserCommunicationActionTypes {
    Load = '[UserCommunication] LOAD',
    Load_Success = '[UserCommunication] LOAD_SUCCESS',
    Load_Failure = '[UserCommunication] LOAD_FAILURE',
    Load_All = '[UserCommunication] LOAD_ALL',
    Load_All_Success = '[UserCommunication] LOAD_ALL_SUCCESS',
    Load_All_Failure = '[UserCommunication] LOAD_ALL_FAILURE',
    // Load_New = '[UserCommunication] LOAD_NEW',
    // Add = '[UserCommunication] ADD',
    // Add_Success = '[UserCommunication] ADD_SUCCESS',
    // Add_Failure = '[UserCommunication] ADD_FAILURE',
    Update = '[UserCommunication] UPDATE',
    Update_Success = '[UserCommunication] UPDATE_SUCCESS',
    Update_Failure = '[UserCommunication] UPDATE_FAILURE',
    Create = '[UserCommunication] CREATE',
    Create_Success = '[UserCommunication] Create_SUCCESS',
    Create_Failure = '[UserCommunication] Create_FAILURE',
    Create_Default = '[UserCommunication] CREATE_DEFAULT',
    Cancel_Update = '[UserCommunication] CANCEL_UPDATE',
    Delete = '[UserCommunication] DELETE',
    Delete_Success = '[UserCommunication] DELETE_SUCCESS',
    Delete_Failure = '[UserCommunication] DELETE_FAILURE',
}

export class UserCommunicationLoadAction implements Action {
    readonly type = UserCommunicationActionTypes.Load;
    constructor(public payload: { userCommunicationId: string }) { }
}

export class UserCommunicationLoadSuccessAction implements Action {
    readonly type = UserCommunicationActionTypes.Load_Success;

    constructor(public payload: UserCommunicationDetail) { }
}

export class UserCommunicationLoadFailureAction implements Action {
    readonly type = UserCommunicationActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export class UserCommunicationLoadAllAction implements Action {
    readonly type = UserCommunicationActionTypes.Load_All;
    constructor() { }
}

export class UserCommunicationLoadAllSuccessAction implements Action {
    readonly type = UserCommunicationActionTypes.Load_All_Success;

    constructor(public payload: UserCommunication[]) { }
}

export class UserCommunicationLoadAllFailureAction implements Action {
    readonly type = UserCommunicationActionTypes.Load_All_Failure;

    constructor(public payload: Error) { }
}

export class UserCommunicationUpdateAction implements Action {
    readonly type = UserCommunicationActionTypes.Update;
    constructor(public payload: UserCommunicationDetail) { }
}

export class UserCommunicationUpdateSuccessAction implements Action {
    readonly type = UserCommunicationActionTypes.Update_Success;

    constructor(public payload: UserCommunicationDetail) { }
}

export class UserCommunicationUpdateFailureAction implements Action {
    readonly type = UserCommunicationActionTypes.Update_Failure;

    constructor(public payload: Error) { }
}

export class UserCommunicationCreateAction implements Action {
    readonly type = UserCommunicationActionTypes.Create;
    constructor(public payload: UserCommunicationDetail) { }
}

export class UserCommunicationCreateSuccessAction implements Action {
    readonly type = UserCommunicationActionTypes.Create_Success;

    constructor(public payload: UserCommunicationDetail) { }
}

export class UserCommunicationCreateFailureAction implements Action {
    readonly type = UserCommunicationActionTypes.Create_Failure;

    constructor(public payload: Error) { }
}

export class UserCommunicationDeleteAction implements Action {
    readonly type = UserCommunicationActionTypes.Delete;
    constructor(public payload: UserCommunication) { }
}

export class UserCommunicationDeleteSuccessAction implements Action {
    readonly type = UserCommunicationActionTypes.Delete_Success;

    constructor(public payload: UserCommunication) { }
}

export class UserCommunicationDeleteFailureAction implements Action {
    readonly type = UserCommunicationActionTypes.Delete_Failure;

    constructor(public payload: Error) { }
}

export class UserCommunicationCreateDefaultAction implements Action {
    readonly type = UserCommunicationActionTypes.Create_Default;
}

export class UserCommunicationCancelUpdateAction implements Action {
    readonly type = UserCommunicationActionTypes.Cancel_Update;
}

export type UserCommunicationActions =
    UserCommunicationLoadAction |
    UserCommunicationLoadSuccessAction |
    UserCommunicationLoadFailureAction |
    UserCommunicationUpdateAction |
    UserCommunicationUpdateSuccessAction |
    UserCommunicationUpdateFailureAction |
    UserCommunicationCreateAction |
    UserCommunicationCreateSuccessAction |
    UserCommunicationCreateFailureAction |
    UserCommunicationCreateDefaultAction |
    UserCommunicationDeleteAction |
    UserCommunicationDeleteSuccessAction |
    UserCommunicationDeleteFailureAction |
    UserCommunicationCancelUpdateAction |
    UserCommunicationLoadAllAction |
    UserCommunicationLoadAllSuccessAction |
    UserCommunicationLoadAllFailureAction;
