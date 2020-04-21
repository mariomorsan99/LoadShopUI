import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { LoadCarrierGroupCarrierData } from '../../../shared/models';
import { LoadCarrierGroupCarrierActions, LoadCarrierGroupCarrierActionTypes } from '../actions';

export interface LoadCarrierGroupCarrierState extends EntityState<LoadCarrierGroupCarrierData> {
  loading: boolean;
  saving: boolean;
}

export const adapter: EntityAdapter<LoadCarrierGroupCarrierData> = createEntityAdapter<LoadCarrierGroupCarrierData>({
  selectId: x => x.loadCarrierGroupCarrierId,
});

const initialState: LoadCarrierGroupCarrierState = adapter.getInitialState({
  loading: false,
  saving: false,
});

export function loadCarrierGroupCarrierReducer(
  state: LoadCarrierGroupCarrierState = initialState,
  action: LoadCarrierGroupCarrierActions
): LoadCarrierGroupCarrierState {
  switch (action.type) {
    case LoadCarrierGroupCarrierActionTypes.Load: {
      return adapter.removeAll({ ...state, loading: true });
    }
    case LoadCarrierGroupCarrierActionTypes.Load_Success: {
      return adapter.addAll(action.payload, { ...state, loading: false });
    }
    case LoadCarrierGroupCarrierActionTypes.Load_Failure: {
      return { ...state, loading: false };
    }

    case LoadCarrierGroupCarrierActionTypes.Delete_All: {
      return { ...state, saving: true };
    }
    case LoadCarrierGroupCarrierActionTypes.Delete_All_Failure: {
      return { ...state, saving: false };
    }
    case LoadCarrierGroupCarrierActionTypes.Delete_All_Success: {
      return adapter.removeAll({ ...state, saving: false });
    }

    default:
      return state;
  }
}

export const selectors = adapter.getSelectors();
