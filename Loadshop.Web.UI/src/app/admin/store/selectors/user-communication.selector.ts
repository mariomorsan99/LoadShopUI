import { createSelector } from '@ngrx/store';
import { AdminState, getAdminFeatureState } from '../reducers';
import { selectors, UserCommunicationState } from '../reducers/user-communication.reducer';

const getUserCommunicationState = createSelector(getAdminFeatureState, (state: AdminState) => state.userComunnication);

export const getUserCommunications = createSelector(getUserCommunicationState, (state: UserCommunicationState) =>
  selectors.selectAll(state)
);
export const getAllUserCommunicationsLoading = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.loadingAllUserCommunications
);

export const getUserCommunicationAllCarriers = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.allCarriers
);
export const getUserCommunicationAllCarriersLoading = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.loadingAllCarriers
);

export const getUserCommunicationAllShippers = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.allShippers
);
export const getAllShippersLoading = createSelector(getUserCommunicationState, (state: UserCommunicationState) => state.loadingAllShippers);

export const getUserCommunicationAllSecurityRoles = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.allSecurityRoles
);
export const getUserCommunicationAllSecurityRolesLoading = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.loadingAllSecurityRoles
);

export const getUserCommunicationAllUsers = createSelector(getUserCommunicationState, (state: UserCommunicationState) => state.allUsers);
export const getUserCommunicationAllUsersLoading = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.loadingAllUsers
);

export const getSelectedUserCommunciation = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.selectedUserCommunication
);
export const getSelectedUserCommunciationLoading = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.loadingSelectedUserCommunication
);

export const getSelectedUserCommunciationUpdating = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.updatingUserCommunication
);

export const getUserCommunicationCreateMode = createSelector(
  getUserCommunicationState,
  (state: UserCommunicationState) => state.createMode
);
