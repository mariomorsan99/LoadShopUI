import { defaultUserFocusEntityResult, UserFocusEntityResult } from '../../../shared/models';
import { UserFocusEntitySelectorActions, UserFocusEntitySelectorTypes } from '../actions';

export interface UserFocusEntitySelectorState {
  loaded: boolean;
  loading: boolean;
  saving: boolean;
  authorizedEntities: UserFocusEntityResult;
}

const initialState: UserFocusEntitySelectorState = {
  loaded: false,
  loading: false,
  saving: false,
  authorizedEntities: defaultUserFocusEntityResult,
};

export function userFocusEntitySelectorReducer(
  state: UserFocusEntitySelectorState = initialState,
  action: UserFocusEntitySelectorActions
): UserFocusEntitySelectorState {
  switch (action.type) {
    case UserFocusEntitySelectorTypes.LoadMyAuthorizedEntities_Success: {
      return Object.assign({}, state, {
        authorizedEntities: action.payload,
      });
    }
    case UserFocusEntitySelectorTypes.LoadMyAuthorizedEntities_Fail: {
      return Object.assign({}, state, {
        authorizedEntities: defaultUserFocusEntityResult,
      });
    }
    default:
      return state;
  }
}

export const getLoaded = (state: UserFocusEntitySelectorState) => state.loaded;
export const getLoading = (state: UserFocusEntitySelectorState) => state.loading;
export const getSaving = (state: UserFocusEntitySelectorState) => state.saving;
export const getAuthorizedFocusEntities = (state: UserFocusEntitySelectorState) => state.authorizedEntities;
