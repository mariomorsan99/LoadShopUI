import { LoadStatusNotificationsData, User, ValidationProblemDetails } from '../../../shared/models';
import { UserProfileActions, UserProfileActionTypes } from '../actions';

export interface UserProfileState {
  loaded: boolean;
  loading: boolean;
  saving: boolean;
  entity: User;
  savingUserProfileProblemDetails: ValidationProblemDetails;
  loadStatusNotifications: LoadStatusNotificationsData;
  savingLoadStatusNotificationsProblemDetails: ValidationProblemDetails;
  savedLoadStatusNotificationsProblemDetails: boolean;
}

const initialState: UserProfileState = {
  loaded: false,
  loading: false,
  saving: false,
  entity: null,
  savingUserProfileProblemDetails: null,
  loadStatusNotifications: null,
  savingLoadStatusNotificationsProblemDetails: null,
  savedLoadStatusNotificationsProblemDetails: false,
};

export function userProfileReducer(state: UserProfileState = initialState, action: UserProfileActions): UserProfileState {
  switch (action.type) {
    case UserProfileActionTypes.Load: {
      return Object.assign({}, state, {
        loading: true,
        savingUserProfileProblemDetails: null,
      });
    }
    case UserProfileActionTypes.Load_Success: {
      return Object.assign({}, state, {
        entity: action.payload,
        loading: false,
        savingUserProfileProblemDetails: null,
      });
    }
    case UserProfileActionTypes.Update: {
      return Object.assign({}, state, {
        loading: true,
        saving: true,
        savingUserProfileProblemDetails: null,
      });
    }
    case UserProfileActionTypes.Update_Success: {
      return Object.assign({}, state, {
        entity: action.payload,
        loading: false,
        saving: false,
        savingUserProfileProblemDetails: null,
      });
    }
    case UserProfileActionTypes.Update_Failure: {
      return Object.assign({}, state, {
        loading: false,
        saving: false,
        savingUserProfileProblemDetails: action.payload.error,
      });
    }
    case UserProfileActionTypes.LoadStatusNotifications_Load: {
      return {
        ...state,
        loading: false,
        saving: false,
        loadStatusNotifications: null,
        savingLoadStatusNotificationsProblemDetails: null,
        savedLoadStatusNotificationsProblemDetails: false,
      };
    }
    case UserProfileActionTypes.LoadStatusNotifications_Success: {
      return {
        ...state,
        loading: false,
        saving: false,
        loadStatusNotifications: action.payload,
        savingLoadStatusNotificationsProblemDetails: null,
        savedLoadStatusNotificationsProblemDetails: false,
      };
    }
    case UserProfileActionTypes.LoadStatusNotificationsUpdate: {
      return {
        ...state,
        loading: true,
        saving: false,
        loadStatusNotifications: null,
        savingLoadStatusNotificationsProblemDetails: null,
        savedLoadStatusNotificationsProblemDetails: false,
      };
    }
    case UserProfileActionTypes.LoadStatusNotificationsUpdate_Success: {
      return {
        ...state,
        loading: false,
        saving: false,
        loadStatusNotifications: null,
        savingLoadStatusNotificationsProblemDetails: null,
        savedLoadStatusNotificationsProblemDetails: true,
      };
    }
    case UserProfileActionTypes.LoadStatusNotificationsUpdate_Failure: {
      return {
        ...state,
        loading: false,
        saving: false,
        loadStatusNotifications: null,
        savingLoadStatusNotificationsProblemDetails: action.payload.error,
        savedLoadStatusNotificationsProblemDetails: false,
      };
    }
    default:
      return state;
  }
}

export const getLoaded = (state: UserProfileState) => state.loaded;
export const getLoading = (state: UserProfileState) => state.loading;
export const getSavingValidationProblems = (state: UserProfileState) => state.savingUserProfileProblemDetails;
export const getSaving = (state: UserProfileState) => state.saving;
export const getEntity = (state: UserProfileState) => state.entity;
