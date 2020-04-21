import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { SpecialInstructionData, ValidationProblemDetails } from '../../../shared/models';
import { SpecialInstructionsActions, SpecialInstructionsActionTypes } from '../actions';

export interface SpecialInstructionsState extends EntityState<SpecialInstructionData> {
  loading: boolean;
  selectedSpecialInstructions: SpecialInstructionData;
  loadingSelectedSpecialInstructions: boolean;
  savingSpecialInstructions: boolean;
  savingSpecialInstructionsSucceeded: boolean;
  savingSpecialInstructionsProblemDetails: ValidationProblemDetails;
}

export const adapter: EntityAdapter<SpecialInstructionData> = createEntityAdapter<SpecialInstructionData>({
  selectId: x => x.specialInstructionId,
});

const initialState: SpecialInstructionsState = adapter.getInitialState({
  loading: false,
  selectedSpecialInstructions: null,
  loadingSelectedSpecialInstructions: false,
  savingSpecialInstructions: false,
  savingSpecialInstructionsSucceeded: false,
  savingSpecialInstructionsProblemDetails: null,
});

export function specialInstructionsReducer(
  state: SpecialInstructionsState = initialState,
  action: SpecialInstructionsActions
): SpecialInstructionsState {
  switch (action.type) {
    case SpecialInstructionsActionTypes.Load: {
      return {
        ...state,
        loading: true,
        savingSpecialInstructionsProblemDetails: null,
      };
    }
    case SpecialInstructionsActionTypes.Load_Success: {
      return adapter.addAll(action.payload, { ...state, loading: false });
    }
    case SpecialInstructionsActionTypes.Load_Failure: {
      return { ...state, loading: false };
    }
    case SpecialInstructionsActionTypes.Load_Instruction: {
      return { ...state, loadingSelectedSpecialInstructions: true };
    }
    case SpecialInstructionsActionTypes.Load_Instruction_Success: {
      if (action.payload && action.payload.specialInstructionId > 0) {
        return adapter.upsertOne(action.payload, {
          ...state,
          loadingSelectedSpecialInstructions: false,
          selectedSpecialInstructions: action.payload,
          loadingSelectedCarrierGroup: false,
          saveCarrierGroupError: null,
        });
      }
      return {
        ...state,
        selectedSpecialInstructions: action.payload,
      };
    }
    case SpecialInstructionsActionTypes.Load_Instruction_Failure: {
      return { ...state, loadingSelectedSpecialInstructions: false };
    }

    case SpecialInstructionsActionTypes.Add:
    case SpecialInstructionsActionTypes.Update:
    case SpecialInstructionsActionTypes.Delete: {
      return {
        ...state,
        savingSpecialInstructions: true,
        savingSpecialInstructionsSucceeded: false,
        savingSpecialInstructionsProblemDetails: null,
      };
    }
    case SpecialInstructionsActionTypes.Add_Failure:
    case SpecialInstructionsActionTypes.Update_Failure: {
      return {
        ...state,
        savingSpecialInstructions: false,
        savingSpecialInstructionsSucceeded: false,
        savingSpecialInstructionsProblemDetails: action.payload.error,
      };
    }
    case SpecialInstructionsActionTypes.Delete_Failure:
    case SpecialInstructionsActionTypes.Clear_Save_Succeeded: {
      return {
        ...state,
        savingSpecialInstructions: false,
        savingSpecialInstructionsSucceeded: false,
        savingSpecialInstructionsProblemDetails: null,
      };
    }

    case SpecialInstructionsActionTypes.Add_Success: {
      return adapter.addOne(action.payload, {
        ...state,
        savingSpecialInstructions: false,
        savingSpecialInstructionsSucceeded: true,
        saveCarrierGroupError: null,
      });
    }
    case SpecialInstructionsActionTypes.Update_Success: {
      return adapter.updateOne(
        { id: action.payload.specialInstructionId, changes: action.payload },
        { ...state, savingSpecialInstructions: false, savingSpecialInstructionsSucceeded: true, saveCarrierGroupError: null }
      );
    }
    case SpecialInstructionsActionTypes.Delete_Success: {
      return adapter.removeOne(action.payload.specialInstructionId, {
        ...state,
        savingSpecialInstructions: false,
        savingSpecialInstructionsSucceeded: true,
        saveCarrierGroupError: null,
      });
    }

    default:
      return state;
  }
}

export const selectors = adapter.getSelectors();
