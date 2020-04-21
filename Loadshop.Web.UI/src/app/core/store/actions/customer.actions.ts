import { Action } from '@ngrx/store';
import { Customer } from '../../../shared/models';

export enum CustomerActionTypes {
    Load = '[Customer] LOAD',
    Load_Success = '[Customer] LOAD_SUCCESS',
    Load_Failure = '[Customer] LOAD_FAILURE'
}

export class CustomerLoadAction implements Action {
    readonly type = CustomerActionTypes.Load;

    constructor(public payload: { customerId: string }) { }
}

export class CustomerLoadSuccessAction implements Action {
    readonly type = CustomerActionTypes.Load_Success;

    constructor(public payload: Customer) { }
}

export class CustomerLoadFailureAction implements Action {
    readonly type = CustomerActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type CustomerActions =
    CustomerLoadAction |
    CustomerLoadSuccessAction |
    CustomerLoadFailureAction;
