import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { ValidationProblemDetails } from 'src/app/shared/models/validation-problem-details';
import { IShippingLoadDetail } from '../../../shared/models';
import { ShippingLoadDetailActions, ShippingLoadDetailActionTypes } from '../actions';

export interface ShippingLoadDetailState extends EntityState<IShippingLoadDetail> {
  loading: boolean; // TODO: Switch to a counter for in progress calls?
  postValidationProblemDetails: ValidationProblemDetails;
  postedLoads: IShippingLoadDetail[];
}

export const adapter: EntityAdapter<IShippingLoadDetail> = createEntityAdapter<IShippingLoadDetail>({
  selectId: x => x.loadId,
});

const initialState: ShippingLoadDetailState = adapter.getInitialState({
  loading: false,
  postValidationProblemDetails: null,
  postedLoads: null,
});

export function ShippingLoadDetailReducer(
  state: ShippingLoadDetailState = initialState,
  action: ShippingLoadDetailActions
): ShippingLoadDetailState {
  switch (action.type) {
    case ShippingLoadDetailActionTypes.Load_All: {
      return { ...state, loading: true, postValidationProblemDetails: null };
    }
    case ShippingLoadDetailActionTypes.Load_All_Success: {
      return adapter.addAll(action.payload, { ...state, loading: false });
    }
    case ShippingLoadDetailActionTypes.Load_All_Failure: {
      return adapter.removeAll({ ...state, entity: null, loading: false });
    }
    case ShippingLoadDetailActionTypes.Post_Loads: {
      return { ...state, loading: true, postedLoads: null, postValidationProblemDetails: null };
    }
    case ShippingLoadDetailActionTypes.Post_Loads_Success: {
      /**
       * Update all posted loads on the Shipping Detail screen and save any present
       * validation problem details to state, so that it can be used to display errors
       * on the individual loads.
       */
      const postedLoads = action.payload.postedLoads;
      const validationProblems = action.payload.validationProblemDetails;
      const nextState = {
        ...state,
        loading: false,
        postValidationProblemDetails: validationProblems ? validationProblems : null,
        postedLoads: postedLoads && postedLoads.length ? postedLoads : null,
      };

      if (postedLoads && postedLoads.length > 0) {
        return adapter.upsertMany(postedLoads, nextState);
      } else {
        return nextState;
      }
    }
    case ShippingLoadDetailActionTypes.Post_Loads_Failure: {
      return { ...state, loading: false };
    }
    case ShippingLoadDetailActionTypes.Remove_Load: {
      return { ...state, loading: true };
    }
    case ShippingLoadDetailActionTypes.Remove_Load_Success: {
      delete action.payload.loadStops;
      return adapter.updateOne({ id: action.payload.loadId, changes: { ...action.payload } }, { ...state, loading: false });
    }
    case ShippingLoadDetailActionTypes.Remove_Load_Failure: {
      return { ...state, loading: false };
    }
    case ShippingLoadDetailActionTypes.Delete_Load: {
      return { ...state, loading: true };
    }
    case ShippingLoadDetailActionTypes.Delete_Load_Success: {
      return adapter.removeOne(action.payload.loadId, { ...state, loading: false });
    }
    case ShippingLoadDetailActionTypes.Delete_Load_Failure: {
      return { ...state, loading: false };
    }
    case ShippingLoadDetailActionTypes.Discard_Changes: {
      if (!action.payload || !action.payload.loadId || !state.postValidationProblemDetails || !state.postValidationProblemDetails.errors) {
        return state;
      }

      const loadId = action.payload.loadId;
      const nextState = { ...state };
      const errors = nextState.postValidationProblemDetails.errors;
      Object.keys(errors).forEach(key => {
        if (key.startsWith(`urn:load:${loadId}`)) {
          delete errors[key];
        }
      });
      return {
        ...nextState,
        postValidationProblemDetails: {
          ...nextState.postValidationProblemDetails,
          errors: errors,
        },
      };
    }
    case ShippingLoadDetailActionTypes.Update_Load: {
      return adapter.updateOne({ id: action.payload.loadId, changes: { ...action.payload } }, { ...state });
    }
    default:
      return state;
  }
}

const selectors = adapter.getSelectors();
export const getLoads = (state: ShippingLoadDetailState) => selectors.selectAll(state);
export const getLoading = (state: ShippingLoadDetailState) => state.loading;
export const getPostValidationProblemDetails = (state: ShippingLoadDetailState) => state.postValidationProblemDetails;
export const getSuccessfullyPostedLoads = (state: ShippingLoadDetailState) => state.postedLoads;
