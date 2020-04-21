import { Action } from '@ngrx/store';
import { Customer, CustomerProfile } from '../../../shared/models';
import { LoadshopShipperMapping } from 'src/app/shared/models/loadshop-shipper-mapping';

export enum ShipperProfileActionTypes {
    Load = '[ShipperProfile] LOAD',
    Load_Success = '[ShipperProfile] LOAD_SUCCESS',
    Load_Failure = '[ShipperProfile] LOAD_FAILURE',
    Load_New = '[ShipperProfile] LOAD_NEW',
    Load_Shippers = '[ShipperProfile] LOAD_SHIPPERS',
    Load_Shippers_Success = '[ShipperProfile] LOAD_SHIPPERS_SUCCESS',
    Load_Shippers_Failure = '[ShipperProfile] LOAD_SHIPPERS_FAILURE',
    Add = '[ShipperProfile] ADD',
    Add_Success = '[ShipperProfile] ADD_SUCCESS',
    Add_Failure = '[ShipperProfile] ADD_FAILURE',
    Update = '[ShipperProfile] UPDATE',
    Update_Success = '[ShipperProfile] UPDATE_SUCCESS',
    Update_Failure = '[ShipperProfile] UPDATE_FAILURE',
    EnableShipperApi = '[ShipperProfile] ENABLES_SHIPPER_API',
    EnableShipperApi_Success = '[ShipperProfile] ENABLES_SHIPPER_API_SUCCESS',
    EnableShipperApi_Failure = '[ShipperProfile] ENABLES_SHIPPER_API_FAILURE',
    /*
    Delete = '[ShipperProfile] DELETE',
    Delete_Success = '[ShipperProfile] DELETE_SUCCESS',
    Delete_Failure = '[ShipperProfile] DELETE_FAILURE',
    */
    Clear_Save_Succeeded = '[ShipperProfile] CLEAR_SAVE_SUCCEEDED',
    Load_Shipper_Mappings = '[ShipperProfile] LOAD_SHIPPER_MAPPINGS',
    Load_Shipper_Mappings_Success = '[ShipperProfile] LOAD_SHIPPERS_MAPPING_SUCCESS',
    Load_Shipper_Mappings_Failure = '[ShipperProfile] LOAD_SHIPPERS_MAPPING_FAILURE',
    Create_Shipper_Mapping = '[ShipperProfile] CREATE_SHIPPER_MAPPING',
    Create_Shipper_Mapping_Success = '[ShipperProfile] CREATE_SHIPPER_MAPPING_SUCCESS',
    Create_Shipper_Mapping_Failure = '[ShipperProfile] CREATE_SHIPPER_MAPPING_FAILURE',
    Update_Shipper_Mapping = '[ShipperProfile] UPDATE_SHIPPER_MAPPING',
    Update_Shipper_Mapping_Success = '[ShipperProfile] UPDATE_SHIPPER_MAPPING_SUCCESS',
    Update_Shipper_Mapping_Failure = '[ShipperProfile] UPDATE_SHIPPER_MAPPING_FAILURE',
    Load_SourceSystem_Owners = '[ShipperProfile] LOAD_SOURCESYSTEM_OWNERS',
    Load_SourceSystem_OwnersSuccess = '[ShipperProfile] LOAD_SOURCESYSTEM_OWNERS_SUCCESS',
    Load_SourceSystem_OwnersFailure = '[ShipperProfile] LOAD_SOURCESYSTEM_OWNERS_FAILURE',
}

export class ShipperProfileLoadAction implements Action {
    readonly type = ShipperProfileActionTypes.Load;
    constructor(public payload: { customerId: string }) { }
}

export class ShipperProfileLoadSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_Success;

    constructor(public payload: CustomerProfile) { }
}

export class ShipperProfileLoadFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export class ShipperProfileLoadNewAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_New;

    constructor(public payload: CustomerProfile) { }
}

export class ShipperProfileLoadShippersAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_Shippers;
    constructor() { }
}

export class ShipperProfileLoadShippersSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_Shippers_Success;

    constructor(public payload: Customer[]) { }
}

export class ShipperProfileLoadShippersFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_Shippers_Failure;

    constructor(public payload: Error) { }
}

export class ShipperProfileAddAction implements Action {
    readonly type = ShipperProfileActionTypes.Add;

    constructor(public payload: CustomerProfile) { }
}

export class ShipperProfileAddSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.Add_Success;

    constructor(public payload: CustomerProfile) { }
}

export class ShipperProfileAddFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.Add_Failure;

    constructor(public payload: Error) { }
}

export class ShipperProfileUpdateAction implements Action {
    readonly type = ShipperProfileActionTypes.Update;

    constructor(public payload: CustomerProfile) { }
}

export class ShipperProfileUpdateSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.Update_Success;

    constructor(public payload: CustomerProfile) { }
}

export class ShipperProfileUpdateFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.Update_Failure;

    constructor(public payload: Error) { }
}

export class ShipperProfileEnableShipperApiAction implements Action {
    readonly type = ShipperProfileActionTypes.EnableShipperApi;

    constructor(public payload: CustomerProfile) { }
}

export class ShipperProfileEnableShipperApiSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.EnableShipperApi_Success;

    constructor(public payload: CustomerProfile) { }
}

export class ShipperProfileEnableShipperApiFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.EnableShipperApi_Failure;

    constructor(public payload: Error) { }
}

export class ShipperProfileLoadShipperMappingsAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_Shipper_Mappings;
    constructor(public payload: { ownerId: string }) { }
}

export class ShipperProfileLoadShipperMappingsSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_Shipper_Mappings_Success;

    constructor(public payload: LoadshopShipperMapping[]) { }
}

export class ShipperProfileLoadShipperMappingsFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_Shipper_Mappings_Failure;

    constructor(public payload: Error) { }
}

export class ShipperProfileCreateShipperMappingAction implements Action {
    readonly type = ShipperProfileActionTypes.Create_Shipper_Mapping;
    constructor(public payload: { mapping: LoadshopShipperMapping }) { }
}

export class ShipperProfileCreateShipperMappingSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.Create_Shipper_Mapping_Success;

    constructor(public payload: LoadshopShipperMapping) { }
}

export class ShipperProfileCreateShipperMappingFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.Create_Shipper_Mapping_Failure;

    constructor(public payload: Error) { }
}

export class ShipperProfileUpdateShipperMappingAction implements Action {
    readonly type = ShipperProfileActionTypes.Update_Shipper_Mapping;
    constructor(public payload: { mapping: LoadshopShipperMapping }) { }
}

export class ShipperProfileUpdateShipperMappingSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.Update_Shipper_Mapping_Success;

    constructor(public payload: LoadshopShipperMapping) { }
}

export class ShipperProfileUpdateShipperMappingFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.Update_Shipper_Mapping_Failure;

    constructor(public payload: Error) { }
}

export class ShipperProfileLoadSourceSystemOwnerAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_SourceSystem_Owners;
    constructor(public payload: { ownerId: string }) { }
}

export class ShipperProfileLoadSourceSystemOwnerSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_SourceSystem_OwnersSuccess;

    constructor(public payload: Map<string, string[]>) { }
}

export class ShipperProfileLoadSourceSystemOwnerFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.Load_SourceSystem_OwnersFailure;

    constructor(public payload: Error) { }
}

/*
export class ShipperProfileDeleteAction implements Action {
    readonly type = ShipperProfileActionTypes.Delete;

    constructor(public payload: ShipperProfileData) { }
}

export class ShipperProfileDeleteSuccessAction implements Action {
    readonly type = ShipperProfileActionTypes.Delete_Success;

    constructor(public payload: ShipperProfileData) { }
}

export class ShipperProfileDeleteFailureAction implements Action {
    readonly type = ShipperProfileActionTypes.Delete_Failure;

    constructor(public payload: Error) { }
}
*/
export class ShipperProfileClearSaveSucceededAction implements Action {
    readonly type = ShipperProfileActionTypes.Clear_Save_Succeeded;

    constructor() { }
}

export type ShipperProfileActions =
    ShipperProfileLoadAction |
    ShipperProfileLoadSuccessAction |
    ShipperProfileLoadFailureAction |
    ShipperProfileLoadNewAction |
    ShipperProfileLoadShippersAction |
    ShipperProfileLoadShippersSuccessAction |
    ShipperProfileLoadShippersFailureAction |
    ShipperProfileAddAction |
    ShipperProfileAddSuccessAction |
    ShipperProfileAddFailureAction |
    ShipperProfileUpdateAction |
    ShipperProfileUpdateSuccessAction |
    ShipperProfileUpdateFailureAction |
    ShipperProfileEnableShipperApiAction |
    ShipperProfileEnableShipperApiSuccessAction |
    ShipperProfileEnableShipperApiFailureAction |
    /*
    ShipperProfileDeleteAction |
    ShipperProfileDeleteSuccessAction |
    ShipperProfileDeleteFailureAction |
        */
    ShipperProfileClearSaveSucceededAction |
    ShipperProfileLoadShipperMappingsAction |
    ShipperProfileLoadShipperMappingsSuccessAction |
    ShipperProfileLoadShipperMappingsFailureAction |
    ShipperProfileLoadSourceSystemOwnerAction |
    ShipperProfileLoadSourceSystemOwnerSuccessAction |
    ShipperProfileLoadSourceSystemOwnerFailureAction |
    ShipperProfileCreateShipperMappingAction |
    ShipperProfileCreateShipperMappingSuccessAction |
    ShipperProfileCreateShipperMappingFailureAction |
    ShipperProfileUpdateShipperMappingAction |
    ShipperProfileUpdateShipperMappingSuccessAction |
    ShipperProfileUpdateShipperMappingFailureAction;
