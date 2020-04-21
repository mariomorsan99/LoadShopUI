import { Action } from '@ngrx/store';
import { LoadCarrierScacRestrictionData } from '../../../shared/models';

export enum ShippingLoadCarrierScacRestrictionActionTypes {
    Load_Carrier_Scac_Restrictions_Load = '[ShippingLoadDetail] LOAD_CARRIER_SCAC_RESTRICTIONS_LOAD',
    Load_Carrier_Scac_Restrictions_Load_Success = '[ShippingLoadDetail] LOAD_CARRIER_SCAC_RESTRICTIONS_LOAD_SUCCESS',
    Load_Carrier_Scac_Restrictions_Load_Failure = '[ShippingLoadDetail] LOAD_CARRIER_SCAC_RESTRICTIONS_LOAD_FAILURE',
}

export class ShippingLoadCarrierScacRestrictionsLoadAction implements Action {
    readonly type = ShippingLoadCarrierScacRestrictionActionTypes.Load_Carrier_Scac_Restrictions_Load;

    constructor(public payload: { loadId: string }) { }
}

export class ShippingLoadCarrierScacRestrictionsLoadSuccessAction implements Action {
    readonly type = ShippingLoadCarrierScacRestrictionActionTypes.Load_Carrier_Scac_Restrictions_Load_Success;

    constructor(public payload: { loadId: string, scacs: LoadCarrierScacRestrictionData[] }) { }
}

export class ShippingLoadCarrierScacRestrictionsLoadFailureAction implements Action {
    readonly type = ShippingLoadCarrierScacRestrictionActionTypes.Load_Carrier_Scac_Restrictions_Load_Failure;

    constructor(public payload: { loadId: string, error: Error }) { }
}

export type ShippingLoadCarrierScacRestrictionActions =
    ShippingLoadCarrierScacRestrictionsLoadAction |
    ShippingLoadCarrierScacRestrictionsLoadSuccessAction |
    ShippingLoadCarrierScacRestrictionsLoadFailureAction;
