import { Action } from '@ngrx/store';
import { Carrier, CarrierCarrierScacGroup } from '../../../shared/models';

export enum CarrierActionTypes {
    Load = '[Carrier] LOAD',
    Load_Success = '[Carrier] LOAD_SUCCESS',
    Load_Failure = '[Carrier] LOAD_FAILURE',
    CarrierCarrierScacLoad = '[CarrierCarrierScac] LOAD',
    CarrierCarrierScacLoad_Success = '[CarrierCarrierScac] LOAD_SUCCESS',
    CarrierCarrierScacLoad_Failure = '[CarrierCarrierScac] LOAD_FAILURE'
}

export class CarrierLoadAction implements Action {
    readonly type = CarrierActionTypes.Load;

    constructor() { }
}

export class CarrierLoadSuccessAction implements Action {
    readonly type = CarrierActionTypes.Load_Success;

    constructor(public payload: Carrier[]) { }
}

export class CarrierLoadFailureAction implements Action {
    readonly type = CarrierActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export class CarrierCarrierScacLoadAction implements Action {
    readonly type = CarrierActionTypes.CarrierCarrierScacLoad;

    constructor() { }
}

export class CarrierCarrierScacLoadSuccessAction implements Action {
    readonly type = CarrierActionTypes.CarrierCarrierScacLoad_Success;

    constructor(public payload: CarrierCarrierScacGroup[]) { }
}

export class CarrierCarrierScacLoadFailureAction implements Action {
    readonly type = CarrierActionTypes.CarrierCarrierScacLoad_Failure;

    constructor(public payload: Error) { }
}

export type CarrierActions =
    CarrierLoadAction |
    CarrierLoadSuccessAction |
    CarrierLoadFailureAction |
    CarrierCarrierScacLoadAction |
    CarrierCarrierScacLoadSuccessAction |
    CarrierCarrierScacLoadFailureAction;
