import { createSelector } from '@ngrx/store';
import { UserModel } from 'src/app/shared/models';
import { getUserFeatureState, UserState } from '../reducers';
import { getEntity, getLoaded, getLoading, getSaving, getSavingValidationProblems } from '../reducers/user-profile.reducer';

const getUserProfileState = createSelector(getUserFeatureState, (state: UserState) => state.userProfile);

export const getUserProfileEntity = createSelector(getUserProfileState, getEntity);
export const getUserProfileModel = createSelector(getUserProfileEntity, user => (user ? new UserModel(user) : null));
export const getUserProfileLoading = createSelector(getUserProfileState, getLoading);
export const getUserProfileProblemDetails = createSelector(getUserProfileState, getSavingValidationProblems);
export const getUserProfileLoaded = createSelector(getUserProfileState, getLoaded);
export const getUserProfileSaving = createSelector(getUserProfileState, getSaving);

export const getLoadStatusNotifications = createSelector(getUserProfileState, x => x.loadStatusNotifications);
export const getLoadStatusNotificationsProblemDetails = createSelector(
  getUserProfileState,
  x => x.savingLoadStatusNotificationsProblemDetails
);

export const getSavedLoadStatusNotificationsProblemDetails = createSelector(
  getUserProfileState,
  x => x.savedLoadStatusNotificationsProblemDetails
);
