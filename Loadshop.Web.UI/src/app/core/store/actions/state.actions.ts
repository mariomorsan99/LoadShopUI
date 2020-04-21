import { Action } from '@ngrx/store';
import { State } from '../../../shared/models';

export enum StateActionTypes {
    Load = '[State] LOAD',
    Load_Success = '[State] LOAD_SUCCESS',
    Load_Failure = '[State] LOAD_FAILURE'
}

export class StateLoadAction implements Action {
    readonly type = StateActionTypes.Load;

    constructor() { }
}

export class StateLoadSuccessAction implements Action {
    readonly type = StateActionTypes.Load_Success;

    constructor(public payload: State[]) { }
}

export class StateLoadFailureAction implements Action {
    readonly type = StateActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type StateActions =
    StateLoadAction |
    StateLoadSuccessAction |
    StateLoadFailureAction;
