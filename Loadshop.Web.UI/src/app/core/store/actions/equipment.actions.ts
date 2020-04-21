import { Action } from '@ngrx/store';
import { Equipment } from '../../../shared/models';

export enum EquipmentActionTypes {
    Load = '[Equipment] LOAD',
    Load_Success = '[Equipment] LOAD_SUCCESS',
    Load_Failure = '[Equipment] LOAD_FAILURE'
}

export class EquipmentLoadAction implements Action {
    readonly type = EquipmentActionTypes.Load;

    constructor() { }
}

export class EquipmentLoadSuccessAction implements Action {
    readonly type = EquipmentActionTypes.Load_Success;

    constructor(public payload: Equipment[]) { }
}

export class EquipmentLoadFailureAction implements Action {
    readonly type = EquipmentActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type EquipmentActions =
    EquipmentLoadAction |
    EquipmentLoadSuccessAction |
    EquipmentLoadFailureAction;
