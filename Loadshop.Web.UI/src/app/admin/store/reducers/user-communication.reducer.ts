import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import {
  UserCommunication,
  UserCommunicationDetail,
  Carrier,
  Customer,
  UserAdminData,
  ISecurityAccessRoleData,
  defualtUserCommunicationDetail,
} from 'src/app/shared/models';
import { UserCommunicationActions, UserCommunicationActionTypes } from '../actions';

export interface UserCommunicationState extends EntityState<UserCommunication> {
  loadingUserCommunication: boolean;
  updatingUserCommunication: boolean;
  loadingAllUserCommunications: boolean;
  loadingSelectedUserCommunication: boolean;
  selectedUserCommunication: UserCommunicationDetail;
  loadingAllUsers: boolean;
  allUsers: UserAdminData[];
  loadingAllCarriers: boolean;
  allCarriers: Carrier[];
  loadingAllShippers: boolean;
  allShippers: Customer[];
  loadingAllSecurityRoles: boolean;
  allSecurityRoles: ISecurityAccessRoleData[];
  createMode: boolean;
}

export const adapter: EntityAdapter<UserCommunication> = createEntityAdapter<UserCommunication>({
  selectId: x => x.userCommunicationId,
});

const initialState: UserCommunicationState = adapter.getInitialState({
  loadingUserCommunication: false,
  updatingUserCommunication: false,
  loadingAllUserCommunications: false,
  loadingSelectedUserCommunication: false,
  selectedUserCommunication: null,
  loadingAllUsers: false,
  allUsers: [],
  loadingAllCarriers: false,
  allCarriers: [],
  loadingAllShippers: false,
  allShippers: [],
  loadingAllSecurityRoles: false,
  allSecurityRoles: [],
  createMode: false,
});

export function UserCommunicationReducer(
  state: UserCommunicationState = initialState,
  action: UserCommunicationActions
): UserCommunicationState {
  switch (action.type) {
    case UserCommunicationActionTypes.Load: {
      return { ...state, loadingUserCommunication: true, selectedUserCommunication: null };
    }

    case UserCommunicationActionTypes.Load_Success: {
      return adapter.upsertOne(action.payload, {
        ...state,
        selectedUserCommunication: action.payload,
        loadingUserCommunication: false,
        createMode: false,
      });
    }

    case UserCommunicationActionTypes.Load_Failure: {
      return { ...state, loadingUserCommunication: false, selectedUserCommunication: null };
    }

    case UserCommunicationActionTypes.Update: {
      return { ...state, updatingUserCommunication: true };
    }

    case UserCommunicationActionTypes.Update_Success: {
      return adapter.upsertOne(action.payload, { ...state, selectedUserCommunication: action.payload, updatingUserCommunication: false });
    }

    case UserCommunicationActionTypes.Update_Failure: {
      return { ...state, updatingUserCommunication: false };
    }

    case UserCommunicationActionTypes.Create: {
      return { ...state, updatingUserCommunication: true };
    }

    case UserCommunicationActionTypes.Create_Success: {
      return { ...state, updatingUserCommunication: false, selectedUserCommunication: null, createMode: false };
    }

    case UserCommunicationActionTypes.Create_Failure: {
      return { ...state, updatingUserCommunication: false };
    }

    case UserCommunicationActionTypes.Cancel_Update: {
      return { ...state, selectedUserCommunication: null, createMode: false };
    }

    case UserCommunicationActionTypes.Load_All: {
      return adapter.removeAll({ ...state, loadingAllUserCommunications: true });
    }

    case UserCommunicationActionTypes.Load_All_Success: {
      return adapter.addAll(action.payload, { ...state, loadingAllUserCommunications: false });
    }

    case UserCommunicationActionTypes.Load_All_Failure: {
      return adapter.removeAll({ ...state, loadingAllUserCommunications: false });
    }

    case UserCommunicationActionTypes.Create_Default: {
      return {
        ...state,
        selectedUserCommunication: { ...defualtUserCommunicationDetail },
        loadingUserCommunication: false,
        createMode: true,
      };
    }

    default: {
      return state;
    }
  }
}

export const selectors = adapter.getSelectors();
