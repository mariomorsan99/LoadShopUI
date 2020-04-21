import { Action } from '@ngrx/store';
import { MenuItem } from 'primeng/api';

export enum AdminMenuActionTypes {
    Load = '[AdminMenu] LOAD',
    Load_Success = '[AdminMenu] LOAD_SUCCESS',
    Load_Failure = '[AdminMenu] LOAD_FAILURE',
}

export class AdminMenuLoadAction implements Action {
    readonly type = AdminMenuActionTypes.Load;

    constructor() { }
}

export class AdminMenuLoadSuccessAction implements Action {
    readonly type = AdminMenuActionTypes.Load_Success;

    constructor(public payload: MenuItem[]) { }
}

export class AdminMenuLoadFailureAction implements Action {
    readonly type = AdminMenuActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type AdminMenuActions =
    AdminMenuLoadAction |
    AdminMenuLoadSuccessAction |
    AdminMenuLoadFailureAction;
