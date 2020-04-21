import { createSelector } from '@ngrx/store';
import { AdminState, getAdminFeatureState } from '../reducers';
import { selectors, UserAdminState } from '../reducers/user-admin.reducer';

const getUserAdminState = createSelector(getAdminFeatureState,
    (state: AdminState) => state.userAdmin);

export const getUsers = createSelector(getUserAdminState, (state: UserAdminState) => selectors.selectAll(state));
export const getUsersLoading = createSelector(getUserAdminState, (state: UserAdminState) => state.loadingUsers);

export const getAdminUsers = createSelector(getUserAdminState, (state: UserAdminState) => state.adminUsers);
export const getAdminUsersLoading = createSelector(getUserAdminState, (state: UserAdminState) => state.loadingAdminUsers);

export const getCarrierScacsLoading = createSelector(getUserAdminState, (state: UserAdminState) => state.loadingCarrierScacs);
export const getAllAuthorizedCarrierScacs = createSelector(getUserAdminState, (state: UserAdminState) => state.allAuthorizedCarrierScacs);

export const getShippersLoading = createSelector(getUserAdminState, (state: UserAdminState) => state.loadingCarrierScacs);
export const getAllAuthorizedShippers = createSelector(getUserAdminState, (state: UserAdminState) => state.allAuthorizedShippers);

export const getSecurityRolesLoading = createSelector(getUserAdminState, (state: UserAdminState) => state.loadingSecurityRoles);
export const getAllAuthorizedSecurityRoles = createSelector(getUserAdminState, (state: UserAdminState) => state.allAuthorizedSecurityRoles);

export const getSelectedUser = createSelector(getUserAdminState, (state: UserAdminState) => state.selectedUser);
export const getLoadingSelectedUser = createSelector(getUserAdminState, (state: UserAdminState) => state.loadingUser);

export const getCreateMode = createSelector(getUserAdminState, (state: UserAdminState) => state.createMode);
export const getLoadingIdentityUser = createSelector(getUserAdminState, (state: UserAdminState) => state.loadingIdentityUser);

export const getSavingUser = createSelector(getUserAdminState, (state: UserAdminState) => state.savingUser);

export const getCarriersLoading = createSelector(getUserAdminState, (state: UserAdminState) => state.loadingCarriers);
export const getAllAuthorizedCarriers = createSelector(getUserAdminState, (state: UserAdminState) => state.allAuthorizedCarriers);
export const getLoadingCarrierUsers = createSelector(getUserAdminState, (state: UserAdminState) => state.loadingCarrierUsers);
export const getCarrierUsers = createSelector(getUserAdminState, (state: UserAdminState) => state.carrierUsers);
