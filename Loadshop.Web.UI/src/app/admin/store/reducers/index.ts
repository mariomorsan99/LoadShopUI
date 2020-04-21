import { ActionReducerMap, createFeatureSelector } from '@ngrx/store';
import { loadCarrierGroupCarrierReducer, LoadCarrierGroupCarrierState } from './load-carrier-group-carrier.reducer';
import { loadCarrierGroupReducer, LoadCarrierGroupState } from './load-carrier-group.reducer';
import { ShipperProfileReducer, ShipperProfileState } from './shipper-profile.reducer';
import { specialInstructionsReducer, SpecialInstructionsState } from './special-instructions.reducer';
import { UserAdminReducer, UserAdminState } from './user-admin.reducer';
import { CarrierProfileReducer, CarrierProfileState } from './carrier-profile.reducer';
import { UserCommunicationState, UserCommunicationReducer } from './user-communication.reducer';

export interface AdminState {
  loadCarrierGroups: LoadCarrierGroupState;
  loadCarrierGroupCarriers: LoadCarrierGroupCarrierState;
  shipperProfile: ShipperProfileState;
  userAdmin: UserAdminState;
  specialInstructions: SpecialInstructionsState;
  carrierProfile: CarrierProfileState;
  userComunnication: UserCommunicationState;
}

export const reducers: ActionReducerMap<AdminState> = {
  loadCarrierGroups: loadCarrierGroupReducer,
  loadCarrierGroupCarriers: loadCarrierGroupCarrierReducer,
  shipperProfile: ShipperProfileReducer,
  userAdmin: UserAdminReducer,
  specialInstructions: specialInstructionsReducer,
  carrierProfile: CarrierProfileReducer,
  userComunnication: UserCommunicationReducer
};

export const getAdminFeatureState = createFeatureSelector<AdminState>('admin');
