import { Action } from '@ngrx/store';
import { ServiceType } from '../../../shared/models';

export enum ServiceTypeActionTypes {
    Load = '[ServiceType] LOAD',
    Load_Success = '[ServiceType] LOAD_SUCCESS',
    Load_Failure = '[ServiceType] LOAD_FAILURE'
}

export class ServiceTypeLoadAction implements Action {
    readonly type = ServiceTypeActionTypes.Load;

    constructor() { }
}

export class ServiceTypeLoadSuccessAction implements Action {
    readonly type = ServiceTypeActionTypes.Load_Success;

    constructor(public payload: ServiceType[]) { }
}

export class ServiceTypeLoadFailureAction implements Action {
    readonly type = ServiceTypeActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type ServiceTypeActions =
    ServiceTypeLoadAction |
    ServiceTypeLoadSuccessAction |
    ServiceTypeLoadFailureAction;
