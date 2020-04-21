import { Action } from '@ngrx/store';
import { Commodity } from '../../../shared/models';

export enum CommodityActionTypes {
    Load = '[Commodity] LOAD',
    Load_Success = '[Commodity] LOAD_SUCCESS',
    Load_Failure = '[Commodity] LOAD_FAILURE'
}

export class CommodityLoadAction implements Action {
    readonly type = CommodityActionTypes.Load;

    constructor() { }
}

export class CommodityLoadSuccessAction implements Action {
    readonly type = CommodityActionTypes.Load_Success;

    constructor(public payload: Commodity[]) { }
}

export class CommodityLoadFailureAction implements Action {
    readonly type = CommodityActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type CommodityActions =
    CommodityLoadAction |
    CommodityLoadSuccessAction |
    CommodityLoadFailureAction;
