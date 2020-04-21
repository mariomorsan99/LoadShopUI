
import { ActionReducerMap, createFeatureSelector } from '@ngrx/store';
import { UserProfileState, userProfileReducer } from './user-profile.reducer';
import { UserLaneState, userLaneReducer } from './user-lane.reducer';
import { UserFocusEntitySelectorState, userFocusEntitySelectorReducer} from './user-focus-entity-selector.reducer';

export interface UserState {
    userProfile: UserProfileState;
    userLanes: UserLaneState;
    userFocusEntitySelector: UserFocusEntitySelectorState;
}

export const reducers: ActionReducerMap<UserState> = {
    userProfile: userProfileReducer,
    userLanes: userLaneReducer,
    userFocusEntitySelector: userFocusEntitySelectorReducer
};

export const getUserFeatureState = createFeatureSelector<UserState>('user');
