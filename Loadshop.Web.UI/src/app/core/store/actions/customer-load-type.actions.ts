import { Action } from '@ngrx/store';
import { CustomerLoadType } from '../../../shared/models';

export enum CustomerLoadTypeActionTypes {
    Load = '[CustomerLoadType] LOAD',
    Load_Success = '[CustomerLoadType] LOAD_SUCCESS',
    Load_Failure = '[CustomerLoadType] LOAD_FAILURE'
}

export class CustomerLoadTypeLoadAction implements Action {
    readonly type = CustomerLoadTypeActionTypes.Load;

    constructor() { }
}

export class CustomerLoadTypeLoadSuccessAction implements Action {
    readonly type = CustomerLoadTypeActionTypes.Load_Success;

    constructor(public payload: CustomerLoadType[]) { }
}

export class CustomerLoadTypeLoadFailureAction implements Action {
    readonly type = CustomerLoadTypeActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type CustomerLoadTypeActions =
  CustomerLoadTypeLoadAction |
  CustomerLoadTypeLoadSuccessAction |
  CustomerLoadTypeLoadFailureAction;
