import { Action } from '@ngrx/store';
import { LoadCarrierScacData } from '../../../shared/models';

export enum ShippingLoadCarrierScacActionTypes {
    Load_Carrier_Scacs_Load = '[ShippingLoadDetail] LOAD_CARRIER_SCACS_LOAD',
    Load_Carrier_Scacs_Load_Success = '[ShippingLoadDetail] LOAD_CARRIER_SCACS_LOAD_SUCCESS',
    Load_Carrier_Scacs_Load_Failure = '[ShippingLoadDetail] LOAD_CARRIER_SCACS_LOAD_FAILURE',
}

export class ShippingLoadCarrierScacsLoadAction implements Action {
    readonly type = ShippingLoadCarrierScacActionTypes.Load_Carrier_Scacs_Load;

    constructor(public payload: { loadId: string }) { }
}

export class ShippingLoadCarrierScacsLoadSuccessAction implements Action {
    readonly type = ShippingLoadCarrierScacActionTypes.Load_Carrier_Scacs_Load_Success;

    constructor(public payload: { loadId: string, scacs: LoadCarrierScacData[] }) { }
}

export class ShippingLoadCarrierScacsLoadFailureAction implements Action {
    readonly type = ShippingLoadCarrierScacActionTypes.Load_Carrier_Scacs_Load_Failure;

    constructor(public payload: { loadId: string, error: Error }) { }
}

export type ShippingLoadCarrierScacActions =
    ShippingLoadCarrierScacsLoadAction |
    ShippingLoadCarrierScacsLoadSuccessAction |
    ShippingLoadCarrierScacsLoadFailureAction;
