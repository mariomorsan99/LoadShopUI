import { Action } from '@ngrx/store';
import { TransportationMode } from '../../../shared/models';

export enum TransportationModeActionTypes {
    Load = '[TransportationMode] LOAD',
    Load_Success = '[TransportationMode] LOAD_SUCCESS',
    Load_Failure = '[TransportationMode] LOAD_FAILURE'
}

export class TransportationModeLoadAction implements Action {
    readonly type = TransportationModeActionTypes.Load;

    constructor() { }
}

export class TransportationModeLoadSuccessAction implements Action {
    readonly type = TransportationModeActionTypes.Load_Success;

    constructor(public payload: TransportationMode[]) { }
}

export class TransportationModeLoadFailureAction implements Action {
    readonly type = TransportationModeActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type TransportationModeActions =
    TransportationModeLoadAction |
    TransportationModeLoadSuccessAction |
    TransportationModeLoadFailureAction;
