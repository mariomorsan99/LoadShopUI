import { LoadBoardBookedActionTypes, LoadBoardBookedActions } from '../../../load-board/store/actions';
import { LoadView } from '../../../shared/models';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { PageableQueryHelper } from 'src/app/shared/utilities';

export interface LoadBoardBookedState extends EntityState<LoadView> {
  loaded: boolean;
  loading: boolean;
  savingLoadId: string;
  errorLoadId: string;
  phoneError: boolean;
  truckError: boolean;
  totalRecords: number;
  queryHelper: PageableQueryHelper;
}

export const adapter: EntityAdapter<LoadView> = createEntityAdapter<LoadView>({
  selectId: x => x.loadId,
});

const initialState: LoadBoardBookedState = adapter.getInitialState({
  loaded: false,
  loading: false,
  savingLoadId: null,
  errorLoadId: null,
  phoneError: false,
  truckError: false,
  totalRecords: 0,
  queryHelper: PageableQueryHelper.default(),
});

export function LoadBoardBookedReducer(state: LoadBoardBookedState = initialState, action: LoadBoardBookedActions): LoadBoardBookedState {
  switch (action.type) {
    case LoadBoardBookedActionTypes.Load: {
      return adapter.removeAll({ ...state, loading: true });
    }
    case LoadBoardBookedActionTypes.Load_Success: {
      return adapter.addAll(action.payload.data, { ...state, loading: false, totalRecords: action.payload.totalRecords });
    }
    case LoadBoardBookedActionTypes.Save_Load: {
      return Object.assign({}, state, { savingLoadId: action.payload.loadId });
    }
    case LoadBoardBookedActionTypes.Save_Load_Success: {
      return Object.assign({}, state, { errorLoadId: null }, { phoneError: false }, { truckError: false }, { savingLoadId: null });
    }
    case LoadBoardBookedActionTypes.Update_Query: {
      return { ...state, queryHelper: action.payload };
    }

    case LoadBoardBookedActionTypes.Save_Load_Failure: {
      return Object.assign(
        {},
        state,
        { errorLoadId: state.savingLoadId },
        { phoneError: action.payload.message.toLowerCase().indexOf('phone') > -1 },
        { truckError: action.payload.message.toLowerCase().indexOf('truck') > -1 },
        { savingLoadId: null }
      );
    }
    default:
      return state;
  }
}

export const getLoaded = (state: LoadBoardBookedState) => state.loaded;
export const getLoading = (state: LoadBoardBookedState) => state.loading;
export const getSavingLoadId = (state: LoadBoardBookedState) => state.savingLoadId;
export const getErrorLoadId = (state: LoadBoardBookedState) => state.errorLoadId;
export const getPhoneError = (state: LoadBoardBookedState) => state.phoneError;
export const getTruckError = (state: LoadBoardBookedState) => state.truckError;
export const getBookedTotalRecords = (state: LoadBoardBookedState) => state.totalRecords;
export const getBookedQueryHelper = (state: LoadBoardBookedState) => state.queryHelper;
export const selectors = adapter.getSelectors();
