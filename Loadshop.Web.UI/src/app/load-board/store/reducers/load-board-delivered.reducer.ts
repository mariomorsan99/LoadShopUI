import { LoadBoardDeliveredActionTypes, LoadBoardDeliveredActions } from '../../../load-board/store/actions';
import { LoadView } from '../../../shared/models';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { PageableQueryHelper } from 'src/app/shared/utilities';

export interface LoadBoardDeliveredState extends EntityState<LoadView> {
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

const initialState: LoadBoardDeliveredState = adapter.getInitialState({
  loaded: false,
  loading: false,
  savingLoadId: null,
  errorLoadId: null,
  phoneError: false,
  truckError: false,
  totalRecords: 0,
  queryHelper: PageableQueryHelper.default(),
});

export function LoadBoardDeliveredReducer(
  state: LoadBoardDeliveredState = initialState,
  action: LoadBoardDeliveredActions
): LoadBoardDeliveredState {
  switch (action.type) {
    case LoadBoardDeliveredActionTypes.Load: {
      return adapter.removeAll({ ...state, loading: true });
    }
    case LoadBoardDeliveredActionTypes.Load_Success: {
      return adapter.addAll(action.payload.data, { ...state, loading: false, totalRecords: action.payload.totalRecords });
    }
    case LoadBoardDeliveredActionTypes.Save_Load: {
      return Object.assign({}, state, { savingLoadId: action.payload.loadId });
    }
    case LoadBoardDeliveredActionTypes.Save_Load_Success: {
      return Object.assign({}, state, { errorLoadId: null }, { phoneError: false }, { truckError: false }, { savingLoadId: null });
    }
    case LoadBoardDeliveredActionTypes.Update_Query: {
      return { ...state, queryHelper: action.payload };
    }

    case LoadBoardDeliveredActionTypes.Save_Load_Failure: {
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

export const getLoaded = (state: LoadBoardDeliveredState) => state.loaded;
export const getLoading = (state: LoadBoardDeliveredState) => state.loading;
export const getSavingLoadId = (state: LoadBoardDeliveredState) => state.savingLoadId;
export const getErrorLoadId = (state: LoadBoardDeliveredState) => state.errorLoadId;
export const getPhoneError = (state: LoadBoardDeliveredState) => state.phoneError;
export const getTruckError = (state: LoadBoardDeliveredState) => state.truckError;
export const getDeliveredTotalRecords = (state: LoadBoardDeliveredState) => state.totalRecords;
export const getDeliveredQueryHelper = (state: LoadBoardDeliveredState) => state.queryHelper;
export const selectors = adapter.getSelectors();
