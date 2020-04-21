import { AppointmentSchedulerConfirmationType } from '../../../shared/models';
import { AppointmentSchedulerConfirmationTypeActions, AppointmentSchedulerConfirmationTypeActionTypes } from '../actions';

export interface AppointmentSchedulerConfirmationTypeState {
  loading: boolean;
  entities: AppointmentSchedulerConfirmationType[];
}

const initialState: AppointmentSchedulerConfirmationTypeState = {
  loading: false,
  entities: [],
};

export function appointmentSchedulerConfirmationTypeReducer(
  state: AppointmentSchedulerConfirmationTypeState = initialState,
  action: AppointmentSchedulerConfirmationTypeActions
): AppointmentSchedulerConfirmationTypeState {
  switch (action.type) {
    case AppointmentSchedulerConfirmationTypeActionTypes.Load: {
      return { ...state, loading: true };
    }
    case AppointmentSchedulerConfirmationTypeActionTypes.Load_Success: {
      return { ...state, entities: action.payload, loading: false };
    }
    case AppointmentSchedulerConfirmationTypeActionTypes.Load_Failure: {
      return { ...state, loading: false };
    }
    default:
      return state;
  }
}

export const getLoading = (state: AppointmentSchedulerConfirmationTypeState) => state.loading;
export const getEntities = (state: AppointmentSchedulerConfirmationTypeState) => state.entities;
