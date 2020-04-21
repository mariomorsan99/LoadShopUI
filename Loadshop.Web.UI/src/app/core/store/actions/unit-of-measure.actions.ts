import { Action } from '@ngrx/store';
import { UnitOfMeasure } from '../../../shared/models';

export enum UnitOfMeasureActionTypes {
    Load = '[UnitOfMeasure] LOAD',
    Load_Success = '[UnitOfMeasure] LOAD_SUCCESS',
    Load_Failure = '[UnitOfMeasure] LOAD_FAILURE'
}

export class UnitOfMeasureLoadAction implements Action {
    readonly type = UnitOfMeasureActionTypes.Load;

    constructor() { }
}

export class UnitOfMeasureLoadSuccessAction implements Action {
    readonly type = UnitOfMeasureActionTypes.Load_Success;

    constructor(public payload: UnitOfMeasure[]) { }
}

export class UnitOfMeasureLoadFailureAction implements Action {
    readonly type = UnitOfMeasureActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type UnitOfMeasureActions =
    UnitOfMeasureLoadAction |
    UnitOfMeasureLoadSuccessAction |
    UnitOfMeasureLoadFailureAction;
