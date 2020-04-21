import { Action } from '@ngrx/store';
import { ShippingLoadCarrierGroupData } from '../../../shared/models';

export enum ShippingLoadCarrierGroupActionTypes {
    Load_Carrier_Groups_Load = '[ShippingLoadDetail] LOAD_CARRIER_GROUPS_LOAD',
    Load_Carrier_Groups_Load_Success = '[ShippingLoadDetail] LOAD_CARRIER_GROUPS_LOAD_SUCCESS',
    Load_Carrier_Groups_Load_Failure = '[ShippingLoadDetail] LOAD_CARRIER_GROUPS_LOAD_FAILURE',
}

export class ShippingLoadCarrierGroupsLoadAction implements Action {
    readonly type = ShippingLoadCarrierGroupActionTypes.Load_Carrier_Groups_Load;

    constructor(public payload: { loadId: string }) { }
}

export class ShippingLoadCarrierGroupsLoadSuccessAction implements Action {
    readonly type = ShippingLoadCarrierGroupActionTypes.Load_Carrier_Groups_Load_Success;

    constructor(public payload: { loadId: string, logs: ShippingLoadCarrierGroupData[] }) { }
}

export class ShippingLoadCarrierGroupsLoadFailureAction implements Action {
    readonly type = ShippingLoadCarrierGroupActionTypes.Load_Carrier_Groups_Load_Failure;

    constructor(public payload: { loadId: string, error: Error }) { }
}

export type ShippingLoadCarrierGroupActions =
    ShippingLoadCarrierGroupsLoadAction |
    ShippingLoadCarrierGroupsLoadSuccessAction |
    ShippingLoadCarrierGroupsLoadFailureAction;
