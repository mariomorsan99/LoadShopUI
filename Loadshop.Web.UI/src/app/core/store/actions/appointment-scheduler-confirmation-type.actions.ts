import { Action } from '@ngrx/store';
import { AppointmentSchedulerConfirmationType } from '../../../shared/models';

export enum AppointmentSchedulerConfirmationTypeActionTypes {
    Load = '[AppointmentSchedulerConfirmationType] LOAD',
    Load_Success = '[AppointmentSchedulerConfirmationType] LOAD_SUCCESS',
    Load_Failure = '[AppointmentSchedulerConfirmationType] LOAD_FAILURE'
}

export class AppointmentSchedulerConfirmationTypeLoadAction implements Action {
    readonly type = AppointmentSchedulerConfirmationTypeActionTypes.Load;

    constructor() { }
}

export class AppointmentSchedulerConfirmationTypeLoadSuccessAction implements Action {
    readonly type = AppointmentSchedulerConfirmationTypeActionTypes.Load_Success;

    constructor(public payload: AppointmentSchedulerConfirmationType[]) { }
}

export class AppointmentSchedulerConfirmationTypeLoadFailureAction implements Action {
    readonly type = AppointmentSchedulerConfirmationTypeActionTypes.Load_Failure;

    constructor(public payload: Error) { }
}

export type AppointmentSchedulerConfirmationTypeActions =
    AppointmentSchedulerConfirmationTypeLoadAction |
    AppointmentSchedulerConfirmationTypeLoadSuccessAction |
    AppointmentSchedulerConfirmationTypeLoadFailureAction;
