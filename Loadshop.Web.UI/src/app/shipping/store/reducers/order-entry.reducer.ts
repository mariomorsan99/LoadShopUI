import { ValidationProblemDetails } from 'src/app/shared/models';
import { defaultOrderEntry, OrderEntryForm } from 'src/app/shared/models/order-entry-form';
import { OrderEntryActions, OrderEntryActionTypes } from '../actions';

export interface OrderEntryState {
  loading: boolean;
  problemDetails: ValidationProblemDetails;
  orderEntryForm: OrderEntryForm;
  savedId: string;
}

const initialState: OrderEntryState = {
  loading: false,
  problemDetails: null,
  orderEntryForm: defaultOrderEntry,
  savedId: null,
};

export function OrderEntryReducer(state: OrderEntryState = initialState, action: OrderEntryActions): OrderEntryState {
  switch (action.type) {
    case OrderEntryActionTypes.Get_Load: {
      return { ...state, loading: true };
    }
    case OrderEntryActionTypes.Get_Load_Success: {
      return { ...state, orderEntryForm: action.payload, loading: false };
    }
    case OrderEntryActionTypes.Get_Load_Failure: {
      return { ...state, loading: false, problemDetails: action.payload.error };
    }

    case OrderEntryActionTypes.Create_Load: {
      return { ...state, loading: true };
    }
    case OrderEntryActionTypes.Create_Load_Success: {
      return { ...state, ...initialState, savedId: action.payload.referenceLoadId };
    }
    case OrderEntryActionTypes.Create_Load_Failure: {
      return { ...state, loading: false, problemDetails: action.payload.error };
    }

    case OrderEntryActionTypes.Update_Load: {
      return { ...state, loading: true };
    }
    case OrderEntryActionTypes.Update_Load_Success: {
      return { ...state, ...initialState, savedId: action.payload.referenceLoadId };
    }
    case OrderEntryActionTypes.Update_Load_Failure: {
      return { ...state, loading: false, problemDetails: action.payload.error };
    }

    case OrderEntryActionTypes.Clear_Errors: {
      return { ...state, problemDetails: initialState.problemDetails };
    }
    case OrderEntryActionTypes.Reset_Saved: {
      return { ...state, savedId: initialState.savedId };
    }
    default:
      return state;
  }
}

export const getLoading = (state: OrderEntryState) => state.loading;
export const getForm = (state: OrderEntryState) => state.orderEntryForm;
export const getProblemDetails = (state: OrderEntryState) => state.problemDetails;
export const getSavedId = (state: OrderEntryState) => state.savedId;
