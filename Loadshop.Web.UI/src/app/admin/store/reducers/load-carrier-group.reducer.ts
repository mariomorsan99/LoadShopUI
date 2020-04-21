import { createEntityAdapter, EntityAdapter, EntityState } from '@ngrx/entity';
import { LoadCarrierGroupDetailData, LoadCarrierGroupType, ValidationProblemDetails } from '../../../shared/models';
import { LoadCarrierGroupActions, LoadCarrierGroupActionTypes } from '../actions';

export interface LoadCarrierGroupState extends EntityState<LoadCarrierGroupDetailData> {
  loadingCarrierGroups: boolean;
  selectedLoadCarrierGroup: LoadCarrierGroupDetailData;
  loadingSelectedCarrierGroup: boolean;
  savingCarrierGroup: boolean;
  saveCarrierGroupSucceeded: boolean;
  saveCarrierGroupProblemDetails: ValidationProblemDetails;
  loadCarrierGroupTypes: LoadCarrierGroupType[];
  loadCarrierGroupTypesLoading: boolean;
}

export const adapter: EntityAdapter<LoadCarrierGroupDetailData> = createEntityAdapter<LoadCarrierGroupDetailData>({
  selectId: x => x.loadCarrierGroupId,
});

const initialState: LoadCarrierGroupState = adapter.getInitialState({
  loadingCarrierGroups: false,
  selectedLoadCarrierGroup: null,
  loadingSelectedCarrierGroup: false,
  savingCarrierGroup: false,
  saveCarrierGroupSucceeded: false,
  saveCarrierGroupProblemDetails: null,
  loadCarrierGroupTypes: [],
  loadCarrierGroupTypesLoading: false,
});

export function loadCarrierGroupReducer(
  state: LoadCarrierGroupState = initialState,
  action: LoadCarrierGroupActions
): LoadCarrierGroupState {
  switch (action.type) {
    case LoadCarrierGroupActionTypes.Load: {
      return { ...state, loadingCarrierGroups: true, saveCarrierGroupProblemDetails: null };
    }
    case LoadCarrierGroupActionTypes.Load_Success: {
      return adapter.addAll(action.payload, { ...state, loadingCarrierGroups: false, saveCarrierGroupError: null });
    }
    case LoadCarrierGroupActionTypes.Load_Failure: {
      return { ...state, loadingCarrierGroups: false, saveCarrierGroupProblemDetails: null };
    }

    case LoadCarrierGroupActionTypes.Add:
    case LoadCarrierGroupActionTypes.Update:
    case LoadCarrierGroupActionTypes.Delete: {
      return { ...state, savingCarrierGroup: true, saveCarrierGroupSucceeded: false, saveCarrierGroupProblemDetails: null };
    }
    case LoadCarrierGroupActionTypes.Add_Failure:
    case LoadCarrierGroupActionTypes.Update_Failure: {
      return {
        ...state,
        savingCarrierGroup: false,
        saveCarrierGroupSucceeded: false,
        saveCarrierGroupProblemDetails: action.payload.error,
      };
    }
    case LoadCarrierGroupActionTypes.Delete_Failure:
    case LoadCarrierGroupActionTypes.Clear_Save_Succeeded: {
      return { ...state, savingCarrierGroup: false, saveCarrierGroupSucceeded: false, saveCarrierGroupProblemDetails: null };
    }

    case LoadCarrierGroupActionTypes.Add_Success: {
      return adapter.addOne(action.payload, {
        ...state,
        savingCarrierGroup: false,
        saveCarrierGroupSucceeded: true,
        saveCarrierGroupError: null,
      });
    }
    case LoadCarrierGroupActionTypes.Update_Success: {
      return adapter.updateOne(
        { id: action.payload.loadCarrierGroupId, changes: action.payload },
        { ...state, savingCarrierGroup: false, saveCarrierGroupSucceeded: true, saveCarrierGroupError: null }
      );
    }
    case LoadCarrierGroupActionTypes.Delete_Success: {
      return adapter.removeOne(action.payload.loadCarrierGroupId, {
        ...state,
        savingCarrierGroup: false,
        saveCarrierGroupSucceeded: true,
        saveCarrierGroupError: null,
      });
    }

    case LoadCarrierGroupActionTypes.Load_Group: {
      return { ...state, loadingSelectedCarrierGroup: true, saveCarrierGroupProblemDetails: null };
    }
    case LoadCarrierGroupActionTypes.Load_Group_Success: {
      if (action.payload && action.payload.loadCarrierGroupId > 0) {
        return adapter.upsertOne(action.payload, {
          ...state,
          selectedLoadCarrierGroup: action.payload,
          loadingSelectedCarrierGroup: false,
          saveCarrierGroupError: null,
        });
      }
      return {
        ...state,
        selectedLoadCarrierGroup: action.payload,
        loadingSelectedCarrierGroup: false,
        saveCarrierGroupProblemDetails: null,
      };
    }
    case LoadCarrierGroupActionTypes.Load_Group_Failure: {
      return { ...state, loadingSelectedCarrierGroup: false, saveCarrierGroupProblemDetails: null };
    }
    case LoadCarrierGroupActionTypes.Copy_Carriers_Load: {
      return { ...state, loadingSelectedCarrierGroup: true };
    }
    case LoadCarrierGroupActionTypes.Copy_Carriers_Load_Success: {
      return {
        ...state,
        selectedLoadCarrierGroup: { ...state.selectedLoadCarrierGroup, carriers: action.payload },
        loadingSelectedCarrierGroup: false,
        saveCarrierGroupProblemDetails: null,
      };
    }
    case LoadCarrierGroupActionTypes.Copy_Carriers_Load_Fail: {
      return { ...state, loadingSelectedCarrierGroup: false };
    }
    case LoadCarrierGroupActionTypes.Load_Carrier_Group_Types: {
      return { ...state, loadCarrierGroupTypesLoading: true, loadCarrierGroupTypes: [] };
    }
    case LoadCarrierGroupActionTypes.Load_Carrier_Group_Types_Success: {
      return { ...state, loadCarrierGroupTypesLoading: false, loadCarrierGroupTypes: action.payload };
    }
    case LoadCarrierGroupActionTypes.Load_Carrier_Group_Types_Failure: {
      return { ...state, loadCarrierGroupTypesLoading: false, loadCarrierGroupTypes: [] };
    }
    default:
      return state;
  }
}

export const selectors = adapter.getSelectors();
