import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { UserLane } from '../../../shared/models';
import { UserLaneActions, UserLaneActionTypes } from '../actions';

export interface UserLaneState extends EntityState<UserLane> {
  loading: boolean;
  selectedUserLane: UserLane;
}

export const adapter: EntityAdapter<UserLane> = createEntityAdapter<UserLane>({
  selectId: x => x.userLaneId,
});

const initialState: UserLaneState = adapter.getInitialState({
  loading: false,
  selectedUserLane: null,
});

export function userLaneReducer(state: UserLaneState = initialState, action: UserLaneActions): UserLaneState {
  switch (action.type) {
    case UserLaneActionTypes.Add:
    case UserLaneActionTypes.Update:
    case UserLaneActionTypes.Delete:
    case UserLaneActionTypes.Load: {
      return { ...state, loading: true };
    }
    case UserLaneActionTypes.Load_Failure:
    case UserLaneActionTypes.Add_Failure:
    case UserLaneActionTypes.Delete_Failure:
    case UserLaneActionTypes.Update_Failure: {
      return { ...state, loading: false };
    }
    case UserLaneActionTypes.Load_Success: {
      return adapter.addAll(action.payload, { ...state, loading: false });
    }
    case UserLaneActionTypes.ToggleAll_Display: {
      return adapter.updateMany(
        action.payload.map(x => {
          return {
            id: x.userLaneId,
            changes: x,
          };
        }),
        state
      );
    }
    case UserLaneActionTypes.Toggle_Display:
    case UserLaneActionTypes.Update_Success: {
      return adapter.updateOne({ id: action.payload.userLaneId, changes: action.payload }, { ...state, loading: false });
    }
    case UserLaneActionTypes.Add_Success: {
      return adapter.addOne(action.payload, { ...state, loading: false });
    }
    case UserLaneActionTypes.Delete_Success: {
      return adapter.removeOne(action.payload.userLaneId, { ...state, loading: false });
    }
    case UserLaneActionTypes.Selected: {
      return Object.assign({}, state, { selectedUserLane: action.payload });
    }
    default:
      return state;
  }
}

export const selectors = adapter.getSelectors();
