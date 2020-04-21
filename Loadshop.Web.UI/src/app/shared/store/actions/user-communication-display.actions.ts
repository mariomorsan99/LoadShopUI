import { Action } from '@ngrx/store';
import { UserCommunication } from '../../models';

export enum UserCommunicationDisplayActionTypes {
    Load = '[UserCommunicationDisplay] USER_COMMUNICATIOND_DISPLAY_LOAD',
    Load_Success = '[UserCommunicationDisplay] USER_COMMUNICATIOND_DISPLAY_LOAD_SUCESS',
    Load_Failure = '[UserCommunicationDisplay] USER_COMMUNICATIOND_DISPLAY_LOAD_Failure',
    Acknowledge = '[UserCommunicationDisplay] USER_COMMUNICATIOND_DISPLAY_ACKNOWLEDGE',
    Acknowledge_Success = '[UserCommunicationDisplay] USER_COMMUNICATIOND_DISPLAY_ACKNOWLEDGE_SUCCESS',
    Acknowledge_Failure = '[UserCommunicationDisplay] USER_COMMUNICATIOND_DISPLAY_ACKNOWLEDGE_FAILURE',
}

export class UserCommunicationDisplayLoadAction implements Action {
    readonly type = UserCommunicationDisplayActionTypes.Load;

    constructor() { }
}

export class UserCommunicationDisplayLoadSuccessAction implements Action {
    readonly type = UserCommunicationDisplayActionTypes.Load_Success;

    constructor(public payload: UserCommunication[]) { }
}


export class UserCommunicationDisplayLoadFailureAction implements Action {
    readonly type = UserCommunicationDisplayActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export class UserCommunicationDisplayAcknowledgeAction implements Action {
    readonly type = UserCommunicationDisplayActionTypes.Acknowledge;

    constructor(public payload: UserCommunication) { }
}

export class UserCommunicationDisplayAcknowledgeSuccessAction implements Action {
    readonly type = UserCommunicationDisplayActionTypes.Acknowledge_Success;

    constructor(public payload: UserCommunication[]) { }
}


export class UserCommunicationDisplayAcknowledgeFailureAction implements Action {
    readonly type = UserCommunicationDisplayActionTypes.Acknowledge_Failure;

    constructor(public payload: Error) { }
}

export type UserCommunicationDisplayActions =
    | UserCommunicationDisplayLoadAction
    | UserCommunicationDisplayLoadSuccessAction
    | UserCommunicationDisplayLoadFailureAction
    | UserCommunicationDisplayAcknowledgeAction
    | UserCommunicationDisplayAcknowledgeSuccessAction
    | UserCommunicationDisplayAcknowledgeFailureAction;
