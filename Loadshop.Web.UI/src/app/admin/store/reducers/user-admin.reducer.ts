import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { CarrierScac, Customer, ISecurityAccessRoleData, UserAdminData, Carrier } from '../../../shared/models';
import { UserAdminActions, UserAdminActionTypes } from '../actions';

export interface UserAdminState extends EntityState<UserAdminData> {
  loadingUsers: boolean;
  loadingUser: boolean;
  loadingShippers: boolean;
  loadingCarrierScacs: boolean;
  loadingSecurityRoles: boolean;
  loadingAdminUsers: boolean;
  loadingIdentityUser: boolean;
  savingUser: boolean;
  createMode: boolean;
  selectedUser: UserAdminData;
  allAuthorizedShippers: Customer[];
  allAuthorizedCarrierScacs: CarrierScac[];
  allAuthorizedSecurityRoles: ISecurityAccessRoleData[];
  adminUsers: UserAdminData[];
  loadingCarriers: boolean;
  allAuthorizedCarriers: Carrier[];
  loadingCarrierUsers: boolean;
  carrierUsers: UserAdminData[];
}

export const adapter: EntityAdapter<UserAdminData> = createEntityAdapter<UserAdminData>({
  selectId: x => x.userId,
});

const initialState: UserAdminState = adapter.getInitialState({
  loadingUsers: false,
  loadingUser: false,
  loadingShippers: false,
  loadingCarrierScacs: false,
  loadingSecurityRoles: false,
  loadingAdminUsers: false,
  loadingIdentityUser: false,
  savingUser: false,
  createMode: false,
  selectedUser: null,
  allAuthorizedShippers: [],
  allAuthorizedCarrierScacs: [],
  allAuthorizedSecurityRoles: [],
  adminUsers: [],
  loadingCarriers: false,
  allAuthorizedCarriers: [],
  loadingCarrierUsers: false,
  carrierUsers: []
});

export function UserAdminReducer(state: UserAdminState = initialState, action: UserAdminActions): UserAdminState {
  switch (action.type) {
    case UserAdminActionTypes.Load_Users: {
      return { ...state, loadingUsers: true };
    }
    case UserAdminActionTypes.Load_Users_Failure: {
      return adapter.removeAll({ ...state, loadingUsers: false });
    }
    case UserAdminActionTypes.Load_Users_Success: {
      return adapter.addAll(action.payload, { ...state, loadingUsers: false });
    }
    case UserAdminActionTypes.Load_AdminUsers: {
      return { ...state, loadingAdminUsers: true };
    }
    case UserAdminActionTypes.Load_AdminUsers_Failure: {
      return { ...state, loadingAdminUsers: false };
    }
    case UserAdminActionTypes.Load_AdminUsers_Success: {
      return { ...state, adminUsers: action.payload, loadingAdminUsers: false };
    }
    case UserAdminActionTypes.Load_User: {
      return { ...state, loadingUser: true };
    }
    case UserAdminActionTypes.Load_User_Failure: {
      return adapter.removeAll({ ...state, loadingUser: false });
    }
    case UserAdminActionTypes.Load_User_Success: {
      return adapter.upsertOne(action.payload, { ...state, selectedUser: action.payload, loadingUser: false, createMode: false });
    }
    case UserAdminActionTypes.Clear_Selected_User: {
      return { ...state, selectedUser: null, loadingUser: false, createMode: false };
    }
    case UserAdminActionTypes.Load_Authorized_Shippers: {
      return { ...state, loadingShippers: true };
    }
    case UserAdminActionTypes.Load_Authorized_Shippers_Success: {
      return { ...state, allAuthorizedShippers: action.payload, loadingShippers: false };
    }
    case UserAdminActionTypes.Load_Authorized_Shippers_Failure: {
      return { ...state, allAuthorizedShippers: [], loadingShippers: false };
    }
    case UserAdminActionTypes.Load_Authorized_CarrierScacs: {
      return { ...state, loadingCarrierScacs: true };
    }
    case UserAdminActionTypes.Load_Authorized_CarrierScacs_Success: {
      return { ...state, allAuthorizedCarrierScacs: action.payload, loadingCarrierScacs: false };
    }
    case UserAdminActionTypes.Load_Authorized_CarrierScacs_Failure: {
      return { ...state, allAuthorizedCarrierScacs: [], loadingCarrierScacs: false };
    }
    case UserAdminActionTypes.Load_Authorized_Security_Roles: {
      return { ...state, loadingSecurityRoles: true };
    }
    case UserAdminActionTypes.Load_Authorized_Security_Roles_Success: {
      return { ...state, allAuthorizedSecurityRoles: action.payload, loadingSecurityRoles: false };
    }
    case UserAdminActionTypes.Load_Authorized_Security_Roles_Failure: {
      return { ...state, allAuthorizedSecurityRoles: [], loadingSecurityRoles: false };
    }
    case UserAdminActionTypes.Create_User:
    case UserAdminActionTypes.Update_User: {
      return { ...state, savingUser: true };
    }
    case UserAdminActionTypes.Create_User_Success:
    case UserAdminActionTypes.Update_User_Success: {
      return { ...state, selectedUser: null, savingUser: false, createMode: false };
    }
    case UserAdminActionTypes.Create_User_Failure:
    case UserAdminActionTypes.Update_User_Failure: {
      return { ...state, savingUser: false };
    }
    case UserAdminActionTypes.Load_Identity_User:
      return { ...state, loadingIdentityUser: true };
    case UserAdminActionTypes.Load_Identity_User_Success: {
      return { ...state, selectedUser: action.payload, loadingIdentityUser: false, createMode: true };
    }
    case UserAdminActionTypes.Load_Identity_User_Failure:
    case UserAdminActionTypes.Load_Identity_User_Not_Found:
      return { ...state, selectedUser: null, loadingIdentityUser: false, createMode: false };
    case UserAdminActionTypes.Load_Authorized_Carriers: {
      return { ...state, loadingCarriers: true };
    }
    case UserAdminActionTypes.Load_Authorized_Carriers_Success: {
      return { ...state, allAuthorizedCarriers: action.payload, loadingCarriers: false };
    }
    case UserAdminActionTypes.Load_Authorized_Carriers_Failure: {
      return { ...state, allAuthorizedCarriers: [], loadingCarriers: false };
    }
    case UserAdminActionTypes.Load_Carrier_Users: {
      return { ...state, loadingCarrierUsers: true };
    }
    case UserAdminActionTypes.Load_Carrier_Users_Failure: {
      return { ...state, carrierUsers: [], loadingCarrierUsers: false };
    }
    case UserAdminActionTypes.Load_Carrier_Users_Success: {
      return { ...state, carrierUsers: action.payload, loadingCarrierUsers: false };
    }
    default:
      return state;
  }
}

export const selectors = adapter.getSelectors();
