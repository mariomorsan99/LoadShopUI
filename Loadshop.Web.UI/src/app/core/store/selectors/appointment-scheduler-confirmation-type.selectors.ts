import { createSelector } from '@ngrx/store';
import { CoreState } from '../reducers';
import { getEntities, getLoading } from '../reducers/appointment-scheduler-confirmation-type.reducer';

export const getAppointmentSchedulerConfirmationTypeState = (state: CoreState) => state.appointmentSchedulerConfirmationType;
export const getAppointmentSchedulerConfirmationTypes = createSelector(getAppointmentSchedulerConfirmationTypeState, getEntities);
export const getLoadingAppointmentSchedulerConfirmationTypes = createSelector(getAppointmentSchedulerConfirmationTypeState, getLoading);
