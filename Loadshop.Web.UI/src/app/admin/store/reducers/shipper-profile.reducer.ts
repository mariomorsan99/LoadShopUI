import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { Customer, CustomerProfile } from '../../../shared/models';
import { ShipperProfileActions, ShipperProfileActionTypes } from '../actions';
import { LoadshopShipperMapping } from 'src/app/shared/models/loadshop-shipper-mapping';

export interface ShipperProfileState extends EntityState<Customer> {
  loadingShipper: boolean;
  loadingShippers: boolean;
  loadingShipperMappings: boolean;
  loadingSourceSystemOwners: boolean;
  selectedShipper: CustomerProfile;
  allShippers: Customer[];
  shipperMappings: LoadshopShipperMapping[];
  sourceSystemOwners: Map<string, string[]>;
  savingCustomer: boolean;
  saveCustomerSucceeded: boolean;
  savingMapping: boolean;
}

export const adapter: EntityAdapter<Customer> = createEntityAdapter<Customer>({
  selectId: x => x.customerId,
});

const initialState: ShipperProfileState = adapter.getInitialState({
  loadingShipper: false,
  loadingShippers: false,
  loadingShipperMappings: false,
  loadingSourceSystemOwners: false,
  selectedShipper: null,
  allShippers: null,
  shipperMappings: null,
  sourceSystemOwners: null,
  savingCustomer: false,
  saveCustomerSucceeded: false,
  savingMapping: false,
});

export function ShipperProfileReducer(state: ShipperProfileState = initialState, action: ShipperProfileActions): ShipperProfileState {
  switch (action.type) {
    case ShipperProfileActionTypes.Load: {
      return { ...state, loadingShipper: true };
    }
    case ShipperProfileActionTypes.Load_Success: {
      return { ...state, selectedShipper: action.payload, loadingShipper: false };
    }
    case ShipperProfileActionTypes.Load_Failure: {
      return { ...state, loadingShipper: false };
    }
    case ShipperProfileActionTypes.Load_New: {
      return { ...state, selectedShipper: action.payload, loadingShipper: false };
    }
    case ShipperProfileActionTypes.Add:
    case ShipperProfileActionTypes.Update:
    case ShipperProfileActionTypes.EnableShipperApi: {
      // case ShipperProfileActionTypes.Delete: {
      return { ...state, savingCustomer: true, saveCustomerSucceeded: false };
    }
    // case ShipperProfileActionTypes.Delete_Failure:
    case ShipperProfileActionTypes.Add_Failure:
    case ShipperProfileActionTypes.Update_Failure:
    case ShipperProfileActionTypes.Clear_Save_Succeeded:
    case ShipperProfileActionTypes.EnableShipperApi_Failure: {
      return { ...state, savingCustomer: false, saveCustomerSucceeded: false };
    }

    case ShipperProfileActionTypes.Add_Success: {
      return adapter.addOne(action.payload, {
        ...state,
        selectedShipper: action.payload,
        savingCustomer: false,
        saveCustomerSucceeded: true,
      });
    }
    case ShipperProfileActionTypes.Update_Success:
    case ShipperProfileActionTypes.EnableShipperApi_Success: {
      return adapter.updateOne(
        { id: action.payload.customerId, changes: action.payload },
        { ...state, selectedShipper: action.payload, savingCustomer: false, saveCustomerSucceeded: true }
      );
    }
    /*
        case ShipperProfileActionTypes.Delete_Success: {
            return adapter.removeOne(action.payload.ShipperProfileId,
                { ...state, savingCarrierGroup: false, saveCarrierGroupSucceeded: true });
        }
*/
    case ShipperProfileActionTypes.Load_Shippers: {
      return { ...state, loadingShippers: true };
    }
    case ShipperProfileActionTypes.Load_Shippers_Success: {
      return adapter.addAll(action.payload, { ...state, loadingShippers: false });
    }
    case ShipperProfileActionTypes.Load_Shippers_Failure: {
      return { ...state, loadingShippers: false };
    }
    case ShipperProfileActionTypes.Load_Shipper_Mappings: {
      return { ...state, loadingShipperMappings: true };
    }
    case ShipperProfileActionTypes.Load_Shipper_Mappings_Success: {
      return { ...state, shipperMappings: action.payload, loadingShipperMappings: false };
    }
    case ShipperProfileActionTypes.Load_Shipper_Mappings_Failure: {
      return { ...state, loadingShipperMappings: false };
    }
    case ShipperProfileActionTypes.Load_SourceSystem_Owners: {
      return { ...state, loadingSourceSystemOwners: true };
    }
    case ShipperProfileActionTypes.Load_SourceSystem_OwnersSuccess: {
      return { ...state, sourceSystemOwners: action.payload, loadingSourceSystemOwners: false };
    }
    case ShipperProfileActionTypes.Load_SourceSystem_OwnersFailure: {
      return { ...state, loadingSourceSystemOwners: false };
    }
    case ShipperProfileActionTypes.Create_Shipper_Mapping:
    case ShipperProfileActionTypes.Update_Shipper_Mapping: {
      return { ...state, savingMapping: true };
    }
    case ShipperProfileActionTypes.Create_Shipper_Mapping_Success:
    case ShipperProfileActionTypes.Update_Shipper_Mapping_Success: {
        return { ...state, savingMapping: false, loadingShipperMappings: true };
    }
    case ShipperProfileActionTypes.Create_Shipper_Mapping_Failure:
    case ShipperProfileActionTypes.Update_Shipper_Mapping_Failure: {
      return { ...state, savingMapping: false };
    }
    default:
      return state;
  }
}

export const selectors = adapter.getSelectors();
